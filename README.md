# Casilla INE Multi-Agent Simulation

> **Nota:** este README es provisional — documenta el motor de eventos, las
> estaciones (secretario, mesa, casilla, urna), el agente votante, el
> coordinador, el evento externo, la capacidad configurable por estación y
> el rechazo de INE inválida que funcionan hoy. Grid espacial, el API Flask
> y la integración con Unity todavía no están implementados.

A Mesa-based discrete-event simulation of a casilla de votación INE: voter
agents arrive as a Poisson process and move through a chain of
capacity-limited stations — secretario → mesa → casilla → urna — each with
its own FIFO wait queue and randomized service time. All hand-offs (a
station giving a voter their turn or telling them to wait, the coordinator
pausing/resuming a station) are explicit `Message` objects, not bare method
calls. A random external event (corte de luz, temblor, aguacero) fires once
per run and is broadcast by a `Coordinador` agent to pause every station for
a while. The simulated clock is Mesa's built-in priority-queue event
scheduler — it jumps directly from event to event, never fixed ticks. A C#
console client and a Unity visualization exist but are temporarily inactive
— they depended on a Flask API that will return in a later increment.

## How the Simulation Works

```text
Voter arrival (Poisson, --arrival-rate)
        |
        v
  secretario --TURN/WAIT--> mesa --TURN/WAIT--> casilla --TURN/WAIT--> urna --> exit
   (1.5-2.5 min)   |      (0.5-1.5 min)        (2.0-4.0 min)        (0.2-0.6 min)
   each: configurable capacity (default 1), FIFO queue, random (uniform) service time
                   | --rejection-rate (default 2%)
                   v
              REJECTED --> voter leaves the system (never reaches mesa)

External event (random time, random kind: corte_de_luz | temblor | aguacero)
        |
        v
  Coordinador --PAUSE broadcast--> secretario, mesa, casilla, urna
        | (after a random duration)
        v
  Coordinador --RESUME broadcast--> secretario, mesa, casilla, urna
```

- Every station is a `Station(mesa.Agent)` instance with its own capacity
  (default 1, configurable per station), FIFO `deque` queue, and
  service-time range in simulated minutes. A busy station queues incoming
  voters; freeing up pulls the next one from the queue automatically.
- Voters are `VoterAgent(mesa.Agent)` instances that react to `Message`s
  (`TURN`, `WAIT`, `REJECTED`) sent by whichever station is currently
  handling them.
- After the secretario finishes with a voter, there's a `--rejection-rate`
  chance (default 2%) their INE is rejected: the model sends the voter a
  `REJECTED` message, logs a `REJECTED` entry in `event_log`, and the voter
  leaves the system right there — it never reaches mesa/casilla/urna.
  Otherwise the voter continues down the chain as normal.
- The external event is scheduled by the model itself at a random point
  within the run (not injected from outside), with a random kind and a
  random 3-10 minute duration. The `Coordinador` is the single place that
  broadcasts to all four stations — this is the actual inter-agent
  communication the event triggers, not the event by itself.
- `model.event_log` records `ARRIVAL`, `EXIT`, and `EXTERNAL_EVENT` entries
  with simulated timestamps; every message send/receive and station
  start/stop is also logged via `logging` (not `print`).
- The clock is driven event-by-event to completion with
  `model.run_to_completion()` — never a `step()` loop.

## Project Structure

```text
backend/
  casilla/
    __init__.py               Exports CasillaModel
    model.py                  CasillaModel: builds stations/coordinador, schedules arrivals + external event
    agents.py                 VoterAgent, Station, Coordinador, Message
  tests/
    conftest.py                sys.path shim so `import casilla` resolves
    test_casilla_model.py      pytest suite (scheduler, arrivals, stations, coordinador)
  main.py                      Console entry point (schedules arrivals, runs the clock to completion)
  requirements.txt             Python dependencies
client-csharp/
  Program.cs                   C# HTTP client (temporarily inactive, see below)
unity-client/
  MultiAgent-simulation/       Unity project
    Assets/Scripts/
      FlaskAgentClient.cs      Unity API integration (temporarily inactive, see below)
.gitignore
```

## Requirements

- Python 3.12+ (tested with 3.12.10)
- .NET 9 SDK (only needed for the currently-inactive C# client)
- Unity 6.4 or a compatible Unity 6 editor (only needed for the currently-inactive Unity integration)

## Run the Simulation Demo

From the project root, create a virtual environment and install dependencies (only needed once):

```powershell
cd backend
python -m venv .venv
.\.venv\Scripts\python.exe -m pip install -r requirements.txt
```

Then run the demo:

```powershell
.\.venv\Scripts\python.exe main.py
```

Optional flags:

```powershell
.\.venv\Scripts\python.exe main.py --num-voters 30 --arrival-rate 0.5 --seed 7
```

- `--num-voters` — how many voter-arrival events to schedule (default 20).
- `--arrival-rate` — average arrivals per simulated minute (default 1/3, i.e. ~1 every 3 minutes).
- `--seed` — integer seed for reproducible runs (default: unseeded/random).
- `--secretario-capacity`, `--mesa-capacity`, `--casilla-capacity`, `--urna-capacity` — how many voters each station can serve at once (default 1 each). Raise these to relieve the bottleneck at scale (e.g. `--casilla-capacity 3` for a 1400-voter run).
- `--rejection-rate` — probability (0-1) that a voter's INE is rejected by the secretario, ending their run right there (default 0.02, i.e. 2%).

## Run the Tests

```powershell
cd backend
.\.venv\Scripts\python.exe -m pytest -v
```

Verifies, across 15 cases: the core scheduler (chronological order
regardless of insertion order, exact non-integer event times, same-timestamp
priority tiebreaks, `run_until()` boundary/resume behavior, the
weak-reference restriction on bare lambdas); voter arrivals (Poisson
inter-arrival times reproducible with a seed, arrivals logged via
`logging`, one `event_log` entry per station completion in chain order);
the INE rejection branch (a rejected voter stops after `SECRETARIO_DONE`
and never reaches mesa, an accepted voter reaches `EXIT`); configurable
per-station capacity; a `Station` in isolation (FIFO queueing under
capacity, `PAUSE`/`RESUME` blocking and releasing the queue); and the
`Coordinador` (broadcasting `PAUSE`/`RESUME` to all four stations, the
external event appearing exactly once in `event_log` with a valid kind and
trigger time).

## Run the C# Client (temporarily inactive)

> The Flask API this client depends on was removed in an earlier increment (see "Run the Simulation Demo" above). These instructions are preserved for when the Flask API returns in a future increment; running them now will fail to connect.

```powershell
cd client-csharp
dotnet run
```

The client sends an HTTP request to Flask and prints the formatted JSON response.

## Run the Unity Integration (temporarily inactive)

> The Flask API this integration depends on was removed in an earlier increment. These instructions are preserved for when the Flask API returns in a future increment; running them now will show a connection error in the Game window.

1. Open `unity-client/MultiAgent-simulation` in Unity.
2. Open `Assets/Scenes/SampleScene`.
3. Press **Play**.
4. Unity polls the Flask endpoint once per second and would display agents once the API returns.

## Example Console Output

```text
[16:22:29] Votante 1 llega en t=0.54
[16:22:29] secretario envia TURN a votante 1 en t=0.54
[16:22:29] Votante 1 recibe TURN de secretario en t=0.54
[16:22:29] EVENTO EXTERNO: temblor en t=1.79 (dura 4.82 min)
[16:22:29] Coordinador recibe EXTERNAL_EVENT (temblor, dura 4.82 min) en t=1.79
[16:22:29] Coordinador envia PAUSE a secretario en t=1.79
[16:22:29] secretario recibe PAUSE de Coordinador en t=1.79
...
[16:22:29] Votante 2 llega en t=2.12
[16:22:29] secretario envia WAIT a votante 2 en t=2.12
[16:22:29] Votante 2 recibe WAIT de secretario en t=2.12
...
[16:22:29] Coordinador envia RESUME a secretario en t=6.61
[16:22:29] secretario recibe RESUME de Coordinador en t=6.61
[16:22:29] secretario envia TURN a votante 2 en t=6.61
...
[16:22:29] urna termina con votante 1 en t=11.80
[16:22:29] Votante 1 EXITS en t=11.80
...
[16:22:29] Simulación terminada en t=28.72 (6 votantes procesados)
```

## Verification

The following has been tested against a clean checkout for this increment (fresh venv, `pip install -r requirements.txt`):

- `python main.py` runs to completion, chains every voter through
  secretario → mesa → casilla → urna → exit in strict chronological order,
  and shows a `corte_de_luz`/`temblor`/`aguacero` event pausing all four
  stations and resuming them later (verified with `--seed 7` and `--seed 3`
  runs).
- `pytest -v` passes all 15 cases in `backend/tests/test_casilla_model.py`.

The C# client and Unity integration bullets below reflect verification from a prior increment against the now-removed Flask demo, not re-verified this round:

- The C# client builds and successfully retrieves and displays an API response.
- Unity compiles the integration script and displays agents in the scene.

## AI Assistance Disclosure

AI tools were used to assist with code creation, debugging, documentation, and understanding the technology stack. The implementation was reviewed and tested by the student, who understands the main concepts involved: Mesa's event-driven scheduling, discrete-event simulation, inter-agent messaging, HTTP communication with C#, and Unity integration using `UnityWebRequest`.
