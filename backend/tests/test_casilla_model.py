import logging

import pytest
from mesa.time import Priority

from casilla import CasillaModel


def test_events_execute_in_chronological_order_regardless_of_insertion_order():
    model = CasillaModel(num_voters=0, rng=1)

    model.schedule_voter_arrival(100, at=5.0)
    model.schedule_voter_arrival(101, at=1.0)
    model.schedule_voter_arrival(102, at=3.0)

    model.run_until(10.0)

    assert model.arrival_log == [(101, 1.0), (102, 3.0), (100, 5.0)]


def test_clock_advances_to_exact_event_time_not_fixed_ticks():
    model = CasillaModel(num_voters=0, rng=1)

    model.schedule_voter_arrival(1, at=0.5)
    model.schedule_voter_arrival(2, at=2.75)
    model.schedule_voter_arrival(3, at=10.1)

    model.run_until(11.0)

    times = [t for _, t in model.arrival_log]
    assert times == pytest.approx([0.5, 2.75, 10.1])


def test_same_timestamp_events_respect_priority_order():
    model = CasillaModel(num_voters=0, rng=1)

    model.schedule_voter_arrival(1, at=5.0, priority=Priority.LOW)
    model.schedule_voter_arrival(2, at=5.0, priority=Priority.HIGH)

    model.run_until(6.0)

    assert model.arrival_log == [(2, 5.0), (1, 5.0)]


def test_run_until_boundary_leaves_later_events_unexecuted():
    model = CasillaModel(num_voters=0, rng=1)

    model.schedule_voter_arrival(1, at=1.0)
    model.schedule_voter_arrival(2, at=5.0)
    model.schedule_voter_arrival(3, at=9.0)

    model.run_until(5.0)

    assert len(model.arrival_log) == 2
    assert model.time == 5.0
    assert model._event_list.peek_ahead(1)[0].time == 9.0

    model.run_until(9.0)

    assert len(model.arrival_log) == 3


def test_random_arrivals_reproducible_with_seeded_rng():
    model_a = CasillaModel(num_voters=10, arrival_rate=0.3, rng=42)
    model_b = CasillaModel(num_voters=10, arrival_rate=0.3, rng=42)

    horizon = max(model_a.last_scheduled_arrival_time, model_b.last_scheduled_arrival_time) + 1.0
    model_a.run_until(horizon)
    model_b.run_until(horizon)

    assert model_a.arrival_log == model_b.arrival_log


def test_bare_lambda_callback_is_rejected():
    model = CasillaModel(num_voters=0, rng=1)

    with pytest.raises(ValueError):
        model.schedule_event(lambda: None, at=1.0)


def test_arrival_logs_via_logging(caplog):
    model = CasillaModel(num_voters=0, rng=1)
    model.schedule_voter_arrival(1, at=0.5)

    with caplog.at_level(logging.INFO):
        model.run_until(1.0)

    assert "Votante 1 llega en t=0.50" in caplog.text
