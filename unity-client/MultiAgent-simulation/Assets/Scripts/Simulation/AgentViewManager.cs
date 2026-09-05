using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PollingStation.Simulation
{
    public sealed class AgentViewManager : MonoBehaviour
    {
        [SerializeField] private SceneLayout layout;
        [SerializeField] private AgentView voterPrefab;
        [SerializeField] private Transform agentContainer;
        [SerializeField] private ExternalEventVisualizer externalEventVisualizer;
        [SerializeField] private Text connectionText;
        [SerializeField] private Text clockText;
        [SerializeField] private Text countersText;
        [SerializeField] private bool smoothMovement;
        [SerializeField] private float movementSpeed = 4f;

        private readonly Dictionary<int, AgentView> activeViews = new Dictionary<int, AgentView>();
        private readonly Stack<AgentView> pool = new Stack<AgentView>();
        private readonly Dictionary<int, int> terminalFrames = new Dictionary<int, int>();
        private readonly HashSet<int> suppressedTerminalIds = new HashSet<int>();
        private readonly HashSet<string> warnedStates = new HashSet<string>();

        public int ActiveAgentCount => activeViews.Count;

        public void Configure(
            SceneLayout sceneLayout,
            AgentView prefab,
            Transform container,
            ExternalEventVisualizer eventVisualizer,
            Text connection,
            Text clockLabel,
            Text counters,
            bool useSmoothMovement)
        {
            layout = sceneLayout;
            voterPrefab = prefab;
            agentContainer = container;
            externalEventVisualizer = eventVisualizer;
            connectionText = connection;
            clockText = clockLabel;
            countersText = counters;
            smoothMovement = useSmoothMovement;
        }

        public void SetConnectionStatus(string value, bool connected)
        {
            if (connectionText == null) return;
            connectionText.text = value;
            connectionText.color = connected ? new Color(0.24f, 0.80f, 0.42f) : new Color(0.95f, 0.35f, 0.28f);
        }

        public void ApplySnapshot(SimulationSnapshot snapshot)
        {
            if (snapshot == null || layout == null)
            {
                return;
            }

            AgentSnapshot[] agents = snapshot.agents ?? Array.Empty<AgentSnapshot>();
            Array.Sort(agents, (left, right) => left.id.CompareTo(right.id));

            HashSet<int> receivedIds = new HashSet<int>();
            Dictionary<string, int> stateOrdinals = new Dictionary<string, int>();
            Dictionary<string, HashSet<int>> usedQueuePositions = new Dictionary<string, HashSet<int>>();
            int waiting = 0;
            int inService = 0;
            int rejected = 0;
            int exited = 0;

            foreach (AgentSnapshot agent in agents)
            {
                if (agent == null) continue;
                receivedIds.Add(agent.id);
                string state = agent.EffectiveState;
                bool terminal = IsTerminal(state);

                if (!IsKnownState(state) && warnedStates.Add(state))
                {
                    Debug.LogWarning($"Estado de agente desconocido '{state}'. Se usará el punto Fallback.", this);
                }

                if (terminal && suppressedTerminalIds.Contains(agent.id))
                {
                    continue;
                }

                int ordinal = NextOrdinal(stateOrdinals, state);
                int queueOrdinal = ResolveQueueOrdinal(agent, state, ordinal, usedQueuePositions);
                bool created = !activeViews.TryGetValue(agent.id, out AgentView view);
                if (created)
                {
                    view = Acquire(agent.id);
                }

                Vector3 target = layout.ResolveTarget(agent, ordinal, queueOrdinal);
                view.SetPaused(snapshot.paused);
                view.ApplyState(state, target, !smoothMovement);

                if (terminal)
                {
                    int framesLeft = terminalFrames.TryGetValue(agent.id, out int existing) ? existing : 2;
                    framesLeft--;
                    terminalFrames[agent.id] = framesLeft;
                    if (framesLeft <= 0)
                    {
                        Release(agent.id);
                        suppressedTerminalIds.Add(agent.id);
                    }
                }
                else
                {
                    terminalFrames.Remove(agent.id);
                }

                if (state.StartsWith("esperando_")) waiting++;
                else if (state.StartsWith("en_")) inService++;
                else if (state == "rechazado" || state == "rejected") rejected++;
                else if (state == "salio" || state == "exit") exited++;
            }

            List<int> missing = new List<int>();
            foreach (int id in activeViews.Keys)
            {
                if (!receivedIds.Contains(id)) missing.Add(id);
            }
            foreach (int id in missing) Release(id);

            suppressedTerminalIds.RemoveWhere(id => !receivedIds.Contains(id));

            if (clockText != null)
            {
                clockText.text = $"Tiempo simulado: {snapshot.simulation_time:0.0} min  |  Paso: {snapshot.step}";
            }
            if (countersText != null)
            {
                countersText.text = $"Activos: {activeViews.Count}   Esperando: {waiting}   En atención: {inService}   Rechazados: {rejected}   Salidas: {exited}";
            }

            externalEventVisualizer?.Apply(snapshot.external_event, snapshot.paused);
        }

        private AgentView Acquire(int id)
        {
            AgentView view;
            if (pool.Count > 0)
            {
                view = pool.Pop();
                view.gameObject.SetActive(true);
            }
            else if (voterPrefab != null)
            {
                view = Instantiate(voterPrefab, agentContainer);
            }
            else
            {
                GameObject fallbackObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                fallbackObject.transform.SetParent(agentContainer, false);
                view = fallbackObject.AddComponent<AgentView>();
            }

            view.Initialize(id, movementSpeed);
            if (layout.entrance != null) view.transform.position = layout.entrance.position;
            activeViews[id] = view;
            return view;
        }

        private void Release(int id)
        {
            if (!activeViews.TryGetValue(id, out AgentView view)) return;
            activeViews.Remove(id);
            terminalFrames.Remove(id);
            view.gameObject.SetActive(false);
            pool.Push(view);
        }

        private static int NextOrdinal(Dictionary<string, int> counters, string state)
        {
            int current = counters.TryGetValue(state, out int value) ? value : 0;
            counters[state] = current + 1;
            return current;
        }

        private static int ResolveQueueOrdinal(
            AgentSnapshot agent,
            string state,
            int fallback,
            Dictionary<string, HashSet<int>> used)
        {
            if (!state.StartsWith("esperando_")) return fallback;
            if (!used.TryGetValue(state, out HashSet<int> positions))
            {
                positions = new HashSet<int>();
                used[state] = positions;
            }

            int candidate = agent.queue_position;
            if (candidate < 0 || positions.Contains(candidate))
            {
                candidate = 0;
                while (positions.Contains(candidate)) candidate++;
            }
            positions.Add(candidate);
            return candidate;
        }

        private static bool IsTerminal(string state)
        {
            return state == "salio" || state == "exit" || state == "rechazado" || state == "rejected";
        }

        private static bool IsKnownState(string state)
        {
            switch (state)
            {
                case "arrived":
                case "esperando_secretario":
                case "en_secretario":
                case "esperando_mesa":
                case "en_mesa":
                case "esperando_casilla":
                case "en_casilla":
                case "esperando_urna":
                case "en_urna":
                case "rechazado":
                case "rejected":
                case "salio":
                case "exit":
                    return true;
                default:
                    return false;
            }
        }
    }
}
