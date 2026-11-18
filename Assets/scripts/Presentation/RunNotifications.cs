using Dino.Utility.Audio;
using TMPro;
using UnityEngine;

public sealed class RunNotifications : MonoBehaviour
{
    [SerializeField] private FactoryBackdrop factory;
    [SerializeField] private CanvasGroup badge;
    [SerializeField] private TMP_Text message;
    private GameManager game;
    private float age = float.PositiveInfinity;
    private float duration;
    private bool intro;
    private bool showingRecord;

    private void Start()
    {
        game = GameManager.Instance;
        factory.BiomeChanged += OnBiomeChanged;
        game.NewRecord += Record;
        badge.alpha = 0f;
    }

    private void OnDestroy()
    {
        if (factory != null)
            factory.BiomeChanged -= OnBiomeChanged;
        if (game != null)
            game.NewRecord -= Record;
    }

    private void Update()
    {
        if (!game.IsPlaying)
        {
            badge.alpha = 0f;
            return;
        }
        if (!intro)
        {
            intro = true;
            Show("TAP TO JUMP  ·  HOLD TO JUMP HIGHER", new Color32(202, 240, 255, 255), 4.4f);
        }
        age += Time.deltaTime;
        badge.alpha = Mathf.Clamp01(Mathf.Min(age * 8f, (duration - age) * 3f));
        if (age >= duration)
            showingRecord = false;
    }

    private void OnBiomeChanged(FactoryArt.Biome biome)
    {
        if (showingRecord)
            return;
        Show(biome.name.ToUpperInvariant(), biome.accent, 2.6f);
        AudioManager.Instance?.PlaySound("sectorEntry");
    }

    private void Record()
    {
        showingRecord = true;
        Show("NEW RECORD", new Color32(255, 211, 103, 255), 2.8f);
    }

    private void Show(string text, Color color, float seconds)
    {
        message.text = text;
        message.color = color;
        age = 0f;
        duration = seconds;
    }
}
