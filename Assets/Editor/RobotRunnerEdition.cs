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
        spawn.FindProperty("baseObstacleSpeed").floatValue = 5.6f;
        spawn.FindProperty("speedIncreasePerLevel").floatValue = 0.75f;
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
        Debug.Log("Professional Edition integrated into SampleScene. Course clearance validated at 61 speeds.");
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

    public static void BuildAndroid() => Build(BuildTarget.Android, "RobotRunner_Professional_Test.apk", BuildOptions.None);
    public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "RobotRunner.exe", BuildOptions.Development);

    public static void IntegrateAndBuildWindows()
    {
        Integrate();
        BuildWindows();
    }

    public static void RefreshInterfaceAndBuildWindows()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        FactoryArt art = AssetDatabase.LoadAssetAtPath<FactoryArt>(AssetRoot + "/FactoryArt.asset");
        RunnerInterfaceImporter.Build(scene, art, Find<FactoryBackdrop>(scene));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        BuildWindows();
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
        foreach (Transform item in roots.SelectMany(root => root.GetComponentsInChildren<Transform>(true)))
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item.gameObject) > 0)
                throw new InvalidOperationException($"Missing script on {item.name}.");

        string configuredOutput = Environment.GetEnvironmentVariable("ROBOT_RUNNER_BUILD_DIR");
        string output = string.IsNullOrWhiteSpace(configuredOutput)
            ? Path.GetFullPath("Builds/ProfessionalEdition") : Path.GetFullPath(configuredOutput);
        Directory.CreateDirectory(output);
        if (target == BuildTarget.Android)
            EditorUserBuildSettings.buildAppBundle = false;
        BuildReport report = BuildPipeline.BuildPlayer(new[] { ScenePath }, Path.Combine(output, fileName), target, options);
        Debug.Log($"Robot Runner build: {report.summary.result}, {report.summary.totalSize} bytes, {report.summary.outputPath}");
        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException($"Build failed with {report.summary.totalErrors} errors.");
    }
}
