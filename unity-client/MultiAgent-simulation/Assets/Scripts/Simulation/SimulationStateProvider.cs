using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace PollingStation.Simulation
{
    public sealed class SimulationStateProvider : MonoBehaviour
    {
        [SerializeField] private string endpoint = "http://127.0.0.1:5000/get_agents";
        [SerializeField, Min(0.1f)] private float refreshInterval = 1f;
        [SerializeField] private bool useMockData = true;
        [SerializeField] private AgentViewManager viewManager;

        private int mockStep;
        private float mockTime;

        public void Configure(AgentViewManager manager, string apiEndpoint, bool mockMode)
        {
            viewManager = manager;
            endpoint = apiEndpoint;
            useMockData = mockMode;
        }

        private IEnumerator Start()
        {
            while (true)
            {
                if (useMockData)
                {
                    ApplyMockSnapshot();
                }
                else
                {
                    yield return FetchSnapshot();
                }

                yield return new WaitForSecondsRealtime(refreshInterval);
            }
        }

        private IEnumerator FetchSnapshot()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(endpoint))
            {
                request.timeout = 5;
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    viewManager?.SetConnectionStatus($"Sin conexión: {request.error}", false);
                    yield break;
                }

                SimulationSnapshot snapshot = null;
                try
                {
                    snapshot = JsonUtility.FromJson<SimulationSnapshot>(request.downloadHandler.text);
                }
                catch (System.ArgumentException exception)
                {
                    viewManager?.SetConnectionStatus($"JSON inválido: {exception.Message}", false);
                }

                if (snapshot == null || snapshot.agents == null)
                {
                    viewManager?.SetConnectionStatus("Respuesta sin arreglo de agentes", false);
                    yield break;
                }

                viewManager?.SetConnectionStatus("Conectado al backend", true);
                viewManager?.ApplySnapshot(snapshot);
            }
        }

        private void ApplyMockSnapshot()
        {
            mockStep++;
            mockTime += 2.5f;
            bool eventActive = mockStep % 30 >= 16 && mockStep % 30 <= 20;
            List<AgentSnapshot> agents = new List<AgentSnapshot>();
            string[] stages =
            {
                "arrived",
                "esperando_secretario",
                "en_secretario",
                "esperando_mesa",
                "en_mesa",
                "esperando_casilla",
                "en_casilla",
                "esperando_urna",
                "en_urna",
                "salio"
            };

            for (int id = 1; id <= 18; id++)
            {
                int stageIndex = mockStep - id;
                if (stageIndex < 0 || stageIndex > stages.Length + 1) continue;

                string state;
                if (id % 9 == 0 && stageIndex >= 3)
                {
                    if (stageIndex > 4) continue;
                    state = "rechazado";
                }
                else
                {
                    if (stageIndex >= stages.Length) continue;
                    state = stages[stageIndex];
                }

                agents.Add(new AgentSnapshot
                {
                    id = id,
                    state = state,
                    station = StationFromState(state),
                    queue_position = state.StartsWith("esperando_") ? id % 6 : -1
                });
            }

            SimulationSnapshot snapshot = new SimulationSnapshot
            {
                step = mockStep,
                simulation_time = mockTime,
                running = true,
                paused = eventActive,
                external_event = new ExternalEventSnapshot
                {
                    active = eventActive,
                    kind = eventActive ? "corte_de_luz" : "",
                    remaining = eventActive ? (21 - mockStep % 30) * 2.5f : 0f
                },
                agents = agents.ToArray()
            };

            viewManager?.SetConnectionStatus("Modo demostración (sin backend)", true);
            viewManager?.ApplySnapshot(snapshot);
        }

        private static string StationFromState(string state)
        {
            if (state.Contains("secretario")) return "secretario";
            if (state.Contains("mesa")) return "mesa";
            if (state.Contains("casilla")) return "casilla";
            if (state.Contains("urna")) return "urna";
            return "";
        }
    }
}
