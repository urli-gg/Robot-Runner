using UnityEngine;

public enum ObstacleKind { Saw, Boxes, Drone, Patrol, Barrier, Press, Roller }

public readonly struct ObstacleEnvelope
{
    public readonly float Left;
    public readonly float Right;
    public readonly float Bottom;
    public readonly float Top;

    public ObstacleEnvelope(float left, float right, float bottom, float top)
    {
        Left = left;
        Right = right;
        Bottom = bottom;
        Top = top;
    }
}

[RequireComponent(typeof(Rigidbody2D))]
public sealed class ObstacleMover : MonoBehaviour
{
    [SerializeField] private ObstacleKind kind;
    [SerializeField, Range(0, 3)] private int firstLevel;
    [SerializeField] private bool holdJump;
    [SerializeField] private Vector4 stageWeights = Vector4.one;

    private Rigidbody2D body;
    private ObstacleEnvelope? measured;
    private bool contacted;
    private ObstacleAnimation animation;

    public ObstacleKind Kind => kind;
    public int FirstLevel => firstLevel;
    public bool RequiresHold => holdJump || kind == ObstacleKind.Boxes;
    public int BiomeIndex { get; private set; }
    public bool HasLaunched { get; private set; }
    public float WeightAt(int level) => level >= firstLevel ? Mathf.Max(0f, stageWeights[level]) : 0f;
    public float X => body.position.x;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animation = GetComponent<ObstacleAnimation>();
        Measure();
    }

    public void Launch(Vector2 position, int biome = 0)
    {
        contacted = false;
        HasLaunched = true;
        transform.position = new Vector3(position.x, position.y, 0f);
        BiomeIndex = biome;
        animation?.SetBiome(biome);
        gameObject.SetActive(true);
        body.position = position;
    }

    public void Advance(float distance)
    {
        body.MovePosition(body.position + Vector2.left * distance);
    }

    public void Hit()
    {
        if (contacted || !GameManager.Instance.IsPlaying)
            return;

        contacted = true;
        GameManager.Instance.TakeDamage();
    }

    public ObstacleEnvelope Measure()
    {
        if (Application.isPlaying && measured.HasValue)
            return measured.Value;

        float left = float.PositiveInfinity;
        float right = float.NegativeInfinity;
        float bottom = float.PositiveInfinity;
        float top = float.NegativeInfinity;
        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>(true))
        {
            if (!collider.enabled || !collider.isTrigger)
                continue;
            Vector3 center = collider.transform.TransformPoint(collider.offset) - transform.position;
            Vector2 size = collider switch
            {
                BoxCollider2D box => box.size,
                CircleCollider2D circle => Vector2.one * (circle.radius * 2f),
                CapsuleCollider2D capsule => capsule.size,
                _ => throw new System.InvalidOperationException($"Unsupported obstacle collider on {name}.")
            };
            Vector3 scale = collider.transform.lossyScale;
            float halfWidth = Mathf.Abs(scale.x) * size.x * 0.5f;
            float halfHeight = Mathf.Abs(scale.y) * size.y * 0.5f;
            left = Mathf.Min(left, center.x - halfWidth);
            right = Mathf.Max(right, center.x + halfWidth);
            bottom = Mathf.Min(bottom, center.y - halfHeight);
            top = Mathf.Max(top, center.y + halfHeight);
        }
        var bounds = new ObstacleEnvelope(left, right, bottom, top);
        if (Application.isPlaying)
            measured = bounds;
        return bounds;
    }
}
