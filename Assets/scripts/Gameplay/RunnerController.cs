
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RunnerController : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpSpeed = 8f;
    [SerializeField] private float holdForce = 18f;
    [SerializeField] private float maxHoldTime = 0.20f;

    [Header("Ground")]
    [SerializeField] private Collider2D groundCollider;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isHolding;
    private float holdTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnTapEvent += HandleTap;
        }
        else
        {
            Debug.LogError("InputManager no encontrado.");
        }
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnTapEvent -= HandleTap;
        }
    }

    private void HandleTap(bool isPressed)
    {
        isHolding = isPressed;

        if (!isPressed || !isGrounded)
            return;

        isGrounded = false;
        holdTimer = 0f;

        // Salto inicial.
        rb.linearVelocity = new Vector2( rb.linearVelocity.x,jumpSpeed
        );
    }

    private void FixedUpdate()
    {
        // Mantener pulsado aumenta un poco la altura.
        if (isHolding && holdTimer < maxHoldTime && rb.linearVelocity.y > 0f)
        {
            rb.AddForce(Vector2.up * holdForce, ForceMode2D.Force);

            holdTimer += Time.fixedDeltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckGround(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckGround(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (groundCollider != null && collision.gameObject == groundCollider.gameObject)
        {
            isGrounded = false;
        }
    }

    private void CheckGround(Collision2D collision)
    {
        if (groundCollider == null || collision.gameObject != groundCollider.gameObject || rb.linearVelocity.y > 0.05f)
            return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }
}

