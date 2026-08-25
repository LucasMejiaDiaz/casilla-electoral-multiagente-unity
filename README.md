# Casilla INE Multi-Agent Simulation

> **Nota:** este README es provisional — documenta el motor de eventos y
> reloj simulado que funciona hoy, primer incremento de la simulación de una
> casilla de votación INE. Estaciones (secretario, mesa, casilla, urna),
> roles, el API Flask y la integración con Unity todavía no están
> implementados.

A Mesa-based discrete-event simulation core: voter-arrival events are scheduled on Mesa's built-in priority-queue scheduler and the simulated clock jumps directly from event to event (never fixed ticks). A C# console client and a Unity visualization exist but are temporarily inactive — they depended on a Flask API that will return in a later increment.

## Architecture

```text
Mesa event-driven engine (backend/casilla/) -> console log output
```

Each scheduled event has a simulated time and a callback. Running the model schedules a batch of voter arrivals with randomized inter-arrival times and processes them strictly in chronological order — the clock advances to each event's exact time, not by fixed steps.

## Project Structure

```text
backend/
  casilla/
    model.py                 CasillaModel: event queue + simulated clock core
  tests/
    test_casilla_model.py    pytest suite for the event engine
  main.py                    Console entry point (schedules arrivals, runs the clock)
  requirements.txt           Python dependencies
client-csharp/
  Program.cs                 C# HTTP client (temporarily inactive, see below)
unity-client/
  MultiAgent-simulation/     Unity project
    Assets/Scripts/
      FlaskAgentClient.cs    Unity API integration (temporarily inactive, see below)
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

## Run the Tests

```powershell
cd backend
.\.venv\Scripts\python.exe -m pytest -v
```

Verifies: events fire in chronological order regardless of scheduling order, the clock advances to each event's exact (non-integer) time rather than ticking, same-timestamp events respect priority as a tiebreak, `run_until()` correctly stops at its boundary and resumes correctly on a later call, seeded runs are reproducible, and arrivals are genuinely logged via `logging` (not just recorded in memory).

## Run the C# Client (temporarily inactive)

> The Flask API this client depends on was removed in this increment (see "Run the Simulation Demo" above). These instructions are preserved for when the Flask API returns in a future increment; running them now will fail to connect.

```powershell
cd client-csharp
dotnet run
```

The client sends an HTTP request to Flask and prints the formatted JSON response.

## Run the Unity Integration (temporarily inactive)

> The Flask API this integration depends on was removed in this increment. These instructions are preserved for when the Flask API returns in a future increment; running them now will show a connection error in the Game window.

1. Open `unity-client/MultiAgent-simulation` in Unity.
2. Open `Assets/Scenes/SampleScene`.
3. Press **Play**.
4. Unity polls the Flask endpoint once per second and would display agents once the API returns.

## Example Console Output

```text
[10:17:26] Votante 1 llega en t=1.17
[10:17:26] Votante 2 llega en t=1.66
[10:17:26] Votante 3 llega en t=4.82
...
[10:17:26] Simulación terminada en t=38.33 (20 votantes procesados)
```

## Verification

The following has been tested against a clean checkout for this increment (fresh venv, `pip install -r requirements.txt`):

- `python main.py` prints voter arrivals in strict chronological order with non-integer timestamps (proving event-driven, not fixed-tick, time advancement).
- `pytest -v` passes all 7 cases in `backend/tests/test_casilla_model.py`.

The C# client and Unity integration bullets below reflect verification from a prior increment against the now-removed Flask demo, not re-verified this round:

- The C# client builds and successfully retrieves and displays an API response.
- Unity compiles the integration script and displays agents in the scene.

## AI Assistance Disclosure

AI tools were used to assist with code creation, debugging, documentation, and understanding the technology stack. The implementation was reviewed and tested by the student, who understands the main concepts involved: Mesa's event-driven scheduling, discrete-event simulation, HTTP communication with C#, and Unity integration using `UnityWebRequest`.
