using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private RunnerController runner;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private float minSpawnTime = 3.2f;
    [SerializeField] private float maxSpawnTime = 4.2f;
    [SerializeField] private float minimumSpawnTime = 1.45f;
    [SerializeField] private float secondsPerLevel = 25f;
    [SerializeField] private float timeReductionPerLevel = 0.65f;
    [SerializeField] private float baseObstacleSpeed = 5.6f;
    [SerializeField] private float speedIncreasePerLevel = 0.75f;

    // 0 = tap, 1 = hold, 2 = stay on the ground.
    private static readonly int[][][] Patterns =
    {
        new[] { new[] { 0 }, new[] { 1 }, new[] { 2 } },
        new[] { new[] { 0, 2 }, new[] { 1, 0 }, new[] { 2, 1 } },
        new[] { new[] { 2, 0, 1 }, new[] { 1, 2, 0 }, new[] { 0, 1, 2 } },
        new[] { new[] { 0, 2, 1, 0 }, new[] { 1, 0, 2, 1 }, new[] { 2, 1, 0, 2 } }
    };

    private readonly List<ObstacleMover> active = new(12);
    private Stack<ObstacleMover>[] pools;
    private ObstacleMover[] prefabs;
    private ObstacleEnvelope[] envelopes;
    private RunnerJump jump;
    private ObstacleMover previous;
    private float nextSpawnTime;
    private int[] pattern;
    private int patternPosition;
    private int lastKind = -1;
    private int lastPattern = -1;
    private int lastPatternLevel = -1;

    public float SurvivalTime { get; private set; }
    public float Speed => baseObstacleSpeed + SpeedProgress * speedIncreasePerLevel;
    public int Level => Mathf.Min(Patterns.Length - 1, (int)SpeedProgress);
    public int ActiveCount => active.Count;
    public int AllocatedCount { get; private set; }

    private float SpeedProgress => Mathf.Clamp(SurvivalTime / Mathf.Max(1f, secondsPerLevel), 0f, Patterns.Length - 1);
    private float MaxSpeed => baseObstacleSpeed + (Patterns.Length - 1) * speedIncreasePerLevel;

    private void Start()
    {
        if (runner == null || spawnPoint == null || worldCamera == null ||
            obstaclePrefabs == null || obstaclePrefabs.Length != 3)
        {
            Debug.LogError("ObstacleSpawner requires a runner, camera, spawn point and three obstacle prefabs.", this);
            enabled = false;
            return;
        }

        jump = runner.GetJumpProfile();
        prefabs = new ObstacleMover[3];
        envelopes = new ObstacleEnvelope[3];
        pools = new Stack<ObstacleMover>[3];
        foreach (GameObject prefabObject in obstaclePrefabs)
        {
            ObstacleMover prefab = prefabObject.GetComponent<ObstacleMover>();
            int kind = (int)prefab.Kind;
            prefabs[kind] = prefab;
            envelopes[kind] = prefab.Measure();
        }

        for (int i = 0; i < pools.Length; i++)
        {
            if (prefabs[i] == null || !Passable((ObstacleKind)i, envelopes[i], jump, baseObstacleSpeed))
            {
                Debug.LogError($"Obstacle kind {(ObstacleKind)i} is missing or fails its clearance check.", this);
                enabled = false;
                return;
            }
            pools[i] = new Stack<ObstacleMover>(3);
            for (int j = 0; j < 3; j++)
                pools[i].Push(Create(i));
        }

        nextSpawnTime = 0.85f;
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.IsPlaying)
            return;

        float delta = Time.fixedDeltaTime;
        SurvivalTime += delta;
        float halfWidth = worldCamera.orthographicSize * worldCamera.aspect;
        float cameraX = worldCamera.transform.position.x;
        float distance = Speed * delta;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            ObstacleMover obstacle = active[i];
            if (obstacle.X + envelopes[(int)obstacle.Kind].Right < cameraX - halfWidth - 0.8f)
            {
                if (previous == obstacle)
                    previous = null;
                obstacle.gameObject.SetActive(false);
                pools[(int)obstacle.Kind].Push(obstacle);
                active.RemoveAt(i);
            }
            else
            {
                obstacle.Advance(distance);
            }
        }

        if (SurvivalTime < nextSpawnTime)
            return;

        EnsurePattern();
        int kind = pattern[patternPosition];
        float spawnX = Mathf.Max(spawnPoint.position.x, cameraX + halfWidth + 1.25f);
        float gap = previous == null ? float.PositiveInfinity :
            spawnX + envelopes[kind].Left - (previous.X + envelopes[(int)previous.Kind].Right);
        if (gap < RequiredGap(jump, MaxSpeed, Level) + distance)
            return;

        ObstacleMover mover = pools[kind].Count > 0 ? pools[kind].Pop() : Create(kind);
        mover.Launch(new Vector2(spawnX, spawnPoint.position.y));
        active.Add(mover);
        previous = mover;
        lastKind = kind;

        patternPosition++;
        float reduction = Level * timeReductionPerLevel;
        float min = Mathf.Max(minimumSpawnTime, minSpawnTime - reduction);
        float max = Mathf.Max(min + 0.45f, maxSpawnTime - reduction);
        float recovery = patternPosition >= pattern.Length && Level > 0 ? 0.55f : 0f;
        nextSpawnTime = SurvivalTime + UnityEngine.Random.Range(min, max) + recovery;
    }

    private void EnsurePattern()
    {
        if (pattern != null && patternPosition < pattern.Length)
            return;

        int[][] options = Patterns[Level];
        int first = UnityEngine.Random.Range(0, options.Length);
        for (int i = 0; i < options.Length; i++)
        {
            int candidate = (first + i) % options.Length;
            if ((Level == lastPatternLevel && candidate == lastPattern) || options[candidate][0] == lastKind)
                continue;
            lastPattern = candidate;
            break;
        }
        lastPatternLevel = Level;
        pattern = options[lastPattern];
        patternPosition = 0;
    }

    private ObstacleMover Create(int kind)
    {
        ObstacleMover obstacle = Instantiate(prefabs[kind], transform);
        obstacle.gameObject.SetActive(false);
        AllocatedCount++;
        return obstacle;
    }

    public void ValidateCourse()
    {
        RunnerJump profile = runner.GetJumpProfile();
        foreach (GameObject prefab in obstaclePrefabs)
        {
            ObstacleMover mover = prefab.GetComponent<ObstacleMover>();
            ObstacleEnvelope bounds = mover.Measure();
            for (int step = 0; step <= 60; step++)
            {
                float speed = Mathf.Lerp(baseObstacleSpeed, MaxSpeed, step / 60f);
                if (!Passable(mover.Kind, bounds, profile, speed))
                    throw new InvalidOperationException($"{mover.name} fails clearance at {speed:F2} units/s.");
            }
        }

        for (int level = 0; level < Patterns.Length; level++)
        {
            foreach (int[] pattern in Patterns[level])
                foreach (int kind in pattern)
                    if (kind is < 0 or > 2)
                        throw new InvalidOperationException("Pattern references an unknown obstacle.");

            float recoveryTime = (RequiredGap(profile, MaxSpeed, level) - profile.Width) / MaxSpeed;
            if (recoveryTime < profile.LongFlight + 0.2f)
                throw new InvalidOperationException($"Level {level + 1} leaves insufficient time to land.");
        }
    }

    private static bool Passable(ObstacleKind kind, ObstacleEnvelope obstacle, RunnerJump profile, float speed)
    {
        const float clearance = 0.045f;
        if (kind == ObstacleKind.Drone)
            return obstacle.Bottom >= profile.Height + clearance;

        float requiredTime = (obstacle.Right - obstacle.Left + profile.Width) / speed + profile.Step;
        return profile.TimeAbove(obstacle.Top + clearance, kind == ObstacleKind.Boxes) >= requiredTime;
    }

    private static float RequiredGap(RunnerJump profile, float maxSpeed, int level)
    {
        float recovery = Mathf.Lerp(0.5f, 0.24f, level / (Patterns.Length - 1f));
        return profile.Width + (profile.LongFlight + recovery) * maxSpeed;
    }
}
