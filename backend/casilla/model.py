"""Event-driven core of the casilla INE simulation.

Orchestrates voter arrivals, chains them through the secretario -> mesa ->
casilla -> urna stations (each a capacity-limited FIFO resource), and
injects one external event that pauses every station for a while. All of
it rides on Mesa's built-in priority-queue event scheduler
(``Model.schedule_event`` / ``run_until``), never a fixed-tick loop.
"""

from __future__ import annotations

import functools
import logging
from typing import Any

from mesa import Model
from mesa.time import Event, Priority

from .agents import Coordinador, Message, Station, VoterAgent

logger = logging.getLogger(__name__)

# Simulated minutes, matching CONTEXT.md's station timings.
STATION_SERVICE_TIMES: dict[str, tuple[float, float]] = {
    "secretario": (1.5, 2.5),
    "mesa": (0.5, 1.5),
    "casilla": (2.0, 4.0),
    "urna": (0.2, 0.6),
}

EXTERNAL_EVENT_KINDS = ["corte_de_luz", "temblor", "aguacero"]


class CasillaModel(Model):
    """Schedules voter arrivals and runs them through the station chain."""

    def __init__(
        self,
        num_voters: int = 200,
        arrival_rate: float = 1 / 3,
        *,
        secretario_capacity: int = 1,
        mesa_capacity: int = 1,
        casilla_capacity: int = 1,
        urna_capacity: int = 1,
        rng: int | None = None,
    ) -> None:
        super().__init__(rng=rng)

        # Model.__init__ always starts a hidden recurring event that fires
        # every 1.0 time unit to support the legacy step() API. Left
        # running, it logs "Step N at time T" noise and adds unrelated
        # events to the queue for a model that never calls step(). No public
        # API exists in Mesa 3.5.1 to opt out of it at construction time.
        self._default_schedule.stop()

        self._scheduled_callbacks: list[Any] = []
        self._voter_counter = 0
        self.event_log: list[dict] = []
        self.last_scheduled_arrival_time: float | None = None

        self.secretario = Station(
            self,
            "secretario",
            capacity=secretario_capacity,
            service_time_range=STATION_SERVICE_TIMES["secretario"],
        )
        self.mesa = Station(
            self,
            "mesa",
            capacity=mesa_capacity,
            service_time_range=STATION_SERVICE_TIMES["mesa"],
        )
        self.casilla = Station(
            self,
            "casilla",
            capacity=casilla_capacity,
            service_time_range=STATION_SERVICE_TIMES["casilla"],
        )
        self.urna = Station(
            self,
            "urna",
            capacity=urna_capacity,
            service_time_range=STATION_SERVICE_TIMES["urna"],
        )
        self.coordinador = Coordinador(
            self, stations=[self.secretario, self.mesa, self.casilla, self.urna]
        )

        self.secretario.on_complete = lambda voter: self.mesa.request(voter)
        self.mesa.on_complete = lambda voter: self.casilla.request(voter)
        self.casilla.on_complete = lambda voter: self.urna.request(voter)
        self.urna.on_complete = self._on_exit

        self._schedule_arrivals(num_voters, arrival_rate)
        self._schedule_external_event()

    def schedule_callback(
        self,
        fn,
        *,
        at: float | None = None,
        after: float | None = None,
        priority: Priority = Priority.DEFAULT,
    ) -> Event:
        """``schedule_event``, but keeps a strong reference to ``fn`` alive.

        Mesa's ``Event`` holds callbacks via weak reference; a
        ``functools.partial`` passed inline with no other strong reference
        is garbage-collected before it fires. Every caller with a
        parameterized callback should go through this instead of calling
        ``schedule_event`` directly.
        """
        self._scheduled_callbacks.append(fn)
        return self.schedule_event(fn, at=at, after=after, priority=priority)

    def run_to_completion(self) -> None:
        """Advance the clock event by event until the queue is empty."""
        while not self._event_list.is_empty():
            next_time = self._event_list.peek_ahead(1)[0].time
            self.run_until(next_time)

    def _schedule_arrivals(self, num_voters: int, arrival_rate: float) -> None:
        time = 0.0
        for _ in range(num_voters):
            time += self.random.expovariate(arrival_rate)
            self.schedule_callback(self._on_voter_arrival, at=time)
            self.last_scheduled_arrival_time = time

    def _on_voter_arrival(self) -> None:
        self._voter_counter += 1
        voter = VoterAgent(self, number=self._voter_counter)
        self.event_log.append(
            {"event": "ARRIVAL", "voter": voter.number, "time": self.time}
        )
        logger.info("Votante %s llega en t=%.2f", voter.number, self.time)
        self.secretario.request(voter)

    def _on_exit(self, voter: VoterAgent) -> None:
        self.event_log.append(
            {"event": "EXIT", "voter": voter.number, "time": self.time}
        )
        logger.info("Votante %s EXITS en t=%.2f", voter.number, self.time)

    def _schedule_external_event(self) -> None:
        if self.last_scheduled_arrival_time is None:
            return
        trigger_time = self.random.uniform(
            0.25 * self.last_scheduled_arrival_time,
            0.75 * self.last_scheduled_arrival_time,
        )
        kind = self.random.choice(EXTERNAL_EVENT_KINDS)
        duration = self.random.uniform(3.0, 10.0)
        self.schedule_callback(
            functools.partial(self._trigger_external_event, kind, duration),
            at=trigger_time,
        )

    def _trigger_external_event(self, kind: str, duration: float) -> None:
        self.event_log.append(
            {
                "event": "EXTERNAL_EVENT",
                "kind": kind,
                "duration": duration,
                "time": self.time,
            }
        )
        logger.info(
            "EVENTO EXTERNO: %s en t=%.2f (dura %.2f min)", kind, self.time, duration
        )
        message = Message(
            sender="entorno",
            receiver=self.coordinador,
            type="EXTERNAL_EVENT",
            payload={"kind": kind, "duration": duration},
            time=self.time,
        )
        self.coordinador.receive_message(message)
