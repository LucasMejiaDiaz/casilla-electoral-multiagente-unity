using System;

namespace PollingStation.Simulation
{
    [Serializable]
    public sealed class ExternalEventSnapshot
    {
        public bool active;
        public string kind = "";
        public float remaining;
    }

    [Serializable]
    public sealed class AgentSnapshot
    {
        public int id;
        public string state = "";
        public string status = "";
        public string station = "";
        public int queue_position = -1;

        public string EffectiveState
        {
            get
            {
                string value = string.IsNullOrWhiteSpace(state) ? status : state;
                return string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim().ToLowerInvariant();
            }
        }
    }

    [Serializable]
    public sealed class SimulationSnapshot
    {
        public int step;
        public float simulation_time;
        public bool running;
        public bool paused;
        public ExternalEventSnapshot external_event = new ExternalEventSnapshot();
        public AgentSnapshot[] agents = Array.Empty<AgentSnapshot>();
    }
}
