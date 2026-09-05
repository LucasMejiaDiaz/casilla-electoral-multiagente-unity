using System;
using System.Collections.Generic;
using UnityEngine;

namespace PollingStation.Simulation
{
    [Serializable]
    public sealed class QueueDefinition
    {
        public Transform origin;
        public Vector3 direction = Vector3.back;
        public Vector3 rowDirection = Vector3.right;
        [Min(0.1f)] public float spacing = 0.9f;
        [Min(0.1f)] public float rowSpacing = 1.2f;
        [Min(1)] public int positionsPerRow = 10;
        public List<Transform> explicitPoints = new List<Transform>();

        public Vector3 GetPosition(int queuePosition)
        {
            int safeIndex = Mathf.Max(0, queuePosition);
            if (explicitPoints != null && safeIndex < explicitPoints.Count && explicitPoints[safeIndex] != null)
            {
                return explicitPoints[safeIndex].position;
            }

            if (origin == null)
            {
                return Vector3.zero;
            }

            int generatedIndex = explicitPoints != null && explicitPoints.Count > 0
                ? safeIndex - explicitPoints.Count + 1
                : safeIndex;
            int column = generatedIndex % Mathf.Max(1, positionsPerRow);
            int row = generatedIndex / Mathf.Max(1, positionsPerRow);
            return origin.position
                   + direction.normalized * (column * spacing)
                   + rowDirection.normalized * (row * rowSpacing);
        }
    }

    public sealed class SceneLayout : MonoBehaviour
    {
        public Transform entrance;
        public Transform exit;
        public Transform rejected;
        public Transform fallback;

        public QueueDefinition secretarioQueue = new QueueDefinition();
        public QueueDefinition mesaQueue = new QueueDefinition();
        public QueueDefinition casillaQueue = new QueueDefinition();
        public QueueDefinition urnaQueue = new QueueDefinition();

        public List<Transform> secretarioServicePoints = new List<Transform>();
        public List<Transform> mesaServicePoints = new List<Transform>();
        public List<Transform> casillaServicePoints = new List<Transform>();
        public List<Transform> urnaServicePoints = new List<Transform>();

        public Vector3 ResolveTarget(AgentSnapshot agent, int serviceOrdinal, int queueOrdinal)
        {
            switch (agent.EffectiveState)
            {
                case "arrived":
                    return PositionOrZero(entrance);
                case "esperando_secretario":
                    return secretarioQueue.GetPosition(ResolveQueuePosition(agent, queueOrdinal));
                case "en_secretario":
                    return ServicePosition(secretarioServicePoints, serviceOrdinal, secretarioQueue.origin);
                case "esperando_mesa":
                    return mesaQueue.GetPosition(ResolveQueuePosition(agent, queueOrdinal));
                case "en_mesa":
                    return ServicePosition(mesaServicePoints, serviceOrdinal, mesaQueue.origin);
                case "esperando_casilla":
                    return casillaQueue.GetPosition(ResolveQueuePosition(agent, queueOrdinal));
                case "en_casilla":
                    return ServicePosition(casillaServicePoints, serviceOrdinal, casillaQueue.origin);
                case "esperando_urna":
                    return urnaQueue.GetPosition(ResolveQueuePosition(agent, queueOrdinal));
                case "en_urna":
                    return ServicePosition(urnaServicePoints, serviceOrdinal, urnaQueue.origin);
                case "rechazado":
                case "rejected":
                    return PositionOrZero(rejected);
                case "salio":
                case "exit":
                    return PositionOrZero(exit);
                default:
                    return PositionOrZero(fallback);
            }
        }

        private static int ResolveQueuePosition(AgentSnapshot agent, int fallbackOrdinal)
        {
            return agent.queue_position >= 0 ? agent.queue_position : fallbackOrdinal;
        }

        private static Vector3 ServicePosition(List<Transform> points, int ordinal, Transform overflowOrigin)
        {
            if (points != null && points.Count > 0)
            {
                if (ordinal < points.Count)
                {
                    return points[Mathf.Max(0, ordinal)].position;
                }

                Transform last = points[points.Count - 1];
                return last.position + Vector3.back * (ordinal - points.Count + 1) * 0.8f;
            }

            return PositionOrZero(overflowOrigin);
        }

        private static Vector3 PositionOrZero(Transform target)
        {
            return target == null ? Vector3.zero : target.position;
        }
    }
}
