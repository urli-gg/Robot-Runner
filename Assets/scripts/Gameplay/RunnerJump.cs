using UnityEngine;

public readonly struct RunnerJump
{
    private readonly float launchSpeed;
    private readonly float holdAcceleration;
    private readonly float maxHold;
    private readonly float gravity;

    public float Step { get; }
    public float Width { get; }
    public float Height { get; }

    public RunnerJump(float launchSpeed, float holdAcceleration, float maxHold, float gravity,
        float step, float width, float height)
    {
        this.launchSpeed = launchSpeed;
        this.holdAcceleration = holdAcceleration;
        this.maxHold = maxHold;
        this.gravity = gravity;
        Step = step;
        Width = width;
        Height = height;
    }

    public float LongFlight => TimeAbove(0f, true) + Step;

    public float TimeAbove(float clearance, bool hold)
    {
        Vector2 window = ClearanceWindow(clearance, hold);
        return window.y - window.x;
    }

    public Vector2 ClearanceWindow(float clearance, bool hold)
    {
        float velocity = launchSpeed;
        float y = 0f;
        float held = 0f;
        float start = 0f;
        float end = 0f;
        for (int i = 0; i < Mathf.CeilToInt(5f / Step); i++)
        {
            if (hold && held < maxHold && velocity > 0f)
            {
                velocity += holdAcceleration * Step;
                held += Step;
            }
            velocity -= gravity * Step;
            y += velocity * Step;
            if (y <= 0f)
                break;
            if (y >= clearance)
            {
                end = (i + 1) * Step;
                if (start == 0f)
                    start = end;
            }
        }
        return new Vector2(start, end);
    }
}
