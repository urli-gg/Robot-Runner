using UnityEngine;

public enum ObstacleKind { Saw, Boxes, Drone }

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
    private Rigidbody2D body;
    private bool contacted;

    public ObstacleKind Kind => kind;
    public float X => body.position.x;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 position)
    {
        contacted = false;
        transform.position = new Vector3(position.x, position.y, 0f);
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
        float left = float.PositiveInfinity;
        float right = float.NegativeInfinity;
        float bottom = float.PositiveInfinity;
        float top = float.NegativeInfinity;
        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>(true))
        {
            if (!collider.enabled || !collider.isTrigger)
                continue;
            Vector3 center = collider.transform.TransformPoint(collider.offset) - transform.position;
            Vector2 size = collider is BoxCollider2D box ? box.size : Vector2.one * (((CircleCollider2D)collider).radius * 2f);
            Vector3 scale = collider.transform.lossyScale;
            float halfWidth = Mathf.Abs(scale.x) * size.x * 0.5f;
            float halfHeight = Mathf.Abs(scale.y) * size.y * 0.5f;
            left = Mathf.Min(left, center.x - halfWidth);
            right = Mathf.Max(right, center.x + halfWidth);
            bottom = Mathf.Min(bottom, center.y - halfHeight);
            top = Mathf.Max(top, center.y + halfHeight);
        }
        return new ObstacleEnvelope(left, right, bottom, top);
    }
}
