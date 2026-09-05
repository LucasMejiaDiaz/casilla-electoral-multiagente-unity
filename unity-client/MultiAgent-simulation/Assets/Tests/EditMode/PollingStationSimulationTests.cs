using NUnit.Framework;
using UnityEngine;

namespace PollingStation.Simulation.Tests
{
    public sealed class PollingStationSimulationTests
    {
        [Test]
        public void AgentSnapshot_UsesStatusWhenStateIsMissing()
        {
            AgentSnapshot snapshot = new AgentSnapshot
            {
                state = "",
                status = " EN_MESA "
            };

            Assert.AreEqual("en_mesa", snapshot.EffectiveState);
        }

        [Test]
        public void QueueDefinition_WrapsIntoASecondRow()
        {
            GameObject originObject = new GameObject("QueueOrigin");
            originObject.transform.position = new Vector3(10f, 0f, 20f);
            QueueDefinition queue = new QueueDefinition
            {
                origin = originObject.transform,
                direction = Vector3.forward,
                rowDirection = Vector3.right,
                spacing = 1f,
                rowSpacing = 2f,
                positionsPerRow = 3
            };

            Assert.AreEqual(new Vector3(12f, 0f, 21f), queue.GetPosition(4));
            Object.DestroyImmediate(originObject);
        }

        [Test]
        public void QueueDefinition_PrefersImportedExplicitPoints()
        {
            GameObject originObject = new GameObject("QueueOrigin");
            Transform importedPoint = NewPoint(originObject.transform, "QUEUE_GENERAL_000", new Vector3(4f, 0f, 9f));
            QueueDefinition queue = new QueueDefinition
            {
                origin = originObject.transform,
                explicitPoints = new System.Collections.Generic.List<Transform> { importedPoint }
            };

            Assert.AreEqual(importedPoint.position, queue.GetPosition(0));
            Object.DestroyImmediate(originObject);
        }

        [Test]
        public void AgentViewManager_DoesNotDuplicateIdsAndRecyclesTerminalAgents()
        {
            GameObject root = new GameObject("TestRoot");
            SceneLayout layout = root.AddComponent<SceneLayout>();
            layout.entrance = NewPoint(root.transform, "Entrance", Vector3.zero);
            layout.exit = NewPoint(root.transform, "Exit", Vector3.right);
            layout.rejected = NewPoint(root.transform, "Rejected", Vector3.left);
            layout.fallback = NewPoint(root.transform, "Fallback", Vector3.forward);

            GameObject container = new GameObject("Agents");
            container.transform.SetParent(root.transform);
            AgentViewManager manager = root.AddComponent<AgentViewManager>();
            manager.Configure(layout, null, container.transform, null, null, null, null, false);

            SimulationSnapshot active = Snapshot(7, "arrived");
            manager.ApplySnapshot(active);
            manager.ApplySnapshot(active);
            Assert.AreEqual(1, manager.ActiveAgentCount);

            SimulationSnapshot terminal = Snapshot(7, "salio");
            manager.ApplySnapshot(terminal);
            Assert.AreEqual(1, manager.ActiveAgentCount);
            manager.ApplySnapshot(terminal);
            Assert.AreEqual(0, manager.ActiveAgentCount);

            Object.DestroyImmediate(root);
        }

        private static SimulationSnapshot Snapshot(int id, string state)
        {
            return new SimulationSnapshot
            {
                agents = new[]
                {
                    new AgentSnapshot { id = id, state = state }
                }
            };
        }

        private static Transform NewPoint(Transform parent, string name, Vector3 position)
        {
            GameObject point = new GameObject(name);
            point.transform.SetParent(parent);
            point.transform.position = position;
            return point.transform;
        }
    }
}
