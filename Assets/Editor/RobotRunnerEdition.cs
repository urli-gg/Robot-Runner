using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class RobotRunnerEdition
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string AssetRoot = FactoryArtImporter.Output;

    [MenuItem("Robot Runner/Rebuild factory presentation")]
    public static void Integrate()
    {
        FactoryArt art = FactoryArtImporter.Build();
        Material sprites = SpriteMaterial();
        ConfigureObstacle("SawObstacle", ObstacleKind.Saw, art, sprites);
        ConfigureObstacle("TripleBoxObstacle", ObstacleKind.Boxes, art, sprites);
        ConfigureObstacle("DroneObstacle", ObstacleKind.Drone, art, sprites);

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Camera camera = Find<Camera>(scene);
        camera.orthographicSize = 4.4f;
        camera.transform.position = new Vector3(0f, 0.04f, -10f);
        RunnerController runner = Find<RunnerController>(scene);
        runner.transform.position = new Vector3(-3.8f, -2f, 0f);
        runner.GetComponent<SpriteRenderer>().enabled = false;
        Transform visual = Child(runner.transform, "Robot frames");
        SpriteRenderer robot = SetRenderer(visual.gameObject, art.idle[0], sprites, 3);
        Rigidbody2D playerBody = runner.GetComponent<Rigidbody2D>();
        playerBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var presentation = new SerializedObject(GetOrAdd<RunnerPresentation>(runner.gameObject));
        presentation.FindProperty("art").objectReferenceValue = art;
        presentation.FindProperty("robot").objectReferenceValue = robot;
        presentation.FindProperty("spriteMaterial").objectReferenceValue = sprites;
        presentation.ApplyModifiedPropertiesWithoutUndo();

        foreach (TilemapRenderer map in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<TilemapRenderer>(true)))
            if (map.name == "FloorVisual" || map.name == "BackGroundWalls")
                map.enabled = false;

        ObstacleSpawner spawner = Find<ObstacleSpawner>(scene);
        var spawn = new SerializedObject(spawner);
        var point = (Transform)spawn.FindProperty("spawnPoint").objectReferenceValue;
        var ground = (Collider2D)new SerializedObject(runner).FindProperty("groundCollider").objectReferenceValue;
        float track = ground.bounds.max.y;
        point.position = new Vector3(point.position.x, track, 0f);
        spawn.FindProperty("runner").objectReferenceValue = runner;
        spawn.FindProperty("worldCamera").objectReferenceValue = camera;
        SerializedProperty catalog = spawn.FindProperty("obstaclePrefabs");
        if (catalog.arraySize == 3)
        {
            spawn.FindProperty("secondsPerLevel").floatValue = 18f;
            spawn.FindProperty("speedIncreasePerLevel").floatValue = 1.27f;
        }
        WireHazards(catalog, art, sprites);
        spawn.ApplyModifiedPropertiesWithoutUndo();

        GameObject factory = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Factory Scenery");
        if (factory == null)
            factory = new GameObject("Factory Scenery");
        var sceneArt = new SerializedObject(GetOrAdd<FactoryBackdrop>(factory));
        sceneArt.FindProperty("art").objectReferenceValue = art;
        sceneArt.FindProperty("worldCamera").objectReferenceValue = camera;
        sceneArt.FindProperty("spawner").objectReferenceValue = spawner;
        sceneArt.FindProperty("spriteMaterial").objectReferenceValue = sprites;
        sceneArt.FindProperty("trackHeight").floatValue = track;
        sceneArt.ApplyModifiedPropertiesWithoutUndo();

        RunnerInterfaceImporter.Build(scene, art, Find<FactoryBackdrop>(scene));
        RunnerAudioImporter.Build();
        spawner.ValidateCourse();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Advanced obstacles integrated into SampleScene. Clearance and pattern validation passed.");
    }

    private static void WireHazards(SerializedProperty catalog, FactoryArt art, Material material)
    {
        (ObstacleKind Kind, string Name, Vector2 Hitbox, float Bottom, int Level, bool Hold, Vector4 Weights)[] hazards =
        {
            (ObstacleKind.Patrol, "PatrolObstacle", new(0.67f, 0.78f), 0.01f, 0, true, new(0.8f, 1.2f, 1.1f, 1f)),
            (ObstacleKind.Barrier, "ElectricBarrierObstacle", new(0.94f, 0.62f), 0f, 1, true, new(0f, 1f, 1.2f, 1.2f)),
            (ObstacleKind.Press, "HydraulicPressObstacle", new(0.48f, 1.16f), 1.20f, 2, false, new(0f, 0f, 1.1f, 1.3f)),
            (ObstacleKind.Roller, "IndustrialRollerObstacle", new(0.90f, 0.32f), 0f, 1, false, new(0f, 0.8f, 1.1f, 1.3f))
        };

        Sprite warningPlate = MakeWarningPlate();
        foreach (var hazard in hazards)
        {
            GameObject prefab = BuildHazard(hazard.Kind, hazard.Name, hazard.Hitbox, hazard.Bottom,
                hazard.Level, hazard.Hold, hazard.Weights, art, material, warningPlate);
            int index = 0;
            while (index < catalog.arraySize && catalog.GetArrayElementAtIndex(index).objectReferenceValue != prefab)
                index++;
            if (index == catalog.arraySize)
                catalog.arraySize++;
            catalog.GetArrayElementAtIndex(index).objectReferenceValue = prefab;
        }
    }

    private static GameObject BuildHazard(ObstacleKind kind, string name, Vector2 size, float bottom,
        int firstLevel, bool hold, Vector4 weights, FactoryArt art, Material material, Sprite warningPlate)
    {
        string path = $"Assets/Prefab/{name}.prefab";
        bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
        GameObject root = exists ? PrefabUtility.LoadPrefabContents(path) : new GameObject(name);
        try
        {
            var body = GetOrAdd<Rigidbody2D>(root);
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var hit = GetOrAdd<BoxCollider2D>(root);
            hit.isTrigger = true;
            hit.size = size;
            hit.offset = new Vector2(0f, bottom + size.y * 0.5f);
            GetOrAdd<ObstacleHit>(root);

            var mover = new SerializedObject(GetOrAdd<ObstacleMover>(root));
            mover.FindProperty("kind").enumValueIndex = (int)kind;
            if (!exists)
            {
                mover.FindProperty("firstLevel").intValue = firstLevel;
                mover.FindProperty("holdJump").boolValue = hold;
                mover.FindProperty("stageWeights").vector4Value = weights;
            }
            mover.ApplyModifiedPropertiesWithoutUndo();

            var animation = new SerializedObject(GetOrAdd<ObstacleAnimation>(root));
            animation.FindProperty("kind").enumValueIndex = (int)kind;
            SerializedProperty biomes = animation.FindProperty("biomeClips");
            biomes.arraySize = art.biomes.Length;
            SpriteRenderer[] visuals = null;
            for (int biome = 0; biome < art.biomes.Length; biome++)
            {
                Sprite[] secondary = Array.Empty<Sprite>();
                Sprite[] frames = kind switch
                {
                    ObstacleKind.Patrol => biome == 0 ? art.run :
                        FactoryArtImporter.HazardClip(biome, "enemies_animated", "Bot_Patrol_Run", 100),
                    ObstacleKind.Roller => art.biomes[biome].conveyor,
                    ObstacleKind.Press => art.biomes[biome].arm,
                    _ => FactoryArtImporter.HazardClip(biome, "props_animated", "Toggle_Activation_PingPong_")
                };
                if (kind == ObstacleKind.Barrier)
                    secondary = FactoryArtImporter.HazardClip(biome, "vfx_animated", "SparkA_");

                SerializedProperty clip = biomes.GetArrayElementAtIndex(biome);
                SetReferences(clip.FindPropertyRelative("frames"), frames);
                SetReferences(clip.FindPropertyRelative("secondary"), secondary);
                if (biome != 0)
                    continue;

                visuals = kind switch
                {
                    ObstacleKind.Patrol => new[] { HazardVisual(root, "Patrol frames", frames[0], material,
                        new Vector2(0f, 0.50f), new Vector2(3.35f, 3.55f)) },
                    ObstacleKind.Roller => new[] { HazardVisual(root, "Conveyor rollers", frames[0], material,
                        new Vector2(0f, 0.48f), new Vector2(0.75f, 0.75f)) },
                    ObstacleKind.Press => new[] { HazardVisual(root, "Hydraulic arm", frames[2], material,
                        new Vector2(0f, bottom + size.y * 0.5f), new Vector2(0.95f, size.y / 1.28f)) },
                    _ => new[]
                    {
                        HazardVisual(root, "Emitter left", frames[0], material, new Vector2(-0.34f, 0.48f), new Vector2(0.58f, 0.75f)),
                        HazardVisual(root, "Emitter right", frames[0], material, new Vector2(0.34f, 0.48f), new Vector2(0.58f, 0.75f)),
                        HazardVisual(root, "Electrical arc", secondary[0], material, new Vector2(0f, 0.28f), new Vector2(0.78f, 0.50f))
                    }
                };
            }

            if (kind == ObstacleKind.Patrol)
                visuals[0].flipX = true;
            if (kind == ObstacleKind.Barrier)
                visuals[1].flipX = true;
            if (kind == ObstacleKind.Press)
            {
                HazardVisual(root, "Warning plate", warningPlate, material,
                    new Vector2(0f, bottom + size.y + 0.04f), Vector2.one).sortingOrder = 2;
                animation.FindProperty("pressHitbox").objectReferenceValue = hit;
                animation.FindProperty("pressTop").floatValue = bottom + size.y;
            }
            SetReferences(animation.FindProperty("visuals"), visuals);
            animation.ApplyModifiedPropertiesWithoutUndo();
            return PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            if (exists)
                PrefabUtility.UnloadPrefabContents(root);
            else
                UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static Sprite MakeWarningPlate()
    {
        string path = AssetRoot + "/Hazard Warning Plate.png";
        var pixels = new FactoryArtImporter.Pixels(22, 8);
        pixels.Frame(0, 0, 22, 8, 1, new Color32(28, 23, 41, 255), new Color32(51, 42, 57, 255));
        for (int y = 1; y < 7; y++)
        for (int x = 2; x < 20; x++)
            if ((x + y) % 6 < 3)
                pixels.Fill(x, y, 1, 1, new Color32(255, 186, 76, 255));
        pixels.Write(path);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        return FactoryArtImporter.ImportComposition(path, new Vector2(0.5f, 0.5f), 30);
    }

    private static SpriteRenderer HazardVisual(GameObject root, string name, Sprite sprite, Material material,
        Vector2 position, Vector2 scale)
    {
        Transform child = Child(root.transform, name);
        child.localPosition = position;
        child.localScale = new Vector3(scale.x, scale.y, 1f);
        return SetRenderer(child.gameObject, sprite, material, 1);
    }

    private static void SetReferences<T>(SerializedProperty array, T[] values) where T : UnityEngine.Object
    {
        array.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    private static void ConfigureObstacle(string name, ObstacleKind kind, FactoryArt art, Material material)
    {
        string path = $"Assets/Prefab/{name}.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            root.transform.localScale = Vector3.one;
            Rigidbody2D body = GetOrAdd<Rigidbody2D>(root);
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var mover = new SerializedObject(root.GetComponent<ObstacleMover>());
            mover.FindProperty("kind").enumValueIndex = (int)kind;
            mover.ApplyModifiedPropertiesWithoutUndo();

            SpriteRenderer[] visuals;
            switch (kind)
            {
                case ObstacleKind.Saw:
                    root.GetComponent<SpriteRenderer>().enabled = false;
                    var circle = root.GetComponent<CircleCollider2D>();
                    circle.radius = 0.245f;
                    circle.offset = new Vector2(0f, 0.255f);
                    Transform saw = Child(root.transform, "Saw frames");
                    saw.localPosition = new Vector3(0f, 0.29f, 0f);
                    saw.localScale = new Vector3(0.46f, 0.46f, 1f);
                    visuals = new[] { SetRenderer(saw.gameObject, art.saw[0], material, 1) };
                    break;
                case ObstacleKind.Boxes:
                    visuals = new SpriteRenderer[3];
                    string[] boxes = { "BoxLeft", "BoxRight", "BoxTop" };
                    for (int i = 0; i < boxes.Length; i++)
                    {
                        Transform box = root.transform.Find(boxes[i]);
                        box.localScale = new Vector3(2.1f, 2.1f, 1f);
                        box.localPosition = new Vector3((i == 2 ? 0 : i * 2 - 1) * 0.29f, i == 2 ? 0.87f : 0.29f, 0f);
                        box.GetComponent<BoxCollider2D>().size = new Vector2(0.267f, 0.267f);
                        visuals[i] = SetRenderer(box.gameObject, art.crates[0], material, 1);
                    }
                    break;
                default:
                    Transform drone = root.transform.Find("DroneVisual");
                    drone.localScale = new Vector3(3f, 3f, 1f);
                    drone.localPosition = new Vector3(0f, 1.56f, 0f);
                    var boxCollider = drone.GetComponent<BoxCollider2D>();
                    boxCollider.size = new Vector2(0.18f, 0.15f);
                    boxCollider.offset = new Vector2(0f, -0.02f);
                    visuals = new[] { SetRenderer(drone.gameObject, art.droneIdle[0], material, 1) };
                    break;
            }

            var animation = new SerializedObject(GetOrAdd<ObstacleAnimation>(root));
            animation.FindProperty("art").objectReferenceValue = art;
            animation.FindProperty("kind").enumValueIndex = (int)kind;
            SerializedProperty array = animation.FindProperty("visuals");
            array.arraySize = visuals.Length;
            for (int i = 0; i < visuals.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = visuals[i];
            animation.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Material SpriteMaterial()
    {
        string path = AssetRoot + "/Factory Sprites.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
            return material;
        var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null)
            throw new InvalidOperationException("URP 2D sprite shader is missing.");
        material = new Material(shader);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    internal static T Find<T>(Scene scene) where T : Component =>
        scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)).Single();

    internal static T GetOrAdd<T>(GameObject target) where T : Component =>
        target.TryGetComponent<T>(out var component) ? component : target.AddComponent<T>();

    private static Transform Child(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
            return child;
        child = new GameObject(name).transform;
        child.SetParent(parent, false);
        return child;
    }

    private static SpriteRenderer SetRenderer(GameObject target, Sprite sprite, Material material, int order)
    {
        SpriteRenderer renderer = GetOrAdd<SpriteRenderer>(target);
        renderer.sprite = sprite;
        renderer.sharedMaterial = material;
        renderer.sortingOrder = order;
        return renderer;
    }

    public static void BuildAndroid() => Build(BuildTarget.Android, "RobotRunner_AdvancedObstacles_Test.apk", BuildOptions.None);
    public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "RobotRunner.exe", BuildOptions.Development);

    public static void IntegrateAndBuildWindows()
    {
        Integrate();
        BuildWindows();
    }

    public static void RefreshInterfaceAndBuildWindows()
    {
        RefreshInterface();
        BuildWindows();
    }

    public static void RefreshInterfaceAndBuildAndroid()
    {
        RefreshInterface();
        BuildAndroid();
    }

    private static void RefreshInterface()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        FactoryArt art = AssetDatabase.LoadAssetAtPath<FactoryArt>(AssetRoot + "/FactoryArt.asset");
        RunnerInterfaceImporter.Build(scene, art, Find<FactoryBackdrop>(scene));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    private static void Build(BuildTarget target, string fileName, BuildOptions options)
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        ObstacleSpawner spawner = Find<ObstacleSpawner>(scene);
        spawner.ValidateCourse();
        var prefabs = new SerializedObject(spawner).FindProperty("obstaclePrefabs");
        var roots = scene.GetRootGameObjects().ToList();
        for (int i = 0; i < prefabs.arraySize; i++)
            roots.Add((GameObject)prefabs.GetArrayElementAtIndex(i).objectReferenceValue);
        ValidateReferences(roots);

        string configuredOutput = Environment.GetEnvironmentVariable("ROBOT_RUNNER_BUILD_DIR");
        string output = string.IsNullOrWhiteSpace(configuredOutput)
            ? Path.GetFullPath("Builds/AdvancedObstacles") : Path.GetFullPath(configuredOutput);
        Directory.CreateDirectory(output);
        if (target == BuildTarget.Android)
            EditorUserBuildSettings.buildAppBundle = false;
        BuildReport report = BuildPipeline.BuildPlayer(new[] { ScenePath }, Path.Combine(output, fileName), target, options);
        Debug.Log($"Robot Runner build: {report.summary.result}, {report.summary.totalSize} bytes, {report.summary.outputPath}");
        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException($"Build failed with {report.summary.totalErrors} errors.");
    }

    private static void ValidateReferences(System.Collections.Generic.List<GameObject> roots)
    {
        foreach (Transform item in roots.SelectMany(root => root.GetComponentsInChildren<Transform>(true)))
        {
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item.gameObject) > 0)
                throw new InvalidOperationException($"Missing script on {item.name}.");
        }

        foreach (ObstacleAnimation animation in roots.SelectMany(root => root.GetComponentsInChildren<ObstacleAnimation>(true)))
        {
            var data = new SerializedObject(animation);
            var values = data.FindProperty("visuals");
            for (int i = 0; i < values.arraySize; i++)
            {
                var renderer = (SpriteRenderer)values.GetArrayElementAtIndex(i).objectReferenceValue;
                if (renderer == null || renderer.sprite == null)
                    throw new InvalidOperationException($"Missing obstacle sprite on {animation.name}.");
            }

            var clips = data.FindProperty("biomeClips");
            for (int biome = 0; biome < clips.arraySize; biome++)
            {
                SerializedProperty clip = clips.GetArrayElementAtIndex(biome);
                foreach (string track in new[] { "frames", "secondary" })
                {
                    var sprites = clip.FindPropertyRelative(track);
                    for (int frame = 0; frame < sprites.arraySize; frame++)
                        if (sprites.GetArrayElementAtIndex(frame).objectReferenceValue == null)
                            throw new InvalidOperationException($"Missing {track} frame on {animation.name}, biome {biome}.");
                }
            }
        }
    }
}
