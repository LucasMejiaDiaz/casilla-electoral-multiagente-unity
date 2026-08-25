# Multi-Agent Wealth Distribution Simulation

> **Nota:** este README es provisional — documenta la demo que funciona hoy
> (un modelo Mesa de intercambio de riqueza). Es el punto de partida sobre
> el que se construirá la simulación de una casilla de votación INE; esa
> parte todavía no está implementada.

A Mesa-based wealth distribution simulation exposed through a Flask API, consumed by a C# console client, and visualized in Unity.

## Architecture

```text
Mesa simulation -> Flask REST API -> C# client
                              \-> Unity visualization
```

Each agent has an identifier, a 2D position, a wealth value, and a state describing its latest behavior. Every request to the API advances the simulation by one step.

## Project Structure

```text
backend/
  main.py                    Flask API and Mesa model
  requirements.txt           Python dependencies
client-csharp/
  Program.cs                 C# HTTP client
unity-client/
  MultiAgent-simulation/     Unity project
    Assets/Scripts/
      FlaskAgentClient.cs    Unity API integration
.gitignore
```

## Requirements

- Python 3.12+ (tested with 3.12.10)
- .NET 9 SDK
- Unity 6.4 or a compatible Unity 6 editor

## Run the Flask Backend

From the project root, create a virtual environment and install dependencies (only needed once):

```powershell
cd backend
python -m venv .venv
.\.venv\Scripts\python.exe -m pip install -r requirements.txt
```

Then start the server:

```powershell
.\.venv\Scripts\python.exe main.py
```

The API will be available at:

```text
http://127.0.0.1:5000/get_agents
```

Swagger documentation is available at:

```text
http://127.0.0.1:5000/apidocs
```

The `GET /get_agents` endpoint returns the current agents and advances the Mesa simulation by one step.

## Run the C# Client

Keep Flask running, open a second terminal, and execute:

```powershell
cd client-csharp
dotnet run
```

The client sends an HTTP request to Flask and prints the formatted JSON response, including each agent's position, wealth, and state.

## Run the Unity Integration

1. Open `unity-client/MultiAgent-simulation` in Unity.
2. Open `Assets/Scenes/SampleScene`.
3. Make sure the Flask backend is running.
4. Press **Play**.
5. Observe the agent spheres in the Game window.
6. Open the Unity Console to see each agent's behavior per simulation step.

Unity polls the Flask endpoint once per second, creates one sphere per agent, updates its position, and changes its color according to the agent's wealth.

## Example API Response

```json
{
  "step": 1,
  "agents": [
    {
      "id": 1,
      "x": 7,
      "y": 4,
      "wealth": 2,
      "state": "gave wealth to agent 6"
    }
  ]
}
```

## Verification

The following parts have been tested against a clean checkout (fresh venv, `pip install -r requirements.txt`, `dotnet build`):

- Flask returns HTTP 200 and agent data from `/get_agents`, and Swagger UI loads at `/apidocs`.
- The C# client builds and successfully retrieves and displays the API response.
- Unity compiles the integration script and displays the agents in the scene.

## AI Assistance Disclosure

AI tools were used to assist with code creation, debugging, documentation, and understanding the technology stack. The implementation was reviewed and tested by the student, who understands the main concepts involved: Mesa and Flask on the backend, HTTP communication with C#, and Unity integration using `UnityWebRequest`.
