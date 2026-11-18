#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dino.Utility.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

[DefaultExecutionOrder(-100)]
internal sealed class RobotRunnerSmoke : MonoBehaviour
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly string[] Prefs = { "BestScore", "GameVolume", "MusicEnabled", "SFXEnabled" };
    private readonly Dictionary<string, float?> saved = new();
    private readonly List<string> findings = new();
    private readonly List<string> errors = new();
    private readonly List<ObstacleMover> threats = new(10);
    private string directory;
    private Touchscreen touch;
    private int contact;
    private RunnerController runner;
    private Action<bool> driveJump;
    private RunnerJump profile;
    private ObstacleSpawner spawner;
    private ObstacleMover cleared;
    private bool autoplay;
    private int maxActive;
    private int timedJumps;
    private readonly HashSet<ObstacleKind> testedKinds = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartOnRequest()
    {
        string[] args = Environment.GetCommandLineArgs();
        int argument = Array.IndexOf(args, "-rr-smoke");
        if (argument < 0 || args.Length <= argument + 1 || FindFirstObjectByType<RobotRunnerSmoke>() != null)
            return;

        var probe = new GameObject("Robot Runner smoke test").AddComponent<RobotRunnerSmoke>();
        probe.directory = Path.GetFullPath(args[argument + 1]);
        DontDestroyOnLoad(probe.gameObject);
        Application.runInBackground = true;
        probe.StartCoroutine(probe.Run());
    }

    private IEnumerator Run()
    {
        Directory.CreateDirectory(directory);
        InputSettings.BackgroundBehavior previousFocusBehavior = InputSystem.settings.backgroundBehavior;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        touch = InputSystem.AddDevice<Touchscreen>();
        foreach (string key in Prefs)
            saved[key] = PlayerPrefs.HasKey(key) ? key == "GameVolume" ? PlayerPrefs.GetFloat(key) : PlayerPrefs.GetInt(key) : null;
        Application.logMessageReceived += OnLog;
        var routines = new Stack<IEnumerator>();
        routines.Push(Exercise());
        Exception failure = null;
        while (routines.Count > 0)
        {
            IEnumerator current = routines.Peek();
            object yielded;
            try
            {
                if (!current.MoveNext())
                {
                    routines.Pop();
                    continue;
                }
                yielded = current.Current;
            }
            catch (Exception error)
            {
                failure = error;
                break;
            }

            if (yielded is IEnumerator nested && yielded is not CustomYieldInstruction)
                routines.Push(nested);
            else
                yield return yielded;
        }

        if (failure != null)
            findings.Add("FAIL " + failure);
        findings.AddRange(errors);
        bool success = failure == null && errors.Count == 0;
        findings.Add(success ? "PASS" : "FAIL");
        File.WriteAllLines(Path.Combine(directory, "smoke-test.txt"), findings);
        InputSystem.RemoveDevice(touch);
        InputSystem.settings.backgroundBehavior = previousFocusBehavior;
        foreach (var entry in saved)
        {
            if (!entry.Value.HasValue)
                PlayerPrefs.DeleteKey(entry.Key);
            else if (entry.Key == "GameVolume")
                PlayerPrefs.SetFloat(entry.Key, entry.Value.Value);
            else
                PlayerPrefs.SetInt(entry.Key, (int)entry.Value.Value);
        }
        PlayerPrefs.Save();
        Application.logMessageReceived -= OnLog;
        Application.Quit(success ? 0 : 1);
    }

    private IEnumerator Exercise()
    {
        yield return new WaitForSecondsRealtime(2f);
        GameManager game = GameManager.Instance;
        Check(Window("mainmenuui").CanvasGroup.alpha > 0.99f && !game.HasStarted, "Main menu loads.");
        PlayerPrefs.SetInt("BestScore", 15);
        typeof(GameManager).GetField("bestScore", Private).SetValue(game, 15);
        yield return CaptureLayout("02-menu-tablet", 1200, 900);
        yield return CaptureLayout("01-menu-mobile", 1280, 590);
        yield return CaptureLayout("03-menu-android", 1440, 648);
        yield return CaptureLayout("03b-menu-16x9", 1280, 720);

        yield return ClickAt(ButtonIn("mainmenuui", "SettingsButton").transform.position);
        yield return new WaitForSecondsRealtime(0.55f);
        UIWindow settings = Window("settingsui");
        Check(settings.CanvasGroup.alpha > 0.99f, "Settings opens from the main menu's pointer target.");
        var labels = settings.GetComponentsInChildren<TMP_Text>().Where(text =>
            text.name == "MusicLabel" || text.name == "SFXLabel" || text.name == "VolumeLabel").ToArray();
        Check(labels.Length == 3 && labels.All(text => text.rectTransform.rect.height > 40f &&
            text.transform.parent.name == "SettingsPanel"), "All three audio labels have room in the settings panel.");
        Toggle music = settings.GetComponentsInChildren<Toggle>().Single(toggle => toggle.name == "MusicToggle");
        Toggle sfx = settings.GetComponentsInChildren<Toggle>().Single(toggle => toggle.name == "SFXToggle");
        Slider volume = settings.GetComponentInChildren<Slider>();
        music.isOn = true;
        sfx.isOn = true;
        yield return ClickAt(music.transform.position);
        yield return ClickAt(sfx.transform.position);
        var volumeCorners = new Vector3[4];
        ((RectTransform)volume.handleRect.parent).GetWorldCorners(volumeCorners);
        Vector3 volumeTap = Vector3.Lerp(volumeCorners[0], volumeCorners[2], 0.31f);
        volumeTap.y = volume.transform.position.y;
        yield return ClickAt(volumeTap);
        float savedVolume = volume.value;
        Check(PlayerPrefs.GetInt("MusicEnabled", 1) == 0 &&
            PlayerPrefs.GetInt("SFXEnabled", 1) == 0 &&
            Mathf.Abs(savedVolume - 0.31f) < 0.05f &&
            Mathf.Abs(PlayerPrefs.GetFloat("GameVolume") - savedVolume) < 0.01f,
            "The toggle and volume pointer targets persist their values.");
        Check(AudioManager.Instance.AudioManagerData.MusicAudioMixerGroup.audioMixer.GetFloat("MusicVolume", out float muted) &&
            muted < -79f, "Music toggle controls the mixer.");
        yield return CaptureLayout("04-settings", 1440, 648);
        yield return ClickAt(ButtonIn("settingsui", "BackButton").transform.position);
        yield return new WaitForSecondsRealtime(0.24f);
        yield return ClickAt(ButtonIn("mainmenuui", "CreditsButton").transform.position);
        yield return new WaitForSecondsRealtime(0.35f);
        Check(Window("creditsui").CanvasGroup.alpha > 0.99f, "Credits opens and renders.");
        yield return CaptureLayout("05-credits", 1200, 900);
        yield return ClickAt(ButtonIn("creditsui", "BackButton").transform.position);
        yield return new WaitForSecondsRealtime(0.24f);

        yield return ClickAt(ButtonIn("mainmenuui", "PlayButton").transform.position);
        yield return new WaitForSecondsRealtime(0.28f);
        Check(game.IsPlaying, "PLAY starts gameplay.");
        foreach (string name in new[] { "LivesText", "ScoreHUDText" })
        {
            var text = GameObject.Find(name).GetComponent<TMP_Text>();
            Transform frame = text.transform.parent.Find(name + " Frame");
            Check(text.isActiveAndEnabled && frame.GetSiblingIndex() < text.transform.GetSiblingIndex(),
                name + " renders above its HUD frame.");
        }
        runner = FindFirstObjectByType<RunnerController>();
        profile = runner.GetJumpProfile();
        spawner = FindFirstObjectByType<ObstacleSpawner>();
        spawner.ValidateCourse();
        Check(spawner.enabled, "Course enabled after checking collider envelopes at 61 speeds.");
        // Delay spawns until the touch and UI assertions finish.
        typeof(ObstacleSpawner).GetField("nextSpawnTime", Private).SetValue(spawner, 8f);
        Vector2 openSpace = new(Screen.width * 0.50f, Screen.height * 0.29f);
        int jumps = 0;
        runner.Jumped += () => jumps++;
        float ground = runner.transform.position.y;
        yield return Jump(openSpace, 0.045f);
        float shortApex = apex - ground;
        yield return Jump(openSpace, 0.27f);
        float longApex = apex - ground;
        Check(jumps == 2 && shortApex > 0.8f && longApex > shortApex + 0.5f,
            $"Actual touch jumps produce distinct heights ({shortApex:F2} and {longApex:F2} units).");

        Vector2 pausePosition = GameObject.Find("PauseButton").transform.position;
        yield return TouchClick(pausePosition);
        Check(jumps == 2, "A synthetic touch over Pause is filtered from jump input.");
        if (game.IsPlaying)
            yield return ClickAt(pausePosition);
        yield return new WaitForSecondsRealtime(0.36f);
        int pausedScore = game.Score;
        Check(!game.IsPlaying && Window("pauseui").CanvasGroup.alpha > 0.99f && jumps == 2, "Touching Pause does not jump.");
        yield return TouchClick(new Vector2(Screen.width * 0.06f, Screen.height * 0.36f));
        Check(game.Score == pausedScore && jumps == 2, "Paused touch and score remain idle.");
        yield return CaptureLayout("06-pause", 1280, 590);
        yield return ClickAt(ButtonIn("pauseui", "SettingsButton").transform.position);
        yield return new WaitForSecondsRealtime(0.35f);
        Check(Window("settingsui").CanvasGroup.alpha > 0.99f && !game.IsPlaying, "Settings also opens over Pause.");
        yield return ClickAt(ButtonIn("settingsui", "BackButton").transform.position);
        yield return new WaitForSecondsRealtime(0.24f);
        Check(Window("pauseui").CanvasGroup.alpha > 0.99f && !game.IsPlaying, "Back from Settings returns to Pause.");
        yield return ClickAt(ButtonIn("pauseui", "ResumeButton").transform.position);
        yield return new WaitForSecondsRealtime(0.25f);
        Check(GameManager.Instance == game && game.IsPlaying && runner.Grounded && jumps == 2,
            "Resume restores the run without a queued jump.");

        MethodInfo tap = typeof(RunnerController).GetMethod("HandleTap", Private);
        driveJump = (Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>), runner, tap);
        autoplay = true;
        yield return new WaitUntil(() => spawner.ActiveCount > 0);
        yield return new WaitForSeconds(0.42f);
        yield return CaptureLayout("07-gameplay-night", 1280, 590);
        yield return CaptureLayout("07b-gameplay-tablet", 1200, 900);
        Time.timeScale = 4f;
        FactoryBackdrop world = FindFirstObjectByType<FactoryBackdrop>();
        foreach (string file in new[] { "08-gameplay-day", "09-gameplay-hot", "10-gameplay-toxic" })
        {
            int expectedBiome = world.BiomeIndex + 1;
            yield return new WaitUntil(() => world.BiomeIndex == expectedBiome || game.IsGameOver);
            Check(game.IsPlaying, "Autoplay survived to sector " + expectedBiome + ".");
            yield return new WaitForSeconds(3.2f);
            yield return CaptureLayout(file, 1280, 590);
        }
        yield return new WaitUntil(() => spawner.SurvivalTime >= 300f || game.IsGameOver);
        Time.timeScale = 1f;
        autoplay = false;
        driveJump(false);
        Check(game.IsPlaying && game.Lives == 2 && testedKinds.Count == 3, "Autoplay crossed all three collider types without taking damage.");
        Check(spawner.AllocatedCount <= 12 && maxActive <= 6,
            $"300 in-game seconds at all difficulty tiers. {timedJumps} planned jumps, {maxActive} simultaneously active obstacles, {spawner.AllocatedCount} pooled instances.");

        yield return new WaitUntil(() => runner.Grounded);
        spawner.enabled = false;
        var prefabs = (GameObject[])typeof(ObstacleSpawner).GetField("obstaclePrefabs", Private).GetValue(spawner);
        GameObject boxes = prefabs.Single(prefab => prefab.GetComponent<ObstacleMover>().Kind == ObstacleKind.Boxes);
        ObstacleMover first = MoveIntoRunner(boxes);
        yield return new WaitForSeconds(0.14f);
        Check(game.Lives == 1, "Triple-box contact consumes exactly one life.");
        ObstacleMover second = MoveIntoRunner(boxes);
        yield return new WaitForSeconds(0.14f);
        Check(game.Lives == 1, "A second overlap during invulnerability consumes no life.");
        Destroy(first.gameObject);
        Destroy(second.gameObject);
        yield return new WaitForSeconds(1.6f);
        MoveIntoRunner(boxes);
        yield return new WaitForSecondsRealtime(0.85f);
        Check(game.IsGameOver && game.Lives == 0 && PlayerPrefs.GetInt("BestScore") == game.Score,
            "Fatal collision opens Game Over and saves the record.");
        yield return CaptureLayout("11-game-over", 1440, 648);
        yield return ClickAt(ButtonIn("gameoverui", "PlayAgainButton").transform.position);
        yield return new WaitForSecondsRealtime(0.55f);
        Check(GameManager.Instance != game && GameManager.Instance.IsPlaying &&
            GameManager.Instance.Lives == 2 && FindObjectsByType<AudioManager>().Length == 1,
            "Play Again reloads with two lives and one persistent audio manager.");
        yield return ClickAt(GameObject.Find("PauseButton").transform.position);
        yield return new WaitForSecondsRealtime(0.36f);
        yield return ClickAt(ButtonIn("pauseui", "MainMenuButton").transform.position);
        yield return new WaitForSecondsRealtime(0.65f);
        Check(!GameManager.Instance.HasStarted && Window("mainmenuui").CanvasGroup.alpha > 0.99f,
            "Return to Main Menu works after replay.");
        yield return ClickAt(ButtonIn("mainmenuui", "SettingsButton").transform.position);
        yield return new WaitForSecondsRealtime(0.35f);
        settings = Window("settingsui");
        Check(settings.GetComponentsInChildren<Toggle>().All(toggle => !toggle.isOn) &&
            Mathf.Abs(settings.GetComponentInChildren<Slider>().value - savedVolume) < 0.01f,
            "Settings controls restore saved state across scene reload.");
    }

    private float apex;

    private IEnumerator Jump(Vector2 position, float hold)
    {
        SetTouch(position, TouchPhase.Began);
        float start = Time.unscaledTime;
        apex = float.NegativeInfinity;
        while (Time.unscaledTime - start < hold)
        {
            apex = Mathf.Max(apex, runner.transform.position.y);
            yield return null;
        }
        SetTouch(position, TouchPhase.Ended);
        while (Time.unscaledTime - start < 1.15f)
        {
            apex = Mathf.Max(apex, runner.transform.position.y);
            yield return null;
        }
    }

    private void FixedUpdate()
    {
        if (!autoplay || !GameManager.Instance.IsPlaying)
            return;

        maxActive = Mathf.Max(maxActive, spawner.ActiveCount);
        if (cleared != null && cleared.X < runner.transform.position.x - 1.5f)
        {
            driveJump(false);
            cleared = null;
        }
        if (cleared != null || !runner.Grounded)
            return;

        threats.Clear();
        spawner.GetComponentsInChildren(false, threats);
        foreach (ObstacleMover obstacle in threats)
        {
            ObstacleEnvelope envelope = obstacle.Measure();
            float distance = obstacle.X + (envelope.Left + envelope.Right) * 0.5f - runner.transform.position.x;
            if (distance < 0.5f || distance > 5f)
                continue;
            testedKinds.Add(obstacle.Kind);
            if (obstacle.Kind == ObstacleKind.Drone)
                continue;

            bool hold = obstacle.Kind == ObstacleKind.Boxes;
            Vector2 window = profile.ClearanceWindow(envelope.Top + 0.045f, hold);
            float lead = (window.x + window.y) * 0.5f + profile.Step * 0.5f;
            if (distance > spawner.Speed * lead)
                continue;
            driveJump(true);
            if (!hold)
                driveJump(false);
            cleared = obstacle;
            timedJumps++;
            break;
        }
    }

    private ObstacleMover MoveIntoRunner(GameObject prefab)
    {
        ObstacleMover obstacle = Instantiate(prefab).GetComponent<ObstacleMover>();
        obstacle.Launch(new Vector2(runner.transform.position.x, runner.transform.position.y - 0.5f));
        return obstacle;
    }

    private IEnumerator CaptureLayout(string name, int width, int height)
    {
        if (Screen.width != width || Screen.height != height)
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
        yield return new WaitForSecondsRealtime(0.54f);
        Check(Screen.width == width && Screen.height == height, $"{name} captured at {Screen.width} × {Screen.height}.");
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(directory, name + ".png"));
        yield return new WaitForSecondsRealtime(0.23f);
    }

    private IEnumerator ClickAt(Vector2 position)
    {
        EventSystem system = EventSystem.current;
        var pointer = new PointerEventData(system)
        {
            position = position,
            button = PointerEventData.InputButton.Left
        };
        var results = new List<RaycastResult>();
        system.RaycastAll(pointer, results);
        Check(results.Count != 0, "The control is reachable via Unity's graphic raycaster.");
        RaycastResult hit = results[0];
        pointer.pointerCurrentRaycast = pointer.pointerPressRaycast = hit;
        GameObject pressed = ExecuteEvents.ExecuteHierarchy(hit.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        if (pressed != null)
            ExecuteEvents.Execute(pressed, pointer, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.ExecuteHierarchy(hit.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return null;
    }

    private IEnumerator TouchClick(Vector2 position)
    {
        SetTouch(position, TouchPhase.Began);
        yield return null;
        yield return null;
        yield return new WaitForSecondsRealtime(0.10f);
        SetTouch(position, TouchPhase.Ended);
        yield return null;
        yield return null;
        yield return new WaitForSecondsRealtime(0.10f);
    }

    private void SetTouch(Vector2 position, TouchPhase phase)
    {
        if (phase == TouchPhase.Began)
            contact++;
        InputSystem.QueueStateEvent(touch, new TouchState { touchId = contact, position = position, phase = phase });
    }

    private static UIWindow Window(string id) => UIManager.Instance.GetWindow(id);

    private static Button ButtonIn(string id, string name) =>
        Window(id).GetComponentsInChildren<Button>(true).Single(button => button.name == name);

    private void Check(bool ok, string description)
    {
        if (!ok)
            throw new InvalidOperationException(description);
        findings.Add(description);
    }

    private void OnLog(string message, string stack, LogType type)
    {
        if (type is LogType.Error or LogType.Assert or LogType.Exception)
            errors.Add(type + " " + message + "\n" + stack);
    }
}
#endif
