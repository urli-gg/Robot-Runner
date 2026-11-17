using System;
using System.IO;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

internal static class RunnerInterfaceImporter
{
    private static readonly Color Cyan = new Color32(93, 242, 250, 255);
    private static readonly Color Pink = new Color32(248, 115, 194, 255);
    private static readonly Color Red = new Color32(255, 105, 127, 255);
    private static readonly Color White = new Color32(233, 245, 255, 255);
    private static readonly Color Muted = new Color32(165, 191, 212, 255);
    private static readonly Color Yellow = new Color32(255, 216, 113, 255);
    private const string Root = FactoryArtImporter.Output + "/UI";

    internal static void Build(Scene scene, FactoryArt art, FactoryBackdrop backdrop)
    {
        Directory.CreateDirectory(Root);
        Sprite panel = MakeFrame("Panel", new Color32(11, 18, 35, 250), new Color32(46, 94, 119, 255), Cyan);
        Sprite button = MakeFrame("Button", new Color32(16, 34, 55, 255), new Color32(56, 126, 150, 255), Cyan);
        Sprite primary = MakeFrame("Primary", new Color32(16, 67, 82, 255), Cyan, new Color32(208, 255, 254, 255));
        Sprite pinkPanel = MakeFrame("CreditsPanel", new Color32(25, 17, 39, 250), new Color32(114, 53, 105, 255), Pink);
        Sprite alertPanel = MakeFrame("AlertPanel", new Color32(30, 15, 31, 250), new Color32(135, 48, 65, 255), Red);
        Sprite hero = MakeHero();
        Sprite knob = MakeKnob();

        ConfigurePrefab("MainMenu", 10, (window, content) =>
        {
            RectTransform menu = Panel(window, content, "MainMenuPanel", null, true);
            Background(window).color = new Color32(4, 8, 18, 102);
            Background(window).sprite = null;
            Text(menu, "TitleText", 100, White, TextAlignmentOptions.MidlineLeft, 0.055f, 0.53f, 0.57f, 0.96f)
                .text = "<color=#60F2F8>ROBOT</color>\nRUNNER";

            RectTransform heroPlate = Child(menu, "Robot display");
            Image plate = ImageAt(heroPlate, hero, Color.white);
            plate.type = Image.Type.Simple;
            Dock(heroPlate, 0.53f, 0.08f, 0.98f, 0.95f);
            heroPlate.SetAsFirstSibling();
            RectTransform robot = Named(menu, "RobotImage");
            Dock(robot, 0.57f, 0.13f, 0.95f, 0.91f);
            var portrait = robot.GetComponent<Image>();
            portrait.sprite = art.idle[0];
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;
            SetObject(RobotRunnerEdition.GetOrAdd<MenuRobot>(robot.gameObject), "art", art);
            Action(menu, "PlayButton", primary, 0.055f, 0.36f, 0.41f, 0.49f);
            Action(menu, "SettingsButton", button, 0.055f, 0.215f, 0.41f, 0.337f);
            Action(menu, "CreditsButton", pinkPanel, 0.055f, 0.074f, 0.41f, 0.196f);
            var main = (MainMenuUI)window;
            Bind(menu, "SettingsButton", main.OpenSettings);
            Bind(menu, "CreditsButton", main.OpenCredits);
        });

        ConfigurePrefab("Pause", 20, (window, content) =>
        {
            RectTransform modal = Panel(window, content, "PausePanel", panel);
            Background(window).sprite = null;
            Background(window).color = new Color32(3, 8, 17, 174);
            Text(modal, "TitleText", 62, Cyan, TextAlignmentOptions.Center, 0.1f, 0.73f, 0.9f, 0.95f);
            Action(modal, "ResumeButton", primary, 0.14f, 0.52f, 0.86f, 0.67f);
            Action(modal, "SettingsButton", button, 0.14f, 0.33f, 0.86f, 0.48f);
            Action(modal, "MainMenuButton", pinkPanel, 0.14f, 0.14f, 0.86f, 0.29f);
        });

        ConfigurePrefab("Settings", 30, (window, content) =>
        {
            RectTransform modal = Panel(window, content, "SettingsPanel", panel);
            Background(window).sprite = null;
            Background(window).color = new Color32(3, 8, 17, 175);
            Text(modal, "TitleText", 59, Cyan, TextAlignmentOptions.Center, 0.1f, 0.73f, 0.9f, 0.95f);
            Text(modal, "MusicLabel", 32, White, TextAlignmentOptions.MidlineLeft, 0.14f, 0.57f, 0.49f, 0.69f);
            Text(modal, "SFXLabel", 32, White, TextAlignmentOptions.MidlineLeft, 0.14f, 0.43f, 0.49f, 0.55f);
            Text(modal, "VolumeLabel", 32, White, TextAlignmentOptions.MidlineLeft, 0.14f, 0.29f, 0.49f, 0.41f);
            ConfigureToggle(modal, "MusicToggle", button, knob, 0.57f, 0.69f);
            ConfigureToggle(modal, "SFXToggle", button, knob, 0.43f, 0.55f);
            RectTransform volume = Named(modal, "VolumeSlider");
            Dock(volume, 0.51f, 0.30f, 0.85f, 0.40f);
            Slider slider = volume.GetComponent<Slider>();
            slider.fillRect.GetComponent<Image>().color = Cyan;
            Named(volume, "Background").GetComponent<Image>().raycastTarget = true;
            Image handle = slider.handleRect.GetComponent<Image>();
            handle.sprite = knob;
            handle.type = Image.Type.Simple;
            handle.preserveAspect = true;
            handle.raycastTarget = true;
            handle.color = White;
            slider.handleRect.sizeDelta = new Vector2(27f, 0f);
            Action(modal, "BackButton", button, 0.23f, 0.075f, 0.77f, 0.225f);
        });

        ConfigurePrefab("Credits", 30, (window, content) =>
        {
            RectTransform modal = Panel(window, content, "CreditsPanel", pinkPanel);
            Background(window).sprite = null;
            Background(window).color = new Color32(12, 8, 20, 195);
            Text(modal, "TitleText", 60, Pink, TextAlignmentOptions.Center, 0.1f, 0.74f, 0.9f, 0.95f);
            Text(modal, "DevelopedByText", 24, Muted, TextAlignmentOptions.Center, 0.1f, 0.61f, 0.9f, 0.71f);
            Text(modal, "NameText", 38, White, TextAlignmentOptions.Center, 0.1f, 0.49f, 0.9f, 0.63f);
            Text(modal, "AssetsByText", 24, Muted, TextAlignmentOptions.Center, 0.1f, 0.36f, 0.9f, 0.46f);
            Text(modal, "AssetAuthorText", 36, White, TextAlignmentOptions.Center, 0.1f, 0.24f, 0.9f, 0.37f);
            Action(modal, "BackButton", pinkPanel, 0.23f, 0.075f, 0.77f, 0.215f);
        });

        ConfigurePrefab("GameOver", 20, (window, content) =>
        {
            RectTransform modal = Panel(window, content, "GameOverPanel", alertPanel);
            Text(modal, "TitleText", 64, Red, TextAlignmentOptions.Center, 0.08f, 0.73f, 0.92f, 0.955f);
            Text(modal, "ScoreText", 49, White, TextAlignmentOptions.Center, 0.08f, 0.56f, 0.92f, 0.72f);
            Text(modal, "BestText", 35, Yellow, TextAlignmentOptions.Center, 0.08f, 0.43f, 0.92f, 0.56f);
            Action(modal, "PlayAgainButton", primary, 0.14f, 0.24f, 0.86f, 0.39f);
            Action(modal, "MainMenuButton", alertPanel, 0.14f, 0.07f, 0.86f, 0.22f);
        });

        foreach (UIWindow window in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<UIWindow>(true)))
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(window))
                continue;
            PropertyModification[] overrides = PrefabUtility.GetPropertyModifications(window.gameObject);
            if (overrides == null)
                continue;
            var remaining = overrides.Where(change => !ReplacedOverride(window, change)).ToArray();
            if (remaining.Length != overrides.Length)
                PrefabUtility.SetPropertyModifications(window.gameObject, remaining);
        }

        ConfigureHud(scene, panel, button, backdrop);
    }

    private static bool ReplacedOverride(UIWindow window, PropertyModification change)
    {
        if (change.target is RectTransform)
            return change.propertyPath.StartsWith("m_Anchor", StringComparison.Ordinal) ||
                   change.propertyPath.StartsWith("m_SizeDelta", StringComparison.Ordinal) ||
                   change.propertyPath.StartsWith("m_Pivot", StringComparison.Ordinal);
        return window is MainMenuUI && change.target is Button button &&
               (button.name == "SettingsButton" || button.name == "CreditsButton") &&
               change.propertyPath.StartsWith("m_OnClick.", StringComparison.Ordinal);
    }

    private static void Bind(RectTransform panel, string name, UnityAction action)
    {
        Button.ButtonClickedEvent click = Named(panel, name).GetComponent<Button>().onClick;
        for (int i = click.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(click, i);
        UnityEventTools.AddPersistentListener(click, action);
    }

    private static void ConfigurePrefab(string name, int layer, Action<UIWindow, RectTransform> configure)
    {
        string path = $"Assets/Prefab/{name}UI.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            UIWindow window = root.GetComponent<UIWindow>();
            Canvas canvas = window.CanvasRectTransform.GetComponent<Canvas>();
            ConfigureCanvas(canvas, layer);
            RectTransform group = window.CanvasGroup.GetComponent<RectTransform>();
            Dock(group, 0f, 0f, 1f, 1f);
            RectTransform safe = Child(group, "Safe area");
            Dock(safe, 0f, 0f, 1f, 1f);
            RobotRunnerEdition.GetOrAdd<SafeAreaRect>(safe.gameObject);
            safe.SetAsLastSibling();
            foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = graphic.TryGetComponent<Selectable>(out _);
            if (name != "MainMenu")
                Background(window).raycastTarget = true;
            var properties = new SerializedObject(window);
            properties.FindProperty("_hideOnStart").boolValue = name != "MainMenu";
            properties.FindProperty("showEase").intValue = (int)Ease.OutCubic;
            properties.FindProperty("hideEase").intValue = (int)Ease.InCubic;
            properties.ApplyModifiedPropertiesWithoutUndo();
            configure(window, safe);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ConfigureHud(Scene scene, Sprite panel, Sprite button, FactoryBackdrop backdrop)
    {
        GameManager game = RobotRunnerEdition.Find<GameManager>(scene);
        var properties = new SerializedObject(game);
        var lives = (TMP_Text)properties.FindProperty("livesHUDText").objectReferenceValue;
        var score = (TMP_Text)properties.FindProperty("scoreHUDText").objectReferenceValue;
        var pause = (GameObject)properties.FindProperty("pauseButton").objectReferenceValue;
        Canvas canvas = lives.GetComponentInParent<Canvas>();
        ConfigureCanvas(canvas, 5);
        var safe = Child(canvas.transform, "Safe area");
        Dock(safe, 0f, 0f, 1f, 1f);
        RobotRunnerEdition.GetOrAdd<SafeAreaRect>(safe.gameObject);
        ConfigureHudCard(lives, safe, panel, new Vector2(0f, 1f), new Vector2(26f, -22f), new Vector2(205f, 59f));
        ConfigureHudCard(score, safe, panel, new Vector2(1f, 1f), new Vector2(-127f, -22f), new Vector2(220f, 59f));

        pause.transform.SetParent(safe, false);
        var pauseButton = pause.GetComponent<Button>();
        StyleButton(pauseButton, button);
        Anchor((RectTransform)pause.transform, new Vector2(1f, 1f), new Vector2(-26f, -22f), new Vector2(88f, 59f));
        TMP_Text pauseText = pause.GetComponentInChildren<TMP_Text>(true);
        pauseText.text = "II";
        SetTextStyle(pauseText, 36, White, TextAlignmentOptions.Center);
        Dock(pauseText.rectTransform, 0.03f, 0.06f, 0.97f, 0.94f);

        RectTransform messageRoot = Child(safe, "Run notices");
        Dock(messageRoot, 0.27f, 0.80f, 0.73f, 0.89f);
        var badge = RobotRunnerEdition.GetOrAdd<CanvasGroup>(messageRoot.gameObject);
        badge.blocksRaycasts = false;
        badge.interactable = false;
        ImageAt(messageRoot, panel, new Color32(255, 255, 255, 230));
        RectTransform messageTransform = Child(messageRoot, "Text");
        Dock(messageTransform, 0.045f, 0.05f, 0.955f, 0.95f);
        var message = RobotRunnerEdition.GetOrAdd<TextMeshProUGUI>(messageTransform.gameObject);
        message.font = score.font;
        SetTextStyle(message, 24, White, TextAlignmentOptions.Center);
        var notices = RobotRunnerEdition.GetOrAdd<RunNotifications>(messageRoot.gameObject);
        SetObject(notices, "factory", backdrop);
        SetObject(notices, "badge", badge);
        SetObject(notices, "message", message);
    }

    private static void ConfigureHudCard(TMP_Text text, RectTransform safe, Sprite panel,
        Vector2 point, Vector2 offset, Vector2 size)
    {
        text.transform.SetParent(safe, false);
        Anchor(text.rectTransform, point, offset, size);
        SetTextStyle(text, 27, White, TextAlignmentOptions.Center);
        RectTransform card = Child(safe, text.name + " Frame");
        ImageAt(card, panel, Color.white);
        Anchor(card, point, offset, size);
        card.SetSiblingIndex(text.transform.GetSiblingIndex());
        SetObject(RobotRunnerEdition.GetOrAdd<HudCard>(text.gameObject), "frame", card.gameObject);
    }

    private static void ConfigureCanvas(Canvas canvas, int order)
    {
        canvas.sortingOrder = order;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;
    }

    private static RectTransform Panel(UIWindow window, RectTransform safe, string name, Sprite sprite, bool main = false)
    {
        RectTransform panel = Named(window.CanvasGroup.transform, name);
        panel.SetParent(safe, false);
        Vector2 min = main ? new Vector2(0.04f, 0.075f) : new Vector2(0.26f, 0.095f);
        Vector2 max = main ? new Vector2(0.96f, 0.925f) : new Vector2(0.74f, 0.905f);
        Vector2 tabletMin = main ? new Vector2(0.025f, 0.12f) : new Vector2(0.17f, 0.12f);
        Vector2 tabletMax = main ? new Vector2(0.975f, 0.88f) : new Vector2(0.83f, 0.88f);
        Dock(panel, min.x, min.y, max.x, max.y);
        var responsive = new SerializedObject(panel.GetComponent<ResponsiveElement>());
        responsive.FindProperty("mobileAnchorMin").vector2Value = min;
        responsive.FindProperty("mobileAnchorMax").vector2Value = max;
        responsive.FindProperty("tabletAnchorMin").vector2Value = tabletMin;
        responsive.FindProperty("tabletAnchorMax").vector2Value = tabletMax;
        responsive.ApplyModifiedPropertiesWithoutUndo();
        DisableOutline(panel.gameObject);
        ImageAt(panel, sprite, sprite == null ? Color.clear : Color.white);
        return panel;
    }

    private static void ConfigureToggle(RectTransform panel, string name, Sprite sprite, Sprite knobSprite, float bottom, float top)
    {
        RectTransform target = Named(panel, name);
        Dock(target, 0.59f, bottom, 0.85f, top);
        var toggle = target.GetComponent<Toggle>();
        var background = (Image)toggle.targetGraphic;
        background.sprite = sprite;
        background.type = Image.Type.Sliced;
        background.color = Color.white;
        background.raycastTarget = true;
        Dock(background.rectTransform, 0f, 0f, 1f, 1f);
        var indicator = RobotRunnerEdition.GetOrAdd<ToggleIndicator>(target.gameObject);
        var knob = (Image)toggle.graphic ?? (Image)new SerializedObject(indicator).FindProperty("knob").objectReferenceValue;
        knob.gameObject.SetActive(true);
        knob.raycastTarget = false;
        knob.sprite = knobSprite;
        knob.type = Image.Type.Simple;
        Anchor(knob.rectTransform, new Vector2(0.78f, 0.5f), Vector2.zero, new Vector2(33f, 33f));
        knob.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        toggle.graphic = null;
        TMP_Text state = target.GetComponentInChildren<TMP_Text>(true);
        SetTextStyle(state, 23, White, TextAlignmentOptions.Center);
        Dock(state.rectTransform, 0.08f, 0f, 0.62f, 1f);
        SetObject(indicator, "state", state);
        SetObject(indicator, "knob", knob);
    }

    private static void Action(Transform parent, string name, Sprite sprite, float left, float bottom, float right, float top)
    {
        RectTransform rect = Named(parent, name);
        Dock(rect, left, bottom, right, top);
        Button button = rect.GetComponent<Button>();
        StyleButton(button, sprite);
        var label = rect.GetComponentInChildren<TMP_Text>(true);
        label.text = label.text.Trim();
        SetTextStyle(label, 31f, White, TextAlignmentOptions.MidlineLeft);
        Dock(label.rectTransform, 0f, 0f, 1f, 1f);
        label.rectTransform.offsetMin = new Vector2(87f, 0f);
        label.rectTransform.offsetMax = new Vector2(-15f, 0f);

        Image icon = rect.GetComponentsInChildren<Image>(true).FirstOrDefault(i => i != button.targetGraphic && i.name.EndsWith("Icon", StringComparison.Ordinal));
        if (icon != null)
        {
            icon.color = Color.white;
            icon.raycastTarget = false;
            icon.preserveAspect = true;
            Anchor(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(28f, 0f), new Vector2(30f, 30f));
        }
    }

    private static void StyleButton(Button button, Sprite sprite)
    {
        Image image = (Image)button.targetGraphic;
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.raycastTarget = true;
        image.color = Color.white;
        DisableOutline(button.gameObject);
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color32(213, 255, 255, 255);
        colors.pressedColor = new Color32(130, 197, 221, 255);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        RobotRunnerEdition.GetOrAdd<ButtonFeedback>(button.gameObject);
    }

    private static Image Background(UIWindow window) => window.CanvasGroup.transform.Find("Background").GetComponent<Image>();

    private static TMP_Text Text(Transform host, string name, float size, Color color,
        TextAlignmentOptions alignment, float left, float bottom, float right, float top)
    {
        var rect = Named(host, name);
        Dock(rect, left, bottom, right, top);
        TMP_Text text = rect.GetComponent<TMP_Text>();
        SetTextStyle(text, size, color, alignment);
        return text;
    }

    private static void SetTextStyle(TMP_Text text, float size, Color color, TextAlignmentOptions alignment)
    {
        text.raycastTarget = false;
        text.color = color;
        text.fontStyle = FontStyles.Bold;
        text.fontSize = size;
        text.enableAutoSizing = true;
        text.fontSizeMin = 15f;
        text.fontSizeMax = size;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.characterSpacing = 2.3f;
        text.margin = Vector4.zero;
    }

    private static void DisableOutline(GameObject target)
    {
        foreach (Outline outline in target.GetComponents<Outline>())
            outline.enabled = false;
    }

    private static Image ImageAt(RectTransform rect, Sprite sprite, Color tint)
    {
        var image = RobotRunnerEdition.GetOrAdd<Image>(rect.gameObject);
        image.sprite = sprite;
        image.color = tint;
        image.type = sprite == null ? Image.Type.Simple : Image.Type.Sliced;
        image.raycastTarget = false;
        return image;
    }

    private static void Anchor(RectTransform rect, Vector2 point, Vector2 offset, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = point;
        rect.pivot = new Vector2(point.x <= 0.01f ? 0f : point.x >= 0.99f ? 1f : 0.5f, point.y);
        rect.anchoredPosition = offset;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }

    private static void Dock(RectTransform rect, float left, float bottom, float right, float top)
    {
        rect.anchorMin = new Vector2(left, bottom);
        rect.anchorMax = new Vector2(right, top);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static RectTransform Named(Transform parent, string name) =>
        parent.GetComponentsInChildren<RectTransform>(true).Single(rect => rect.name == name);

    private static RectTransform Child(Transform parent, string name)
    {
        var rect = parent.Find(name) as RectTransform;
        if (rect != null)
            return rect;
        rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void SetObject(Object component, string property, Object target)
    {
        var obj = new SerializedObject(component);
        obj.FindProperty(property).objectReferenceValue = target;
        obj.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Sprite MakeFrame(string name, Color fill, Color edge, Color light)
    {
        var pixels = new FactoryArtImporter.Pixels(40, 40);
        pixels.Fill(0, 4, 40, 32, edge);
        pixels.Fill(4, 0, 32, 40, edge);
        pixels.Fill(2, 6, 36, 28, fill);
        pixels.Fill(6, 2, 28, 36, fill);
        pixels.Fill(3, 4, 3, 3, edge);
        pixels.Fill(34, 33, 3, 3, edge);
        pixels.Fill(5, 0, 9, 2, light);
        pixels.Fill(0, 5, 2, 8, light);
        pixels.Fill(26, 38, 9, 2, light, 0.7f);
        pixels.Fill(38, 26, 2, 9, light, 0.7f);
        string path = $"{Root}/{name}.png";
        pixels.Write(path);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        return FactoryArtImporter.ImportComposition(path, new Vector2(0.5f, 0.5f), 60, new Vector4(14, 14, 14, 14));
    }

    private static Sprite MakeHero()
    {
        const int width = 176;
        const int height = 160;
        var pixels = new FactoryArtImporter.Pixels(width, height);
        Color dark = new Color32(8, 17, 35, 211);
        Color edge = new Color32(48, 120, 147, 226);
        pixels.Frame(0, 0, width, height, 1, edge, dark);
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                float radius = Vector2.Distance(new Vector2(x, y), new Vector2(92, 82));
                float band = 1f - Mathf.Clamp01(radius / 88f);
                if (band > 0f)
                    pixels.Fill(x, y, 1, 1, Cyan, band * band * 0.28f);
                if (Mathf.Abs(radius - 51f) < 0.6f || Mathf.Abs(radius - 60f) < 0.55f)
                    pixels.Fill(x, y, 1, 1, Cyan, y % 6 < 4 ? 0.26f : 0.09f);
            }
        }
        for (int y = 14; y < height - 5; y += 12)
            pixels.Fill(5, y, 3, 1, edge, 0.75f);
        for (int x = 15; x < 166; x += 11)
            pixels.Fill(x, 149, 2, 2, edge);
        pixels.Fill(1, 1, 23, 1, Cyan, 0.7f);
        pixels.Fill(1, 2, 1, 17, Cyan, 0.7f);
        pixels.Fill(width - 24, height - 2, 23, 1, Cyan, 0.7f);
        string path = $"{Root}/RobotDisplay.png";
        pixels.Write(path);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        return FactoryArtImporter.ImportComposition(path, new Vector2(0.5f, 0.5f), 100);
    }

    private static Sprite MakeKnob()
    {
        var pixels = new FactoryArtImporter.Pixels(16, 16);
        pixels.Fill(2, 0, 12, 16, Color.white);
        pixels.Fill(0, 2, 16, 12, Color.white);
        pixels.Fill(3, 3, 10, 10, new Color32(217, 240, 248, 255));
        string path = $"{Root}/ControlHandle.png";
        pixels.Write(path);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        return FactoryArtImporter.ImportComposition(path, new Vector2(0.5f, 0.5f), 100);
    }
}
