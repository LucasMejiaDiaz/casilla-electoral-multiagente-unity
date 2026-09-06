using System.Collections.Generic;
using System.IO;
using System.Linq;
using PollingStation.Simulation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PollingStation.Editor
{
    public static class PollingStationSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/PollingStationAndares.unity";
        private const string PrefabPath = "Assets/Prefabs/PollingStation";
        private const string MaterialPath = "Assets/Materials/PollingStation";
        private const string DetailedModelPath = "Assets/Models/PollingStation/casilla_votacion.fbx";
        private const string TeamAssetPath = "Assets/Models/PollingStation/TeamAssets";
        private const string TeamTablePath = TeamAssetPath + "/mesa.fbx";
        private const string TeamBoothPath = TeamAssetPath + "/Casilla.fbx";
        private const string TeamBallotBoxPath = TeamAssetPath + "/Urna.fbx";
        private const string TeamChairPath = TeamAssetPath + "/Silla.fbx";
        private const string TeamWomanPath = TeamAssetPath + "/modelo_mujer.fbx";
        private const string VoterControllerPath = "Assets/Animations/PollingStation/TeamVoter.controller";

        private static readonly Dictionary<string, Color> DetailedMaterialColors = new Dictionary<string, Color>
        {
            { "Acrilico_Urna", new Color(0.72f, 0.80f, 0.86f, 1f) },
            { "Asfalto_Cajones", new Color(0.11f, 0.11f, 0.12f) },
            { "Asfalto_Calle", new Color(0.15f, 0.15f, 0.17f) },
            { "Asfalto_Pasillo", new Color(0.19f, 0.19f, 0.21f) },
            { "Cancel_Aluminio", new Color(0.42f, 0.43f, 0.45f) },
            { "Cinta_Fila", new Color(0.55f, 0.10f, 0.14f) },
            { "Columna", new Color(0.78f, 0.76f, 0.74f) },
            { "Concreto", new Color(0.62f, 0.61f, 0.58f) },
            { "Cristal", new Color(0.62f, 0.72f, 0.78f) },
            { "Follaje", new Color(0.14f, 0.32f, 0.14f) },
            { "Guarnicion", new Color(0.80f, 0.79f, 0.75f) },
            { "INE_Guinda", new Color(0.42f, 0.08f, 0.20f) },
            { "Junta_Concreto", new Color(0.42f, 0.41f, 0.39f) },
            { "Lampara", new Color(0.95f, 0.93f, 0.80f) },
            { "Linea_Amarilla", new Color(0.85f, 0.68f, 0.10f) },
            { "Linea_Azul", new Color(0.10f, 0.28f, 0.62f) },
            { "Linea_Blanca", new Color(0.92f, 0.92f, 0.88f) },
            { "Madera_Mesa", new Color(0.55f, 0.38f, 0.24f) },
            { "Mampara_Carton", new Color(0.88f, 0.88f, 0.86f) },
            { "Metal", new Color(0.55f, 0.56f, 0.58f) },
            { "Muro", new Color(0.90f, 0.90f, 0.90f) },
            { "Papel_Boletas", new Color(0.95f, 0.94f, 0.90f) },
            { "Pasto", new Color(0.19f, 0.38f, 0.15f) },
            { "Piso_Plaza", new Color(0.72f, 0.71f, 0.68f) },
            { "Poste_Luz", new Color(0.28f, 0.29f, 0.30f) },
            { "Silla", new Color(0.20f, 0.22f, 0.26f) },
            { "Texto_Claro", new Color(0.97f, 0.97f, 0.97f) },
            { "Texto_Oscuro", new Color(0.06f, 0.06f, 0.08f) },
            { "Tronco", new Color(0.24f, 0.17f, 0.11f) },
            { "Zona_Fila", new Color(0.85f, 0.84f, 0.60f) },
            { "Zona_Mampara", new Color(0.70f, 0.86f, 0.72f) },
            { "Zona_Registro", new Color(0.62f, 0.76f, 0.88f) },
            { "Zona_Urna", new Color(0.90f, 0.74f, 0.62f) }
        };

        [MenuItem("Tools/Polling Station/Build Andares Scene")]
        public static void BuildAndaresScene()
        {
            EnsureFolder("Assets/Prefabs");
            EnsureFolder(PrefabPath);
            EnsureFolder("Assets/Materials");
            EnsureFolder(MaterialPath);
            EnsureFolder("Assets/Scenes");

            Material floorMaterial = CreateMaterial("Floor", new Color(0.79f, 0.77f, 0.71f), 0.05f, 0.35f);
            Material wallMaterial = CreateMaterial("Wall", new Color(0.92f, 0.91f, 0.86f), 0f, 0.25f);
            Material glassMaterial = CreateMaterial("Glass", new Color(0.50f, 0.72f, 0.79f), 0.15f, 0.7f);
            Material ineMaterial = CreateMaterial("ElectoralPurple", new Color(0.42f, 0.18f, 0.48f), 0f, 0.4f);
            Material accentMaterial = CreateMaterial("ElectoralPink", new Color(0.79f, 0.12f, 0.45f), 0f, 0.4f);
            Material woodMaterial = CreateMaterial("TableWood", new Color(0.47f, 0.27f, 0.13f), 0f, 0.25f);
            Material metalMaterial = CreateMaterial("BarrierMetal", new Color(0.22f, 0.25f, 0.28f), 0.75f, 0.65f);
            Material whiteMaterial = CreateMaterial("PaperWhite", new Color(0.96f, 0.96f, 0.94f), 0f, 0.3f);
            Material voterMaterial = CreateMaterial("Voter", new Color(0.20f, 0.48f, 0.82f), 0f, 0.35f);
            Material workerMaterial = CreateMaterial("Worker", new Color(0.30f, 0.18f, 0.45f), 0f, 0.35f);

            AgentView voterPrefab = CreateVoterPrefab(voterMaterial);
            GameObject workerPrefab = CreateWorkerPrefab(workerMaterial);
            GameObject tablePrefab = CreateTablePrefab(woodMaterial, metalMaterial);
            GameObject boothPrefab = CreateBoothPrefab(whiteMaterial, ineMaterial);
            GameObject ballotBoxPrefab = CreateBallotBoxPrefab(whiteMaterial, accentMaterial);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("PollingStationAndares");

            GameObject environment = Child(root, "Environment");
            BuildEnvironment(environment.transform, floorMaterial, wallMaterial, glassMaterial, ineMaterial, accentMaterial, metalMaterial);

            GameObject stations = Child(root, "Stations");
            List<Transform> secretarioPoints = BuildSecretarios(stations.transform, tablePrefab, workerPrefab);
            List<Transform> mesaPoints = BuildMesas(stations.transform, tablePrefab, workerPrefab);
            List<Transform> casillaPoints = BuildBooths(stations.transform, boothPrefab);
            List<Transform> urnaPoints = BuildUrnas(stations.transform, ballotBoxPrefab, workerPrefab);

            GameObject waypoints = Child(root, "Waypoints");
            Transform entrance = Point(waypoints.transform, "Entrada", new Vector3(-12f, 0f, -7f));
            Transform exit = Point(waypoints.transform, "Salida", new Vector3(12f, 0f, 7f));
            Transform rejected = Point(waypoints.transform, "Rechazados", new Vector3(-12f, 0f, 7f));
            Transform fallback = Point(waypoints.transform, "Fallback", new Vector3(0f, 0f, 7f));
            Transform secretarioQueue = Point(waypoints.transform, "FilaSecretario", new Vector3(-10.5f, 0f, -5f));
            Transform mesaQueue = Point(waypoints.transform, "FilaMesa", new Vector3(-4.5f, 0f, -4f));
            Transform casillaQueue = Point(waypoints.transform, "FilaCasilla", new Vector3(0.5f, 0f, -5.5f));
            Transform urnaQueue = Point(waypoints.transform, "FilaUrna", new Vector3(7.2f, 0f, 6.5f));

            GameObject agents = Child(root, "Agents");
            GameObject systems = Child(root, "Systems");
            SceneLayout layout = Child(systems, "SceneLayout").AddComponent<SceneLayout>();
            layout.entrance = entrance;
            layout.exit = exit;
            layout.rejected = rejected;
            layout.fallback = fallback;
            layout.secretarioServicePoints = secretarioPoints;
            layout.mesaServicePoints = mesaPoints;
            layout.casillaServicePoints = casillaPoints;
            layout.urnaServicePoints = urnaPoints;
            layout.secretarioQueue = Queue(secretarioQueue, Vector3.forward, Vector3.right, 0.85f, 1.1f, 12);
            layout.mesaQueue = Queue(mesaQueue, Vector3.forward, Vector3.right, 0.85f, 1.1f, 10);
            layout.casillaQueue = Queue(casillaQueue, Vector3.forward, Vector3.left, 0.85f, 1.1f, 12);
            layout.urnaQueue = Queue(urnaQueue, Vector3.back, Vector3.left, 0.85f, 1.1f, 8);

            GameObject detailedModel = TryAddDetailedModel(root.transform);
            if (detailedModel != null)
            {
                environment.SetActive(false);
                stations.SetActive(false);
                ApplyDetailedModelAnchors(layout, detailedModel.transform);
                PrepareOpenArrivalZone(detailedModel.transform);
            }

            BuildPermanentVisualDetails(root.transform, layout, ineMaterial, accentMaterial, whiteMaterial);

            Camera camera = BuildCamera(root.transform);
            Light mainLight = BuildLighting(root.transform);
            BuildUi(root.transform, out Text connectionText, out Text clockText, out Text countersText, out GameObject banner, out Text bannerText);

            ExternalEventVisualizer eventVisualizer = Child(systems, "ExternalEventVisualizer").AddComponent<ExternalEventVisualizer>();
            eventVisualizer.Configure(mainLight, banner, bannerText);

            AgentViewManager manager = Child(systems, "AgentViewManager").AddComponent<AgentViewManager>();
            manager.Configure(layout, voterPrefab, agents.transform, eventVisualizer, connectionText, clockText, countersText, true);

            SimulationStateProvider provider = Child(systems, "SimulationStateProvider").AddComponent<SimulationStateProvider>();
            provider.Configure(manager, "http://127.0.0.1:5000/get_agents", true);

            if (detailedModel != null)
            {
                FocusCameraOnPollingArea(camera, layout);
            }
            else
            {
                camera.transform.LookAt(new Vector3(0f, 0f, 0f));
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = root;
            Debug.Log($"Polling station scene generated at {ScenePath}");
        }

        private static GameObject TryAddDetailedModel(Transform parent)
        {
            if (!File.Exists(Path.GetFullPath(DetailedModelPath)))
            {
                return null;
            }

            AssetDatabase.ImportAsset(DetailedModelPath, ImportAssetOptions.ForceSynchronousImport);
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(DetailedModelPath);
            if (modelAsset == null)
            {
                Debug.LogWarning($"The detailed model could not be loaded from {DetailedModelPath}.");
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset, parent);
            instance.name = "DetailedAndaresModel";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            ApplyDetailedMaterials(instance);
            return instance;
        }

        private static void ApplyDetailedMaterials(GameObject instance)
        {
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                string materialName = MaterialForDetailedObject(renderer.gameObject.name);
                if (string.IsNullOrEmpty(materialName) || !DetailedMaterialColors.TryGetValue(materialName, out Color color)) continue;

                Material[] materials = renderer.sharedMaterials;
                bool metallic = materialName == "Metal" || materialName == "Cancel_Aluminio" || materialName == "Poste_Luz";
                Material replacement = CreateMaterial($"Imported_{materialName}", color, metallic ? 0.65f : 0f, metallic ? 0.65f : 0.3f);
                for (int index = 0; index < materials.Length; index++)
                {
                    materials[index] = replacement;
                }
                renderer.sharedMaterials = materials;
            }
        }

        private static string MaterialForDetailedObject(string objectName)
        {
            if (objectName.StartsWith("Zona_Fila")) return "Zona_Fila";
            if (objectName.StartsWith("Zona_Mampara")) return "Zona_Mampara";
            if (objectName.StartsWith("Zona_Registro")) return "Zona_Registro";
            if (objectName.StartsWith("Zona_Urna")) return "Zona_Urna";
            if (objectName.Contains("Sillas")) return "Silla";
            if (objectName.StartsWith("Secretario_") || objectName.StartsWith("MesaDirectiva_")) return "Madera_Mesa";
            if (objectName.StartsWith("Mampara_")) return "Mampara_Carton";
            if (objectName.StartsWith("Urna_")) return "Acrilico_Urna";
            if (objectName.Contains("Boletas") || objectName.Contains("Utiles")) return "Papel_Boletas";
            if (objectName.Contains("Cintas_Fila")) return "Cinta_Fila";
            if (objectName.Contains("Postes_Fila")) return "Metal";
            if (objectName.StartsWith("Piso_Interior")) return "Piso_Plaza";
            if (objectName.StartsWith("Columnas_")) return "Columna";
            if (objectName.StartsWith("Muro_")) return "Muro";
            if (objectName.StartsWith("Fachada_Cristal")) return "Cristal";
            if (objectName.StartsWith("Fachada_Cancel")) return "Cancel_Aluminio";
            if (objectName.StartsWith("Banner_INE") || objectName.StartsWith("Arco_")) return "INE_Guinda";
            if (objectName.Contains("Follaje") || objectName.StartsWith("Terreno_Pasto")) return "Pasto";
            if (objectName.Contains("Tronco")) return "Tronco";
            if (objectName.StartsWith("Asfalto")) return "Asfalto_Cajones";
            if (objectName.StartsWith("Linea_Azul")) return "Linea_Azul";
            if (objectName.StartsWith("Linea_Amarilla")) return "Linea_Amarilla";
            return null;
        }

        private static void ApplyDetailedModelAnchors(SceneLayout layout, Transform model)
        {
            Transform spawn = FindNamed(model, "SPAWN");
            Transform outsideSpawn = FindNamed(model, "SPAWN_EXTERIOR");
            layout.entrance = spawn ?? layout.entrance;
            layout.exit = FindNamed(model, "EXIT") ?? layout.exit;
            layout.rejected = outsideSpawn ?? layout.rejected;
            layout.fallback = spawn ?? layout.fallback;

            layout.secretarioServicePoints = FindNamedPrefix(model, "SLOT_SECRETARIO_");
            layout.mesaServicePoints = FindNamedPrefix(model, "SLOT_MESA_");
            layout.casillaServicePoints = FindNamedPrefix(model, "SLOT_CASILLA_");
            layout.urnaServicePoints = FindNamedPrefix(model, "SLOT_URNA_");

            ApplyQueueAnchors(layout.secretarioQueue, FindNamedPrefix(model, "QUEUE_GENERAL_"));
            ApplyQueueAnchors(layout.mesaQueue, FindNamedPrefix(model, "QUEUE_MESA_"));
            ApplyQueueAnchors(layout.casillaQueue, FindNamedPrefix(model, "QUEUE_CASILLA_"));
            ApplyQueueAnchors(layout.urnaQueue, FindNamedPrefix(model, "QUEUE_URNA_"));
        }

        private static void PrepareOpenArrivalZone(Transform model)
        {
            // Los votantes se desplazan entre destinos visuales, por lo que unas
            // cintas rígidas en esta zona hacían parecer que atravesaban objetos.
            // El piso y los puntos de espera se conservan; sólo se abre el pasillo.
            SetNamedActive(model, "Cintas_Fila", false);
            SetNamedActive(model, "Postes_Fila", false);
        }

        private static void ApplyTeamAssetReplacements(Transform sceneRoot, Transform detailedModel)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(TeamTablePath) == null &&
                AssetDatabase.LoadAssetAtPath<GameObject>(TeamBoothPath) == null &&
                AssetDatabase.LoadAssetAtPath<GameObject>(TeamBallotBoxPath) == null)
            {
                return;
            }

            GameObject replacements = Child(sceneRoot.gameObject, "TeamAssetReplacements");

            for (int index = 0; index < 6; index++)
            {
                string tableName = $"Secretario_{index:00}";
                ReplaceDetailedTarget(replacements.transform, detailedModel, tableName, TeamTablePath, Quaternion.Euler(0f, 90f, 0f));
                ReplaceDetailedTarget(replacements.transform, detailedModel, tableName + "_Sillas", TeamChairPath, Quaternion.Euler(0f, 90f, 0f));
            }

            for (int index = 0; index < 3; index++)
            {
                ReplaceDetailedTarget(
                    replacements.transform,
                    detailedModel,
                    $"MesaDirectiva_{index:00}",
                    TeamTablePath,
                    Quaternion.Euler(0f, 90f, 0f));
            }

            for (int index = 0; index < 8; index++)
            {
                ReplaceDetailedTarget(
                    replacements.transform,
                    detailedModel,
                    $"Mampara_{index:00}",
                    TeamBoothPath,
                    Quaternion.identity);
            }

            for (int index = 0; index < 2; index++)
            {
                Transform box = FindNamed(detailedModel, $"Urna_{index:00}_Caja");
                Transform details = FindNamed(detailedModel, $"Urna_{index:00}_Detalles");
                List<Transform> targets = new List<Transform>();
                if (box != null) targets.Add(box);
                if (details != null) targets.Add(details);
                ReplaceDetailedTargets(
                    replacements.transform,
                    targets,
                    $"Team_Urna_{index + 1}",
                    TeamBallotBoxPath,
                    Quaternion.identity);
            }
        }

        private static void ReplaceDetailedTarget(
            Transform replacementParent,
            Transform detailedModel,
            string targetName,
            string assetPath,
            Quaternion visualRotation)
        {
            Transform target = FindNamed(detailedModel, targetName);
            if (target == null) return;
            ReplaceDetailedTargets(
                replacementParent,
                new List<Transform> { target },
                "Team_" + targetName,
                assetPath,
                visualRotation);
        }

        private static void ReplaceDetailedTargets(
            Transform replacementParent,
            List<Transform> targets,
            string replacementName,
            string assetPath,
            Quaternion visualRotation)
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (source == null || targets.Count == 0) return;

            Bounds targetBounds = RendererBounds(targets);
            if (targetBounds.size.sqrMagnitude < 0.0001f) return;

            GameObject wrapper = Child(replacementParent.gameObject, replacementName);
            GameObject visual = Object.Instantiate(source, wrapper.transform);
            visual.name = source.name;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = visualRotation;
            visual.transform.localScale = Vector3.one;

            Bounds sourceBounds = RendererBounds(new List<Transform> { visual.transform });
            wrapper.transform.localScale = new Vector3(
                SafeRatio(targetBounds.size.x, sourceBounds.size.x),
                SafeRatio(targetBounds.size.y, sourceBounds.size.y),
                SafeRatio(targetBounds.size.z, sourceBounds.size.z));

            Bounds fittedBounds = RendererBounds(new List<Transform> { wrapper.transform });
            wrapper.transform.position += targetBounds.center - fittedBounds.center;

            foreach (Transform target in targets)
            {
                target.gameObject.SetActive(false);
            }
        }

        private static Bounds RendererBounds(List<Transform> roots)
        {
            List<Renderer> renderers = new List<Renderer>();
            foreach (Transform root in roots)
            {
                if (root != null) renderers.AddRange(root.GetComponentsInChildren<Renderer>(true));
            }

            if (renderers.Count == 0)
            {
                return new Bounds(roots.Count > 0 && roots[0] != null ? roots[0].position : Vector3.zero, Vector3.zero);
            }

            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Count; index++) bounds.Encapsulate(renderers[index].bounds);
            return bounds;
        }

        private static float SafeRatio(float target, float source)
        {
            return source > 0.0001f ? target / source : 1f;
        }

        private static void SetNamedActive(Transform parent, string exactName, bool active)
        {
            Transform target = FindNamed(parent, exactName);
            if (target != null) target.gameObject.SetActive(active);
        }

        private static void BuildPermanentVisualDetails(
            Transform parent,
            SceneLayout layout,
            Material ineMaterial,
            Material accentMaterial,
            Material whiteMaterial)
        {
            GameObject details = Child(parent.gameObject, "PermanentVisualDetails");

            // Marcas planas: organizan la espera sin convertirse en obstáculos.
            int markerCount = Mathf.Min(12, layout.secretarioQueue.explicitPoints.Count);
            for (int index = 0; index < markerCount; index++)
            {
                Vector3 position = layout.secretarioQueue.explicitPoints[index].position + Vector3.up * 0.025f;
                GameObject marker = DecorativePrimitive(
                    PrimitiveType.Cylinder,
                    $"Espera_{index + 1:00}",
                    details.transform,
                    position,
                    new Vector3(0.28f, 0.012f, 0.28f),
                    index == 0 ? accentMaterial : ineMaterial);
                marker.transform.localRotation = Quaternion.identity;
            }

            // Dos flechas bastan para que se entienda el sentido del recorrido.
            if (layout.entrance != null && layout.secretarioServicePoints.Count > 0)
            {
                Vector3 start = layout.entrance.position;
                Vector3 end = layout.secretarioServicePoints[0].position;
                Vector3 direction = end - start;
                direction.y = 0f;
                CreateFloorArrow(details.transform, "Flecha_Entrada_1", Vector3.Lerp(start, end, 0.35f), direction, accentMaterial);
                CreateFloorArrow(details.transform, "Flecha_Entrada_2", Vector3.Lerp(start, end, 0.70f), direction, accentMaterial);
            }

            // Los títulos grandes se omiten porque los textos 3D cambiaban de escala
            // según la vista de Unity y llegaban a cubrir el escenario. Las etiquetas
            // integradas al modelo y la interfaz en pantalla siguen indicando las zonas.
        }

        private static void CreateFloorArrow(Transform parent, string name, Vector3 position, Vector3 direction, Material material)
        {
            if (direction.sqrMagnitude < 0.001f) return;

            GameObject arrow = Child(parent.gameObject, name);
            arrow.transform.position = position + Vector3.up * 0.035f;
            arrow.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            DecorativePrimitive(PrimitiveType.Cube, "Shaft", arrow.transform, Vector3.zero, new Vector3(0.12f, 0.018f, 0.72f), material);

            GameObject left = DecorativePrimitive(PrimitiveType.Cube, "HeadLeft", arrow.transform, new Vector3(-0.16f, 0f, 0.34f), new Vector3(0.10f, 0.018f, 0.42f), material);
            left.transform.localRotation = Quaternion.Euler(0f, -38f, 0f);
            GameObject right = DecorativePrimitive(PrimitiveType.Cube, "HeadRight", arrow.transform, new Vector3(0.16f, 0f, 0.34f), new Vector3(0.10f, 0.018f, 0.42f), material);
            right.transform.localRotation = Quaternion.Euler(0f, 38f, 0f);
        }

        private static void ApplyQueueAnchors(QueueDefinition queue, List<Transform> anchors)
        {
            if (anchors.Count == 0) return;
            queue.explicitPoints = anchors;
            queue.origin = anchors[anchors.Count - 1];
            queue.positionsPerRow = Mathf.Max(1, anchors.Count);
            if (anchors.Count > 1)
            {
                Vector3 direction = anchors[anchors.Count - 1].position - anchors[anchors.Count - 2].position;
                if (direction.sqrMagnitude > 0.0001f) queue.direction = direction.normalized;
            }
        }

        private static Transform FindNamed(Transform parent, string exactName)
        {
            foreach (Transform item in parent.GetComponentsInChildren<Transform>(true))
            {
                if (item.name == exactName) return item;
            }
            return null;
        }

        private static List<Transform> FindNamedPrefix(Transform parent, string prefix)
        {
            List<Transform> matches = new List<Transform>();
            foreach (Transform item in parent.GetComponentsInChildren<Transform>(true))
            {
                if (item.name.StartsWith(prefix)) matches.Add(item);
            }
            matches.Sort((left, right) => string.CompareOrdinal(left.name, right.name));
            return matches;
        }

        private static void FocusCameraOnPollingArea(Camera camera, SceneLayout layout)
        {
            List<Transform> focusPoints = new List<Transform>();
            focusPoints.AddRange(layout.secretarioServicePoints);
            focusPoints.AddRange(layout.mesaServicePoints);
            focusPoints.AddRange(layout.casillaServicePoints);
            focusPoints.AddRange(layout.urnaServicePoints);
            if (focusPoints.Count == 0) return;

            Bounds bounds = new Bounds(focusPoints[0].position, Vector3.zero);
            foreach (Transform point in focusPoints) bounds.Encapsulate(point.position);
            float span = Mathf.Max(20f, Mathf.Max(bounds.size.x, bounds.size.z));
            Vector3 focus = bounds.center;
            camera.transform.position = focus + new Vector3(0f, span * 0.72f, span * 0.95f);
            camera.transform.LookAt(focus);
            camera.farClipPlane = 500f;
        }

        [MenuItem("Tools/Polling Station/Capture Andares Preview")]
        public static void CaptureAndaresPreview()
        {
            BuildAndaresScene();

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Camera sceneCamera = Object.FindAnyObjectByType<Camera>();
            if (sceneCamera == null)
            {
                throw new MissingReferenceException("The Andares scene does not contain a camera.");
            }

            const int width = 1600;
            const int height = 900;
            RenderTexture renderTexture = new RenderTexture(width, height, 24)
            {
                antiAliasing = 4
            };
            Texture2D preview = new Texture2D(width, height, TextureFormat.RGB24, false);
            RenderTexture previousTarget = sceneCamera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;

            try
            {
                sceneCamera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                sceneCamera.Render();
                preview.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                preview.Apply();

                EnsureFolder("Assets/Documentation");
                string outputPath = Path.Combine(Application.dataPath, "Documentation", "PollingStationAndaresPreview.png");
                File.WriteAllBytes(outputPath, preview.EncodeToPNG());
                AssetDatabase.Refresh();
                Debug.Log($"Andares preview generated at {outputPath}");
            }
            finally
            {
                sceneCamera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                Object.DestroyImmediate(renderTexture);
                Object.DestroyImmediate(preview);
            }
        }

        private static void BuildEnvironment(
            Transform parent,
            Material floor,
            Material wall,
            Material glass,
            Material purple,
            Material pink,
            Material metal)
        {
            GameObject floorObject = Primitive(PrimitiveType.Cube, "Floor", parent, new Vector3(0f, -0.1f, 0f), new Vector3(28f, 0.2f, 18f), floor);
            floorObject.isStatic = true;

            Primitive(PrimitiveType.Cube, "BackWall", parent, new Vector3(0f, 2.5f, 8.8f), new Vector3(28f, 5f, 0.2f), wall);
            Primitive(PrimitiveType.Cube, "LeftGlassWall", parent, new Vector3(-13.8f, 2.2f, 0f), new Vector3(0.15f, 4.4f, 17.5f), glass);
            Primitive(PrimitiveType.Cube, "RightGlassWall", parent, new Vector3(13.8f, 2.2f, 0f), new Vector3(0.15f, 4.4f, 17.5f), glass);

            GameObject storefronts = Child(parent.gameObject, "StorefrontBackground");
            for (int index = 0; index < 5; index++)
            {
                float x = -10.8f + index * 5.4f;
                Primitive(PrimitiveType.Cube, $"Storefront_{index + 1}", storefronts.transform, new Vector3(x, 1.6f, 8.35f), new Vector3(4.6f, 3.1f, 0.45f), index % 2 == 0 ? glass : wall);
            }

            GameObject signs = Child(parent.gameObject, "ElectoralSigns");
            CreateSign(signs.transform, "MainSign", "CASILLA ELECTORAL", new Vector3(0f, 4.2f, 8.15f), new Vector3(7f, 1.2f, 0.15f), purple, Color.white, 0.115f);
            CreateSign(signs.transform, "EntranceSign", "ENTRADA", new Vector3(-11.8f, 2.2f, -8.15f), new Vector3(2.2f, 0.8f, 0.12f), pink, Color.white, 0.08f);
            CreateSign(signs.transform, "ExitSign", "SALIDA", new Vector3(11.8f, 2.2f, 8.1f), new Vector3(2.2f, 0.8f, 0.12f), pink, Color.white, 0.08f);

            GameObject barriers = Child(parent.gameObject, "QueueBarriers");
            BuildBarrierLine(barriers.transform, new Vector3(-11.2f, 0f, -5.4f), Vector3.forward, 8, metal);
            BuildBarrierLine(barriers.transform, new Vector3(-5.2f, 0f, -4.4f), Vector3.forward, 7, metal);
            BuildBarrierLine(barriers.transform, new Vector3(0f, 0f, -5.9f), Vector3.forward, 8, metal);
        }

        private static List<Transform> BuildSecretarios(Transform parent, GameObject tablePrefab, GameObject workerPrefab)
        {
            GameObject group = Child(parent.gameObject, "Secretario");
            List<Transform> points = new List<Transform>();
            for (int index = 0; index < 6; index++)
            {
                float z = -5f + index * 2f;
                InstantiatePrefab(tablePrefab, group.transform, new Vector3(-7.4f, 0f, z), Quaternion.Euler(0f, 90f, 0f), $"MesaSecretario_{index + 1}");
                InstantiatePrefab(workerPrefab, group.transform, new Vector3(-6.7f, 0f, z), Quaternion.Euler(0f, -90f, 0f), $"Secretario_{index + 1}");
                points.Add(Point(group.transform, $"ServicePoint_{index + 1}", new Vector3(-8.5f, 0f, z)));
            }
            return points;
        }

        private static List<Transform> BuildMesas(Transform parent, GameObject tablePrefab, GameObject workerPrefab)
        {
            GameObject group = Child(parent.gameObject, "Mesa");
            List<Transform> points = new List<Transform>();
            for (int index = 0; index < 3; index++)
            {
                float z = -3f + index * 3f;
                InstantiatePrefab(tablePrefab, group.transform, new Vector3(-2.4f, 0f, z), Quaternion.Euler(0f, 90f, 0f), $"MesaDirectiva_{index + 1}");
                InstantiatePrefab(workerPrefab, group.transform, new Vector3(-1.7f, 0f, z), Quaternion.Euler(0f, -90f, 0f), $"FuncionarioMesa_{index + 1}");
                points.Add(Point(group.transform, $"ServicePoint_{index + 1}", new Vector3(-3.5f, 0f, z)));
            }
            return points;
        }

        private static List<Transform> BuildBooths(Transform parent, GameObject boothPrefab)
        {
            GameObject group = Child(parent.gameObject, "Casillas");
            List<Transform> points = new List<Transform>();
            int index = 0;
            for (int row = 0; row < 2; row++)
            {
                for (int column = 0; column < 4; column++)
                {
                    index++;
                    Vector3 position = new Vector3(3f + column * 2.5f, 0f, -2.6f + row * 4.3f);
                    InstantiatePrefab(boothPrefab, group.transform, position, Quaternion.identity, $"Mampara_{index}");
                    points.Add(Point(group.transform, $"ServicePoint_{index}", position + new Vector3(0f, 0f, -0.35f)));
                }
            }
            return points;
        }

        private static List<Transform> BuildUrnas(Transform parent, GameObject ballotBoxPrefab, GameObject workerPrefab)
        {
            GameObject group = Child(parent.gameObject, "Urnas");
            List<Transform> points = new List<Transform>();
            for (int index = 0; index < 2; index++)
            {
                float z = 4.3f + index * 2.3f;
                InstantiatePrefab(ballotBoxPrefab, group.transform, new Vector3(9.3f, 0f, z), Quaternion.identity, $"Urna_{index + 1}");
                InstantiatePrefab(workerPrefab, group.transform, new Vector3(10.5f, 0f, z), Quaternion.Euler(0f, -90f, 0f), $"FuncionarioUrna_{index + 1}");
                points.Add(Point(group.transform, $"ServicePoint_{index + 1}", new Vector3(8.1f, 0f, z)));
            }
            return points;
        }

        private static Camera BuildCamera(Transform parent)
        {
            GameObject cameraObject = Child(parent.gameObject, "MainCamera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 18f, 23f);
            camera.fieldOfView = 46f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.07f, 0.10f);
            camera.allowHDR = true;
            camera.allowMSAA = true;
            camera.allowDynamicResolution = false;
            camera.useOcclusionCulling = true;
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static Light BuildLighting(Transform parent)
        {
            GameObject lighting = Child(parent.gameObject, "Lighting");
            GameObject lightObject = Child(lighting, "DirectionalLight");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.95f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.82f;
            light.shadowBias = 0.045f;
            light.shadowNormalBias = 0.3f;
            light.transform.rotation = Quaternion.Euler(48f, 35f, 0f);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.38f, 0.41f, 0.46f);
            return light;
        }

        private static void BuildUi(
            Transform parent,
            out Text connectionText,
            out Text clockText,
            out Text countersText,
            out GameObject banner,
            out Text bannerText)
        {
            GameObject ui = Child(parent.gameObject, "UI");
            Canvas canvas = ui.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = ui.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            ui.AddComponent<GraphicRaycaster>();

            GameObject statusPanel = UiPanel(ui.transform, "StatusPanel", new Color(0.07f, 0.08f, 0.11f, 0.88f));
            RectTransform statusRect = statusPanel.GetComponent<RectTransform>();
            SetRect(statusRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(22f, -22f), new Vector2(870f, 145f), new Vector2(0f, 1f));

            connectionText = UiText(statusPanel.transform, "ConnectionStatus", "Inicializando…", 25, TextAnchor.MiddleLeft);
            SetRect(connectionText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -15f), new Vector2(-20f, 36f), new Vector2(0.5f, 1f));
            clockText = UiText(statusPanel.transform, "SimulationClock", "Tiempo simulado: 0.0 min", 22, TextAnchor.MiddleLeft);
            SetRect(clockText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -54f), new Vector2(-20f, 34f), new Vector2(0.5f, 1f));
            countersText = UiText(statusPanel.transform, "AgentCounters", "Activos: 0", 20, TextAnchor.MiddleLeft);
            SetRect(countersText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -92f), new Vector2(-20f, 36f), new Vector2(0.5f, 1f));

            GameObject flowPanel = UiPanel(ui.transform, "FlowGuide", new Color(0.07f, 0.08f, 0.11f, 0.80f));
            RectTransform flowRect = flowPanel.GetComponent<RectTransform>();
            SetRect(flowRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-22f, -22f), new Vector2(660f, 104f), new Vector2(1f, 1f));
            Text flowText = UiText(flowPanel.transform, "FlowText", "RECORRIDO DE VOTACIÓN\nEntrada  →  Registro  →  Mamparas  →  Urnas  →  Salida", 22, TextAnchor.MiddleCenter);
            SetRect(flowText.rectTransform, Vector2.zero, Vector2.one, new Vector2(14f, 8f), new Vector2(-14f, -8f), new Vector2(0.5f, 0.5f));

            banner = UiPanel(ui.transform, "ExternalEventBanner", new Color(0.68f, 0.08f, 0.12f, 0.94f));
            RectTransform bannerRect = banner.GetComponent<RectTransform>();
            SetRect(bannerRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -25f), new Vector2(720f, 70f), new Vector2(0.5f, 1f));
            bannerText = UiText(banner.transform, "BannerText", "EVENTO EXTERNO", 27, TextAnchor.MiddleCenter);
            SetRect(bannerText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            banner.SetActive(false);
        }

        private static QueueDefinition Queue(Transform origin, Vector3 direction, Vector3 rowDirection, float spacing, float rowSpacing, int perRow)
        {
            return new QueueDefinition
            {
                origin = origin,
                direction = direction,
                rowDirection = rowDirection,
                spacing = spacing,
                rowSpacing = rowSpacing,
                positionsPerRow = perRow
            };
        }

        private static void BuildBarrierLine(Transform parent, Vector3 start, Vector3 direction, int count, Material material)
        {
            for (int index = 0; index < count; index++)
            {
                Vector3 position = start + direction.normalized * index * 1.25f;
                Primitive(PrimitiveType.Cylinder, $"Post_{parent.childCount + 1}", parent, position + Vector3.up * 0.45f, new Vector3(0.08f, 0.45f, 0.08f), material);
                if (index < count - 1)
                {
                    Vector3 midpoint = position + direction.normalized * 0.625f + Vector3.up * 0.75f;
                    Vector3 scale = Mathf.Abs(direction.x) > 0.5f ? new Vector3(1.25f, 0.06f, 0.06f) : new Vector3(0.06f, 0.06f, 1.25f);
                    Primitive(PrimitiveType.Cube, $"Rail_{parent.childCount + 1}", parent, midpoint, scale, material);
                }
            }
        }

        private static AgentView CreateVoterPrefab(Material material)
        {
            string path = $"{PrefabPath}/VoterPlaceholder.prefab";
            GameObject root = new GameObject("VoterPlaceholder");
            AgentView view = root.AddComponent<AgentView>();
            GameObject woman = AddTeamModel(root.transform, TeamWomanPath, Quaternion.identity);
            if (woman != null)
            {
                FitModelHeight(woman, 1.72f);
                ConfigureTeamVoterAnimator(woman);
            }
            else
            {
                Primitive(PrimitiveType.Capsule, "Body", root.transform, new Vector3(0f, 0.9f, 0f), new Vector3(0.58f, 0.9f, 0.58f), material);
            }
            GameObject saved = SavePrefab(root, path);
            return saved.GetComponent<AgentView>();
        }

        private static GameObject CreateWorkerPrefab(Material material)
        {
            GameObject root = new GameObject("SecretaryPlaceholder");
            Primitive(PrimitiveType.Capsule, "Body", root.transform, new Vector3(0f, 0.9f, 0f), new Vector3(0.6f, 0.9f, 0.6f), material);
            return SavePrefab(root, $"{PrefabPath}/SecretaryPlaceholder.prefab");
        }

        private static GameObject CreateTablePrefab(Material wood, Material metal)
        {
            GameObject root = new GameObject("TablePlaceholder");
            if (AddTeamModel(root.transform, TeamTablePath, Quaternion.identity) == null)
            {
                Primitive(PrimitiveType.Cube, "Top", root.transform, new Vector3(0f, 0.78f, 0f), new Vector3(1.8f, 0.12f, 0.75f), wood);
                foreach (Vector3 offset in new[] { new Vector3(-0.72f, 0.38f, -0.27f), new Vector3(0.72f, 0.38f, -0.27f), new Vector3(-0.72f, 0.38f, 0.27f), new Vector3(0.72f, 0.38f, 0.27f) })
                {
                    Primitive(PrimitiveType.Cube, "Leg", root.transform, offset, new Vector3(0.09f, 0.76f, 0.09f), metal);
                }
            }
            return SavePrefab(root, $"{PrefabPath}/TablePlaceholder.prefab");
        }

        private static GameObject CreateBoothPrefab(Material white, Material accent)
        {
            GameObject root = new GameObject("BoothPlaceholder");
            if (AddTeamModel(root.transform, TeamBoothPath, Quaternion.identity) == null)
            {
                Primitive(PrimitiveType.Cube, "Back", root.transform, new Vector3(0f, 1f, 0.55f), new Vector3(1.8f, 2f, 0.1f), white);
                Primitive(PrimitiveType.Cube, "Left", root.transform, new Vector3(-0.85f, 1f, 0f), new Vector3(0.1f, 2f, 1.2f), white);
                Primitive(PrimitiveType.Cube, "Right", root.transform, new Vector3(0.85f, 1f, 0f), new Vector3(0.1f, 2f, 1.2f), white);
                Primitive(PrimitiveType.Cube, "Shelf", root.transform, new Vector3(0f, 0.9f, 0.1f), new Vector3(1.6f, 0.08f, 0.75f), accent);
            }
            return SavePrefab(root, $"{PrefabPath}/BoothPlaceholder.prefab");
        }

        private static GameObject CreateBallotBoxPrefab(Material white, Material accent)
        {
            GameObject root = new GameObject("BallotBoxPlaceholder");
            if (AddTeamModel(root.transform, TeamBallotBoxPath, Quaternion.identity) == null)
            {
                Primitive(PrimitiveType.Cube, "Box", root.transform, new Vector3(0f, 0.58f, 0f), new Vector3(0.85f, 1.05f, 0.85f), white);
                Primitive(PrimitiveType.Cube, "Lid", root.transform, new Vector3(0f, 1.12f, 0f), new Vector3(0.95f, 0.08f, 0.95f), accent);
                Primitive(PrimitiveType.Cube, "Slot", root.transform, new Vector3(0f, 1.17f, 0f), new Vector3(0.45f, 0.02f, 0.08f), ColorMaterial(Color.black));
            }
            return SavePrefab(root, $"{PrefabPath}/BallotBoxPlaceholder.prefab");
        }

        private static GameObject AddTeamModel(Transform parent, string assetPath, Quaternion rotation)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset == null) return null;

            GameObject instance = Object.Instantiate(asset, parent);
            instance.name = asset.name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = rotation;
            instance.transform.localScale = Vector3.one;
            return instance;
        }

        private static void FitModelHeight(GameObject model, float desiredHeight)
        {
            Bounds bounds = RendererBounds(new List<Transform> { model.transform });
            if (bounds.size.y < 0.0001f) return;

            float scale = desiredHeight / bounds.size.y;
            model.transform.localScale = Vector3.one * scale;
            Bounds fitted = RendererBounds(new List<Transform> { model.transform });
            model.transform.position += new Vector3(-fitted.center.x, -fitted.min.y, -fitted.center.z);
        }

        private static void ConfigureTeamVoterAnimator(GameObject model)
        {
            AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(TeamWomanPath)
                .OfType<AnimationClip>()
                .Where(clip => !clip.name.StartsWith("__preview__"))
                .ToArray();
            AnimationClip idleClip = clips.FirstOrDefault(clip => clip.name.EndsWith("|Idle"));
            AnimationClip walkClip = clips.FirstOrDefault(clip => clip.name.EndsWith("|Walk"));
            if (idleClip == null || walkClip == null) return;

            EnsureFolder("Assets/Animations");
            EnsureFolder("Assets/Animations/PollingStation");
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(VoterControllerPath);
            if (controller == null || controller.layers.Length == 0)
            {
                if (controller != null)
                {
                    AssetDatabase.DeleteAsset(VoterControllerPath);
                }
                controller = AnimatorController.CreateAnimatorControllerAtPath(VoterControllerPath);
            }

            if (!controller.parameters.Any(parameter => parameter.name == "Moving"))
            {
                controller.AddParameter("Moving", AnimatorControllerParameterType.Bool);
            }

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState idleState = stateMachine.states.Select(item => item.state).FirstOrDefault(state => state.name == "Idle") ?? stateMachine.AddState("Idle");
            AnimatorState walkState = stateMachine.states.Select(item => item.state).FirstOrDefault(state => state.name == "Walk") ?? stateMachine.AddState("Walk");
            idleState.motion = idleClip;
            walkState.motion = walkClip;
            stateMachine.defaultState = idleState;

            if (!idleState.transitions.Any(transition => transition.destinationState == walkState))
            {
                AnimatorStateTransition transition = idleState.AddTransition(walkState);
                transition.hasExitTime = false;
                transition.duration = 0.15f;
                transition.AddCondition(AnimatorConditionMode.If, 0f, "Moving");
            }
            if (!walkState.transitions.Any(transition => transition.destinationState == idleState))
            {
                AnimatorStateTransition transition = walkState.AddTransition(idleState);
                transition.hasExitTime = false;
                transition.duration = 0.15f;
                transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "Moving");
            }

            Animator animator = model.GetComponent<Animator>();
            if (animator == null)
            {
                animator = model.AddComponent<Animator>();
            }
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            EditorUtility.SetDirty(controller);
        }

        private static GameObject SavePrefab(GameObject source, string path)
        {
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(source, path);
            Object.DestroyImmediate(source);
            return saved;
        }

        private static void InstantiatePrefab(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation, string name)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.gameObject.scene);
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
        }

        private static Material CreateMaterial(string name, Color color, float metallic, float smoothness)
        {
            string path = $"{MaterialPath}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material ColorMaterial(Color color)
        {
            string key = ColorUtility.ToHtmlStringRGB(color);
            return CreateMaterial($"Generated_{key}", color, 0f, 0.2f);
        }

        private static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject result = GameObject.CreatePrimitive(type);
            result.name = name;
            result.transform.SetParent(parent, false);
            result.transform.localPosition = position;
            result.transform.localScale = scale;
            Renderer renderer = result.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            return result;
        }

        private static GameObject DecorativePrimitive(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject result = Primitive(type, name, parent, position, scale, material);
            Collider collider = result.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
            return result;
        }

        private static GameObject Child(GameObject parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            return child;
        }

        private static Transform Point(Transform parent, string name, Vector3 position)
        {
            GameObject point = new GameObject(name);
            point.transform.SetParent(parent, false);
            point.transform.position = position;
            return point.transform;
        }

        private static void CreateSign(Transform parent, string name, string text, Vector3 position, Vector3 scale, Material material, Color textColor, float characterSize)
        {
            GameObject sign = DecorativePrimitive(PrimitiveType.Cube, name, parent, position, scale, material);
            AddFrontLabel(sign.transform, text, textColor, characterSize);
        }

        private static void AddFrontLabel(Transform sign, string text, Color textColor, float characterSize)
        {
            GameObject label = new GameObject("Label");
            label.transform.SetParent(sign, false);
            label.transform.localPosition = new Vector3(0f, 0f, 0.56f);
            label.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            label.transform.localScale = new Vector3(1f / sign.localScale.x, 1f / sign.localScale.y, 1f);
            TextMesh textMesh = label.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = characterSize;
            textMesh.fontSize = 64;
            textMesh.color = textColor;
        }

        private static GameObject UiPanel(Transform parent, string name, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static Text UiText(Transform parent, string name, string value, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 pivot)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            rect.pivot = pivot;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int separator = path.LastIndexOf('/');
            string parent = path.Substring(0, separator);
            string name = path.Substring(separator + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void AddSceneToBuildSettings(string path)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(scene => scene.path == path)) return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
