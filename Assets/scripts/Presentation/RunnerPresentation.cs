using Dino.Utility.Audio;
using UnityEngine;

[RequireComponent(typeof(RunnerController))]
public sealed class RunnerPresentation : MonoBehaviour
{
    [SerializeField] private FactoryArt art;
    [SerializeField] private SpriteRenderer robot;
    [SerializeField] private Material spriteMaterial;

    private RunnerController runner;
    private GameManager game;
    private SpriteRenderer dust;
    private SpriteRenderer impact;
    private float motion;
    private float landing;
    private float hitTime;
    private float protection;
    private float deathTime = -1f;
    private float dustAge = float.PositiveInfinity;
    private Vector3 dustPosition;

    private void Start()
    {
        runner = GetComponent<RunnerController>();
        game = GameManager.Instance;
        runner.Jumped += OnJump;
        runner.Landed += OnLanding;
        game.Damaged += OnDamage;
        dust = Effect("Landing sparks", 2);
        impact = Effect("Contact burst", 4);
    }

    private void OnDestroy()
    {
        if (runner != null)
        {
            runner.Jumped -= OnJump;
            runner.Landed -= OnLanding;
        }
        if (game != null)
            game.Damaged -= OnDamage;
        if (dust != null)
            Destroy(dust.gameObject);
        if (impact != null)
            Destroy(impact.gameObject);
    }

    private SpriteRenderer Effect(string label, int order)
    {
        var renderer = new GameObject(label).AddComponent<SpriteRenderer>();
        renderer.sharedMaterial = spriteMaterial;
        renderer.sortingOrder = order;
        renderer.enabled = false;
        return renderer;
    }

    private void LateUpdate()
    {
        if (!game.IsPlaying && game.HasStarted && !game.IsGameOver)
            return;

        robot.enabled = game.HasStarted;
        float delta = game.IsPlaying ? Time.deltaTime : Time.unscaledDeltaTime;
        motion += delta;
        landing = Mathf.Max(0f, landing - delta);
        protection = Mathf.Max(0f, protection - delta);
        hitTime = Mathf.Max(0f, hitTime - delta);
        dustAge += delta;
        if (deathTime >= 0f)
            deathTime += delta;

        Sprite frame;
        float pitch = 0f;
        float y = 0f;
        if (deathTime >= 0f)
            frame = art.dead[Mathf.Min(art.dead.Length - 1, (int)(deathTime * 10f))];
        else if (hitTime > 0f)
            frame = art.hurt[Mathf.Clamp((int)((0.27f - hitTime) * 15f), 0, art.hurt.Length - 1)];
        else if (!game.HasStarted)
            frame = Loop(art.idle, motion, 5f);
        else if (!runner.Grounded)
        {
            bool rising = runner.VerticalSpeed > 0f;
            frame = art.run[rising ? 1 : 2];
            pitch = rising ? -7f : 6f;
        }
        else
        {
            frame = Loop(art.run, motion, 13f);
            y = landing > 0f ? -0.01f : 0f;
        }

        robot.sprite = frame;
        robot.transform.localRotation = Quaternion.Euler(0f, 0f, pitch);
        robot.transform.localPosition = new Vector3(0f, y, 0f);
        float blink = protection > 0f && Mathf.FloorToInt(protection * 12f) % 2 == 0 ? 0.62f : 1f;
        robot.color = hitTime > 0.14f ? new Color(1f, 0.63f, 0.73f, blink) : new Color(1f, 1f, 1f, blink);

        dust.enabled = dustAge < 0.22f;
        if (dust.enabled)
        {
            dust.transform.position = dustPosition + Vector3.left * (dustAge * 2.2f);
            dust.sprite = art.spark[Mathf.Min(art.spark.Length - 1, (int)(dustAge * 15f))];
            dust.color = new Color(0.65f, 0.92f, 1f, 0.95f - dustAge * 3f);
        }

        float burstAge = deathTime >= 0f ? deathTime : 0.27f - hitTime;
        impact.enabled = burstAge < 0.27f;
        if (impact.enabled)
        {
            impact.sprite = art.impact[Mathf.Min(art.impact.Length - 1, (int)(burstAge * 26f))];
            impact.color = new Color(1f, 0.72f, 0.90f, 0.75f);
        }
    }

    private void OnJump() => AudioManager.Instance?.PlaySound("runnerJump");

    private void OnLanding(float speed)
    {
        landing = 0.09f;
        if (speed < 3f)
            return;
        dustAge = 0f;
        dustPosition = transform.position + new Vector3(-0.28f, -0.43f, 0f);
        AudioManager.Instance?.PlaySound("runnerLand");
    }

    private void OnDamage(bool fatal, float invulnerability)
    {
        impact.transform.position = transform.position + Vector3.right * 0.25f;
        hitTime = 0.27f;
        protection = fatal ? 0f : invulnerability;
        if (fatal)
            deathTime = 0f;
        AudioManager.Instance?.PlaySound(fatal ? "runnerShutdown" : "runnerHit");
    }

    private static Sprite Loop(Sprite[] frames, float time, float fps) =>
        frames[(int)(time * fps) % frames.Length];
}
