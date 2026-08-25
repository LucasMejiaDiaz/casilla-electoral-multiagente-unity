"""Console demo of the casilla INE event-driven simulation core.

Schedules voter-arrival events on Mesa's built-in priority queue and runs
the simulated clock straight to completion via ``run_until`` (never a
``step()`` loop), so the printed timestamps show genuine event-driven time
jumps instead of fixed-tick advancement.
"""

import argparse
import logging

from casilla import CasillaModel


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Demo del motor de eventos + reloj simulado (casilla INE)."
    )
    parser.add_argument("--num-voters", type=int, default=20)
    parser.add_argument("--arrival-rate", type=float, default=1 / 3)
    parser.add_argument("--seed", type=int, default=None)
    args = parser.parse_args()

    logging.basicConfig(
        level=logging.INFO, format="[%(asctime)s] %(message)s", datefmt="%H:%M:%S"
    )

    model = CasillaModel(
        num_voters=args.num_voters,
        arrival_rate=args.arrival_rate,
        rng=args.seed,
    )

    horizon = (model.last_scheduled_arrival_time or 0.0) + 1.0
    model.run_until(horizon)

    logging.info(
        "Simulación terminada en t=%.2f (%d votantes procesados)",
        model.time,
        len(model.arrival_log),
    )


if __name__ == "__main__":
    main()
