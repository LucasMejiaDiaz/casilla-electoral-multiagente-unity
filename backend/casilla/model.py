"""Event-driven core of the casilla INE simulation.

Uses Mesa's built-in priority-queue event scheduler (``Model.schedule_event``
/ ``run_until``) instead of Mesa's per-tick scheduler, so the simulated
clock jumps directly to each event's time instead of advancing in fixed
ticks.
"""

from __future__ import annotations

import functools
import logging
from typing import Any

from mesa import Model
from mesa.time import Event, Priority

logger = logging.getLogger(__name__)


class CasillaModel(Model):
    """Schedules voter arrivals on Mesa's event queue and lets them fire."""

    def __init__(
        self,
        num_voters: int = 20,
        arrival_rate: float = 1 / 3,
        *,
        rng: int | None = None,
    ) -> None:
        super().__init__(rng=rng)

        # Model.__init__ always starts a hidden recurring event that fires
        # every 1.0 time unit to support the legacy step() API. Left
        # running, it logs "Step N at time T" noise and adds unrelated
        # events to the queue for a model that never calls step(). No public
        # API exists in Mesa 3.5.1 to opt out of it at construction time.
        self._default_schedule.stop()

        self.arrival_log: list[tuple[int, float]] = []
        self._scheduled_callbacks: list[Any] = []
        self.last_scheduled_arrival_time: float | None = None

        self._schedule_arrivals(num_voters, arrival_rate)

    def schedule_voter_arrival(
        self,
        voter_id: int,
        at: float,
        priority: Priority = Priority.DEFAULT,
    ) -> Event:
        """Schedule a single voter-arrival event at an absolute time."""
        callback = functools.partial(self._on_voter_arrival, voter_id)
        # Event callbacks are held by weak reference; an inline partial with
        # no other strong reference gets garbage-collected before the event
        # fires. Keeping it here keeps it alive until it runs.
        self._scheduled_callbacks.append(callback)
        return self.schedule_event(callback, at=at, priority=priority)

    def _on_voter_arrival(self, voter_id: int) -> None:
        self.arrival_log.append((voter_id, self.time))
        logger.info("Votante %s llega en t=%.2f", voter_id, self.time)

    def _schedule_arrivals(self, num_voters: int, arrival_rate: float) -> None:
        time = 0.0
        for voter_id in range(1, num_voters + 1):
            time += self.random.expovariate(arrival_rate)
            self.schedule_voter_arrival(voter_id, at=time)
            self.last_scheduled_arrival_time = time
