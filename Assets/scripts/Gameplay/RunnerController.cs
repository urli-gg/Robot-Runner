using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class RunnerController : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpSpeed = 8f;
    [SerializeField] private float holdForce = 18f;
    [SerializeField] private float maxHoldTime = 0.20f;
    [SerializeField] private float bufferWindow = 0.10f;

    [Header("Ground")]
    [SerializeField] private Collider2D groundCollider;

    private Rigidbody2D body;
    private InputManager input;
    private bool grounded;
    private bool holding;
    private bool extending;
    private bool jumped;
    private float holdTimer;
    private float queuedUntil = float.NegativeInfinity;
    private float fallSpeed;

    public bool Grounded => grounded;
    public float VerticalSpeed => body.linearVelocity.y;
    public event Action Jumped;
    public event Action<float> Landed;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        input = InputManager.Instance;
        if (input != null)
            input.OnTapEvent += HandleTap;
        else
            Debug.LogError("InputManager no encontrado.", this);
    }

    private void OnDestroy()
    {
        if (input != null)
            input.OnTapEvent -= HandleTap;
    }

    private void Update()
    {
        if (GameManager.Instance.IsPlaying)
            return;
        holding = false;
        extending = false;
        queuedUntil = float.NegativeInfinity;
    }

    private void HandleTap(bool pressed)
    {
        holding = pressed && GameManager.Instance.IsPlaying;
        if (holding)
            queuedUntil = Time.time + bufferWindow;
        else
            extending = false;
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.IsPlaying)
            return;

        if (grounded && Time.time <= queuedUntil)
        {
            queuedUntil = float.NegativeInfinity;
            grounded = false;
            jumped = true;
            extending = holding;
            holdTimer = 0f;
            fallSpeed = 0f;
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpSpeed);
            Jumped?.Invoke();
        }

        if (extending && holdTimer < maxHoldTime && body.linearVelocity.y > 0f)
        {
            body.AddForce(Vector2.up * holdForce, ForceMode2D.Force);
            holdTimer += Time.fixedDeltaTime;
        }

        if (!grounded)
            fallSpeed = Mathf.Max(fallSpeed, -body.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision) => CheckGround(collision);
    private void OnCollisionStay2D(Collision2D collision) => CheckGround(collision);

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider == groundCollider)
            grounded = false;
    }

    private void CheckGround(Collision2D collision)
    {
        if (grounded || collision.collider != groundCollider || body.linearVelocity.y > 0.05f)
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y <= 0.5f)
                continue;
            grounded = true;
            if (jumped)
                Landed?.Invoke(fallSpeed);
            jumped = false;
            return;
        }
    }

    public RunnerJump GetJumpProfile()
    {
        Rigidbody2D physicsBody = GetComponent<Rigidbody2D>();
        BoxCollider2D hitbox = GetComponent<BoxCollider2D>();
        Vector3 scale = transform.lossyScale;
        return new RunnerJump(jumpSpeed, holdForce / physicsBody.mass, maxHoldTime,
            -Physics2D.gravity.y * physicsBody.gravityScale, Time.fixedDeltaTime,
            hitbox.size.x * Mathf.Abs(scale.x), hitbox.size.y * Mathf.Abs(scale.y));
    }
}
