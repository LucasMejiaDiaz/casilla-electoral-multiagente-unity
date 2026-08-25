import functools
import logging

import pytest
from mesa.time import Priority

from casilla import CasillaModel
from casilla.agents import Message, Station, VoterAgent


def _arrivals(model: CasillaModel) -> list[tuple[int, float]]:
    return [(e["voter"], e["time"]) for e in model.event_log if e["event"] == "ARRIVAL"]


# --- Core event scheduler (schedule_callback / run_until) ---------------


def test_events_execute_in_chronological_order_regardless_of_insertion_order():
    model = CasillaModel(num_voters=0, rng=1)
    calls: list[str] = []

    model.schedule_callback(functools.partial(calls.append, "a"), at=5.0)
    model.schedule_callback(functools.partial(calls.append, "b"), at=1.0)
    model.schedule_callback(functools.partial(calls.append, "c"), at=3.0)

    model.run_until(10.0)

    assert calls == ["b", "c", "a"]


def test_clock_advances_to_exact_event_time_not_fixed_ticks():
    model = CasillaModel(num_voters=0, rng=1)
    recorded: list[float] = []

    def record() -> None:
        recorded.append(model.time)

    for t in (0.5, 2.75, 10.1):
        model.schedule_callback(record, at=t)

    model.run_until(11.0)

    assert recorded == pytest.approx([0.5, 2.75, 10.1])


def test_same_timestamp_events_respect_priority_order():
    model = CasillaModel(num_voters=0, rng=1)
    calls: list[str] = []

    model.schedule_callback(functools.partial(calls.append, "low"), at=5.0, priority=Priority.LOW)
    model.schedule_callback(functools.partial(calls.append, "high"), at=5.0, priority=Priority.HIGH)

    model.run_until(6.0)

    assert calls == ["high", "low"]


def test_run_until_boundary_leaves_later_events_unexecuted():
    model = CasillaModel(num_voters=0, rng=1)
    calls: list[str] = []

    model.schedule_callback(functools.partial(calls.append, "a"), at=1.0)
    model.schedule_callback(functools.partial(calls.append, "b"), at=5.0)
    model.schedule_callback(functools.partial(calls.append, "c"), at=9.0)

    model.run_until(5.0)

    assert calls == ["a", "b"]
    assert model.time == 5.0
    assert model._event_list.peek_ahead(1)[0].time == 9.0

    model.run_until(9.0)

    assert calls == ["a", "b", "c"]


def test_bare_lambda_callback_is_rejected():
    model = CasillaModel(num_voters=0, rng=1)

    with pytest.raises(ValueError):
        model.schedule_event(lambda: None, at=1.0)


# --- Voter arrivals -------------------------------------------------------


def test_random_arrivals_reproducible_with_seeded_rng():
    model_a = CasillaModel(num_voters=10, arrival_rate=0.3, rng=42)
    model_b = CasillaModel(num_voters=10, arrival_rate=0.3, rng=42)

    horizon = model_a.last_scheduled_arrival_time + 0.01
    model_a.run_until(horizon)
    model_b.run_until(horizon)

    assert _arrivals(model_a) == _arrivals(model_b)


def test_arrival_logs_via_logging(caplog):
    model = CasillaModel(num_voters=0, rng=1)
    model.schedule_callback(model._on_voter_arrival, at=0.5)

    with caplog.at_level(logging.INFO):
        model.run_until(1.0)

    assert "Votante 1 llega en t=0.50" in caplog.text


def test_event_log_records_each_station_completion_in_order():
    model = CasillaModel(num_voters=0, rng=1)
    model.schedule_callback(model._on_voter_arrival, at=0.1)

    model.run_to_completion()

    events = [e["event"] for e in model.event_log]
    assert events == [
        "ARRIVAL",
        "SECRETARIO_DONE",
        "MESA_DONE",
        "CASILLA_DONE",
        "URNA_DONE",
        "EXIT",
    ]


# --- INE rejection branch --------------------------------------------------


def test_rejected_voter_exits_after_secretario_and_never_reaches_mesa():
    model = CasillaModel(num_voters=0, rng=1, rejection_rate=1.0)
    model.schedule_callback(model._on_voter_arrival, at=0.1)

    model.run_to_completion()

    events = [e["event"] for e in model.event_log]
    assert events == ["ARRIVAL", "SECRETARIO_DONE", "REJECTED"]

    voters = [a for a in model.agents if isinstance(a, VoterAgent)]
    assert len(voters) == 1
    assert voters[0].status == "rechazado"


def test_accepted_voter_reaches_exit_when_rejection_rate_is_zero():
    model = CasillaModel(num_voters=0, rng=1, rejection_rate=0.0)
    model.schedule_callback(model._on_voter_arrival, at=0.1)

    model.run_to_completion()

    events = [e["event"] for e in model.event_log]
    assert events == [
        "ARRIVAL",
        "SECRETARIO_DONE",
        "MESA_DONE",
        "CASILLA_DONE",
        "URNA_DONE",
        "EXIT",
    ]


def test_station_capacity_is_configurable_per_station():
    model = CasillaModel(
        num_voters=0,
        rng=1,
        secretario_capacity=2,
        mesa_capacity=3,
        casilla_capacity=1,
        urna_capacity=1,
    )

    assert model.secretario.capacity == 2
    assert model.mesa.capacity == 3
    assert model.casilla.capacity == 1
    assert model.urna.capacity == 1


# --- Station: capacity-limited FIFO resource -------------------------------


def test_station_queues_when_busy_and_serves_fifo_on_completion():
    model = CasillaModel(num_voters=0, rng=1)
    station = Station(model, "secretario", capacity=1, service_time_range=(1.0, 1.0))
    completed: list[tuple[int, float]] = []
    station.on_complete = lambda voter: completed.append((voter.number, model.time))

    v1 = VoterAgent(model, number=1)
    v2 = VoterAgent(model, number=2)
    station.request(v1)
    station.request(v2)

    assert station.busy == 1
    assert list(station.queue) == [v2]

    model.run_until(1.0)
    assert completed == [(1, 1.0)]
    assert list(station.queue) == []

    model.run_until(2.0)
    assert completed == [(1, 1.0), (2, 2.0)]
    assert station.busy == 0


def test_station_pause_blocks_new_starts_and_resume_releases_queue():
    model = CasillaModel(num_voters=0, rng=1)
    station = Station(model, "urna", capacity=1, service_time_range=(1.0, 1.0))
    completed: list[int] = []
    station.on_complete = lambda voter: completed.append(voter.number)

    station.receive_message(Message(sender="test", receiver=station, type="PAUSE", time=0.0))
    voter = VoterAgent(model, number=1)
    station.request(voter)

    assert station.busy == 0
    assert list(station.queue) == [voter]

    station.receive_message(Message(sender="test", receiver=station, type="RESUME", time=0.0))

    assert station.busy == 1
    assert list(station.queue) == []

    model.run_until(1.0)
    assert completed == [1]


# --- Coordinador: broadcasts the external event to every station ----------


def test_coordinador_broadcast_pauses_then_resumes_all_stations():
    model = CasillaModel(num_voters=0, rng=1)
    stations = [model.secretario, model.mesa, model.casilla, model.urna]

    message = Message(
        sender="entorno",
        receiver=model.coordinador,
        type="EXTERNAL_EVENT",
        payload={"kind": "temblor", "duration": 2.0},
        time=model.time,
    )
    model.coordinador.receive_message(message)

    assert all(s.paused for s in stations)

    model.run_until(2.0)

    assert all(not s.paused for s in stations)


def test_external_event_appears_once_in_event_log():
    model = CasillaModel(num_voters=5, arrival_rate=0.5, rng=7)

    model.run_to_completion()

    external_events = [e for e in model.event_log if e["event"] == "EXTERNAL_EVENT"]
    assert len(external_events) == 1
    assert external_events[0]["kind"] in {"corte_de_luz", "temblor", "aguacero"}
    assert 0 < external_events[0]["time"] < model.last_scheduled_arrival_time
