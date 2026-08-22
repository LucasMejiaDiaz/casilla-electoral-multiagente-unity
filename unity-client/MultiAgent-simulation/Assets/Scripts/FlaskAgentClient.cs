using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FlaskAgentClient : MonoBehaviour
{
    [SerializeField] private string endpoint = "http://127.0.0.1:5000/get_agents";
    [SerializeField] private float refreshInterval = 1f;
    [SerializeField] private float boardSize = 10f;
    [SerializeField] private float agentHeight = 0.5f;
    [SerializeField] private bool logAgentBehavior = true;

    private readonly Dictionary<int, GameObject> agentObjects = new Dictionary<int, GameObject>();
    private string status = "Conectando con Flask...";
    private int currentStep;

    private void Start()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(boardSize / 2f, boardSize * 1.25f, -boardSize * 0.8f);
            mainCamera.transform.LookAt(new Vector3(boardSize / 2f, 0f, boardSize / 2f));
        }

        StartCoroutine(PollAgents());
    }

    private IEnumerator PollAgents()
    {
        while (true)
        {
            yield return FetchAgents();
            yield return new WaitForSeconds(refreshInterval);
        }
    }

    private IEnumerator FetchAgents()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(endpoint))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                status = "Error: " + request.error;
                yield break;
            }

            AgentResponse response;
            try
            {
                response = JsonUtility.FromJson<AgentResponse>(request.downloadHandler.text);
            }
            catch (System.ArgumentException exception)
            {
                status = "JSON inválido: " + exception.Message;
                yield break;
            }

            if (response == null || response.agents == null)
            {
                status = "La respuesta no contiene agentes.";
                yield break;
            }

            currentStep = response.step;
            status = "Conectado | agentes: " + response.agents.Length;
            UpdateAgents(response.agents);

            if (logAgentBehavior)
            {
                LogAgentBehavior(response.agents);
            }
        }
    }

    private void LogAgentBehavior(AgentData[] agents)
    {
        foreach (AgentData agent in agents)
        {
            Debug.Log($"[Mesa step {currentStep}] Agent {agent.id} | " +
                      $"position=({agent.x}, {agent.y}) | wealth={agent.wealth} | state={agent.state}", this);
        }
    }

    private void UpdateAgents(AgentData[] agents)
    {
        HashSet<int> activeIds = new HashSet<int>();

        foreach (AgentData agent in agents)
        {
            activeIds.Add(agent.id);

            if (!agentObjects.TryGetValue(agent.id, out GameObject agentObject))
            {
                agentObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                agentObject.name = "Agent_" + agent.id;
                agentObject.transform.localScale = Vector3.one * 0.7f;
                agentObjects.Add(agent.id, agentObject);
            }

            float coordinateScale = boardSize / 10f;
            agentObject.transform.position = new Vector3(
                agent.x * coordinateScale,
                agentHeight,
                agent.y * coordinateScale);
            agentObject.GetComponent<Renderer>().material.color = Color.Lerp(
                Color.blue,
                Color.red,
                Mathf.Clamp01(agent.wealth / 10f));
        }

        List<int> removedIds = new List<int>();
        foreach (KeyValuePair<int, GameObject> pair in agentObjects)
        {
            if (!activeIds.Contains(pair.Key))
            {
                Destroy(pair.Value);
                removedIds.Add(pair.Key);
            }
        }

        foreach (int removedId in removedIds)
        {
            agentObjects.Remove(removedId);
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(12f, 12f, 500f, 24f), status + " | step: " + currentStep);
        GUI.Label(new Rect(12f, 36f, 500f, 24f), endpoint);
    }

    [System.Serializable]
    private class AgentResponse
    {
        public int step;
        public AgentData[] agents;
    }

    [System.Serializable]
    private class AgentData
    {
        public int id;
        public int x;
        public int y;
        public int wealth;
        public string state;
    }
}
