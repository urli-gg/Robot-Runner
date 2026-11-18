using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ObstacleSpawner : MonoBehaviour
{
    [Serializable]
    private sealed class Stage
    {
        public float interval;
        public float jitter;
        public float recovery;
        [Range(0f, 1f)] public float patternChance;

        public Stage(float interval, float jitter, float recovery, float patternChance)
        {
            this.interval = interval;
            this.jitter = jitter;
            this.recovery = recovery;
            this.patternChance = patternChance;
        }
    }

    [Serializable]
    private sealed class Pattern
    {
        [Range(0, 3)] public int firstLevel;
        public ObstacleKind[] kinds;

        public Pattern(int firstLevel, params ObstacleKind[] kinds)
        {
            this.firstLevel = firstLevel;
            this.kinds = kinds;
        }
    }

    private sealed class Hazard
    {
        public ObstacleMover prefab;
        public ObstacleEnvelope bounds;
        public Vector2 window;
        public float flight;
        public readonly Stack<ObstacleMover> pool = new(2);
        public float Lead => (window.x + window.y) * 0.5f;
    }

    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private RunnerController runner;
    [SerializeField] private Camera worldCamera;
    [SerializeField, Min(1f)] private float secondsPerLevel = 18f;
    [SerializeField, Min(1f)] private float baseObstacleSpeed = 5.6f;
    [SerializeField, Min(1f)] private float maximumSpeed = 9.4f;
    [SerializeField, Min(0f)] private float speedIncreasePerLevel = 1.27f;
    [SerializeField, Min(0.4f)] private float reactionTime = 0.60f;
    [SerializeField, Min(0f)] private float safetyDistance = 0.12f;
    [SerializeField] private Stage[] stages =
    {
        new(2.25f, 0.55f, 0.42f, 0f),
        new(1.40f, 0.32f, 0.28f, 0.54f),
        new(0.98f, 0.22f, 0.22f, 0.68f),
        new(0.72f, 0.14f, 0.16f, 0.80f)
    };
    [SerializeField] private Pattern[] patterns =
    {
        new(1, ObstacleKind.Saw, ObstacleKind.Patrol),
        new(1, ObstacleKind.Boxes, ObstacleKind.Drone),
        new(1, ObstacleKind.Patrol, ObstacleKind.Barrier),
        new(2, ObstacleKind.Drone, ObstacleKind.Saw, ObstacleKind.Roller),
        new(2, ObstacleKind.Roller, ObstacleKind.Barrier, ObstacleKind.Press),
        new(3, ObstacleKind.Press, ObstacleKind.Boxes, ObstacleKind.Roller, ObstacleKind.Drone),
        new(3, ObstacleKind.Saw, ObstacleKind.Patrol, ObstacleKind.Press, ObstacleKind.Barrier)
    };

    private const float Clearance = 0.045f;
    private readonly List<ObstacleMover> active = new(12);
    private readonly Dictionary<ObstacleKind, Hazard> hazards = new();
    private System.Random random;
    private FactoryBackdrop backdrop;
    private RunnerJump jump;
    private ObstacleMover previous;
    private float nextSpawnTime;
    private Pattern pattern;
    private int patternPosition;
    private bool selecting = true;
    private Hazard pending;

    public float SurvivalTime { get; private set; }
    public float Speed => Mathf.Min(maximumSpeed, baseObstacleSpeed + SpeedProgress * speedIncreasePerLevel);
    public int Level => (int)SpeedProgress;
    public int ActiveCount => active.Count;
    public int AllocatedCount { get; private set; }
    public int ReusedCount { get; private set; }
    public int CompletedPatterns { get; private set; }
    public int TypeCount => hazards.Count;
    public float MaxSpeed => Mathf.Min(maximumSpeed, baseObstacleSpeed + 3f * speedIncreasePerLevel);

    private float SpeedProgress => Mathf.Clamp(SurvivalTime / secondsPerLevel, 0f, 3f);

    private void Start()
    {
        try
        {
            ConfigureCourse();
        }
        catch (InvalidOperationException error)
        {
            Debug.LogError(error.Message, this);
            enabled = false;
            return;
        }

        random = new System.Random();
        backdrop = FindAnyObjectByType<FactoryBackdrop>();
        foreach (Hazard hazard in hazards.Values)
        {
            hazard.pool.Push(Create(hazard));
            hazard.pool.Push(Create(hazard));
        }
        nextSpawnTime = 0.85f;
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.IsPlaying)
            return;

        SurvivalTime += Time.fixedDeltaTime;
        float halfWidth = worldCamera.orthographicSize * worldCamera.aspect;
        float cameraX = worldCamera.transform.position.x;
        float distance = Speed * Time.fixedDeltaTime;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            ObstacleMover obstacle = active[i];
            Hazard hazard = hazards[obstacle.Kind];
            if (obstacle.X + hazard.bounds.Right < cameraX - halfWidth - 0.8f)
            {
                if (previous == obstacle)
                    previous = null;
                obstacle.gameObject.SetActive(false);
                hazard.pool.Push(obstacle);
                active.RemoveAt(i);
            }
            else
                obstacle.Advance(distance);
        }

        if (SurvivalTime < nextSpawnTime)
            return;

        Stage stage = stages[Level];
        if (selecting)
        {
            if (pattern == null)
            {
                pattern = SelectPattern(random, Level, previous != null ? previous.Kind : null);
                patternPosition = 0;
            }
            pending = pattern == null ? SelectSingle(random, Level, previous != null ? previous.Kind : null) :
                hazards[pattern.kinds[patternPosition]];
            selecting = false;
        }

        // Spawn beyond the viewport and reserve reaction time before the jump command.
        float spawnX = Mathf.Max(Mathf.Max(spawnPoint.position.x, cameraX + halfWidth + 0.25f - pending.bounds.Left),
            runner.transform.position.x + MaxSpeed * (reactionTime + pending.Lead) - Center(pending.bounds));
        if (previous != null)
        {
            Hazard prior = hazards[previous.Kind];
            float centerGap = spawnX + Center(pending.bounds) - (previous.X + Center(prior.bounds));
            float approachTime = (spawnX + Center(pending.bounds) - runner.transform.position.x) / Speed;
            float speedBound = Mathf.Min(MaxSpeed, Speed + approachTime * speedIncreasePerLevel / secondsPerLevel);
            if (centerGap < RequiredSeparation(prior, pending, stage, speedBound) + distance)
                return;
        }

        ObstacleMover mover;
        if (pending.pool.Count > 0)
        {
            mover = pending.pool.Pop();
            if (mover.HasLaunched)
                ReusedCount++;
        }
        else
            mover = Create(pending);

        mover.Launch(new Vector2(spawnX, spawnPoint.position.y), backdrop != null ? backdrop.BiomeIndex : 0);
        active.Add(mover);
        previous = mover;
        selecting = true;

        bool inPattern = pattern != null && ++patternPosition < pattern.kinds.Length;
        if (pattern != null && !inPattern)
            CompletedPatterns++;
        if (!inPattern)
            pattern = null;
        nextSpawnTime = SurvivalTime + stage.interval + stage.jitter * (float)random.NextDouble()
            + (inPattern ? 0f : stage.recovery);
    }

    private Pattern SelectPattern(System.Random source, int level, ObstacleKind? prior)
    {
        if (source.NextDouble() >= stages[level].patternChance)
            return null;

        Pattern choice = null;
        int count = 0;
        foreach (Pattern candidate in patterns)
        {
            if (candidate.firstLevel > level || candidate.kinds[0] == prior || !Available(candidate, level))
                continue;
            if (source.Next(++count) == 0)
                choice = candidate;
        }
        return choice;
    }

    private bool Available(Pattern candidate, int level)
    {
        foreach (ObstacleKind kind in candidate.kinds)
            if (!hazards.TryGetValue(kind, out Hazard hazard) || hazard.prefab.FirstLevel > level)
                return false;
        return true;
    }

    private Hazard SelectSingle(System.Random source, int level, ObstacleKind? prior)
    {
        Hazard choice = null;
        double total = 0;
        foreach (Hazard hazard in hazards.Values)
        {
            float weight = hazard.prefab.WeightAt(level);
            if (weight <= 0f)
                continue;
            if (hazard.prefab.Kind == prior)
                weight *= 0.2f;
            total += weight;
            if (source.NextDouble() * total < weight)
                choice = hazard;
        }
        return choice;
    }

    private ObstacleMover Create(Hazard hazard)
    {
        ObstacleMover obstacle = Instantiate(hazard.prefab, transform);
        obstacle.gameObject.SetActive(false);
        AllocatedCount++;
        return obstacle;
    }

    private void ConfigureCourse()
    {
        if (runner == null || spawnPoint == null || worldCamera == null || !worldCamera.orthographic ||
            obstaclePrefabs == null || obstaclePrefabs.Length == 0 || stages.Length != 4 ||
            baseObstacleSpeed <= 0f || maximumSpeed < baseObstacleSpeed || secondsPerLevel <= 0f ||
            speedIncreasePerLevel < 0f || reactionTime < 0.4f || safetyDistance < 0f)
            throw new InvalidOperationException("Obstacle course configuration is incomplete.");

        jump = runner.GetJumpProfile();
        hazards.Clear();
        foreach (GameObject root in obstaclePrefabs)
        {
            if (root == null || !root.TryGetComponent(out ObstacleMover prefab) || hazards.ContainsKey(prefab.Kind))
                throw new InvalidOperationException("Obstacle prefabs must each have a unique kind and a mover.");

            ObstacleEnvelope bounds = prefab.Measure();
            if (!(bounds.Right > bounds.Left && bounds.Top > bounds.Bottom))
                throw new InvalidOperationException($"{prefab.name} has no supported trigger geometry.");

            bool ground = bounds.Bottom < jump.Height + Clearance;
            bool hold = prefab.RequiresHold;
            var hazard = new Hazard
            {
                prefab = prefab,
                bounds = bounds,
                window = ground ? jump.ClearanceWindow(bounds.Top + Clearance, hold) : Vector2.zero,
                flight = ground ? jump.Flight(hold) : 0f
            };
            if (ground && hazard.window.y - hazard.window.x < 2f * ApproachRadius(hazard) / baseObstacleSpeed + 2f * jump.Step)
                throw new InvalidOperationException($"{prefab.name} has insufficient jump clearance at base speed.");
            hazards.Add(prefab.Kind, hazard);
        }

        for (int level = 0; level < stages.Length; level++)
        {
            Stage stage = stages[level];
            if (stage.interval <= 0f || stage.jitter < 0f || stage.recovery < 0.12f ||
                SelectSingle(new System.Random(0), level, null) == null)
                throw new InvalidOperationException($"Invalid obstacle settings for stage {level + 1}.");
        }

        foreach (Pattern item in patterns)
            if (item.kinds == null || item.kinds.Length < 2)
                throw new InvalidOperationException("An obstacle pattern needs at least two members.");
    }

    private float RequiredSeparation(Hazard previousHazard, Hazard next, Stage stage, float speedBound)
    {
        // Invariant: the runner finishes the previous action, then has recovery time
        // before starting the next action. A grounded action occupies the whole hitbox.
        float after = previousHazard.flight > 0f
            ? speedBound * (previousHazard.flight - previousHazard.Lead + jump.Step)
            : ApproachRadius(previousHazard);
        float before = next.flight > 0f ? speedBound * (next.Lead + jump.Step) : ApproachRadius(next);
        return Mathf.Max(ApproachRadius(previousHazard) + ApproachRadius(next),
            after + before + speedBound * stage.recovery) + safetyDistance;
    }

    private float ApproachRadius(Hazard hazard) => (hazard.bounds.Right - hazard.bounds.Left + jump.Width) * 0.5f;
    private static float Center(ObstacleEnvelope bounds) => (bounds.Left + bounds.Right) * 0.5f;

    public void ValidateCourse()
    {
        if (random == null)
            ConfigureCourse();
        // Aspect 4:3 is the narrowest supported landscape layout. The reaction
        // clock begins when the hazard enters view, not when it is instantiated.
        float viewRight = worldCamera.transform.position.x + worldCamera.orthographicSize * (4f / 3f);
        foreach (Hazard hazard in hazards.Values)
        {
            float sight = viewRight - runner.transform.position.x + Center(hazard.bounds) - hazard.bounds.Left;
            float visibleReaction = sight / MaxSpeed - Mathf.Max(hazard.Lead, ApproachRadius(hazard) / MaxSpeed);
            if (visibleReaction < reactionTime)
                throw new InvalidOperationException($"{hazard.prefab.name} gives {visibleReaction:F3} s of visible reaction at maximum speed (requires {reactionTime:F3} s).");

            for (int sample = 0; sample <= 60; sample++)
            {
                float speed = Mathf.Lerp(baseObstacleSpeed, MaxSpeed, sample / 60f);
                if (hazard.flight > 0f && hazard.window.y - hazard.window.x <
                    2f * ApproachRadius(hazard) / speed + 2f * jump.Step)
                    throw new InvalidOperationException($"{hazard.prefab.name} fails clearance at {speed:F2} units/s.");
            }
        }

        // Check all legal adjacencies, including joins between different patterns.
        // Returning to the ground between decisions makes this proof compositional.
        foreach (Stage stage in stages)
        foreach (Hazard prior in hazards.Values)
        foreach (Hazard next in hazards.Values)
        {
            float after = prior.flight > 0f ? prior.flight - prior.Lead : ApproachRadius(prior) / MaxSpeed;
            float before = next.flight > 0f ? next.Lead : ApproachRadius(next) / MaxSpeed;
            if (RequiredSeparation(prior, next, stage, MaxSpeed) / MaxSpeed - after - before < stage.recovery - 0.001f)
                throw new InvalidOperationException("Consecutive obstacles leave insufficient recovery.");
        }

        for (int seed = 0; seed < 128; seed++)
        {
            var source = new System.Random(seed);
            ObstacleKind? prior = null;
            for (int level = 0; level < stages.Length; level++)
            for (int sequence = 0; sequence < 64; sequence++)
            {
                Pattern selected = SelectPattern(source, level, prior);
                if (selected == null)
                    prior = SelectSingle(source, level, prior).prefab.Kind;
                else
                {
                    if (!Available(selected, level))
                        throw new InvalidOperationException("An unavailable course pattern was selected.");
                    prior = selected.kinds[selected.kinds.Length - 1];
                }
            }
        }
    }
}
