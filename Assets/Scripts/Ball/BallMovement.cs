using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class BallMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 15f;
    public Vector2 direction = new Vector2(1, 1);
    [SerializeField, Min(0.01f)] private float minimumVelocityThreshold = 0.25f;

    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private Vector2 lastVelocity;

    private void Awake()
    {
        EnsureComponents();
        ConfigurePhysics();
    }

    private void Start()
    {
        ApplyVelocity(GetSafeDirection(direction));
    }

    private void FixedUpdate()
    {
        MaintainConstantVelocity();
        lastVelocity = rb.linearVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 surfaceNormal = collision.contactCount > 0
            ? collision.GetContact(0).normal
            : -GetSafeDirection(lastVelocity);

        Vector2 incomingDirection = lastVelocity.sqrMagnitude > minimumVelocityThreshold * minimumVelocityThreshold
            ? lastVelocity.normalized
            : GetSafeDirection(direction);

        ApplyVelocity(Vector2.Reflect(incomingDirection, surfaceNormal));
    }

    public Vector2 GetCurrentDirection()
    {
        EnsureComponents();

        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.0001f)
        {
            return rb.linearVelocity.normalized;
        }

        return GetSafeDirection(direction);
    }

    public void SetDirection(Vector2 newDirection)
    {
        EnsureComponents();
        ApplyVelocity(newDirection);
    }

    private void ConfigurePhysics()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;

        PhysicsMaterial2D perfectMaterial = new PhysicsMaterial2D("PerfectBounce");
        perfectMaterial.friction = 0f;
        perfectMaterial.bounciness = 1f;
        circleCollider.sharedMaterial = perfectMaterial;
    }

    private void MaintainConstantVelocity()
    {
        EnsureComponents();

        if (speed <= 0f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 currentVelocity = rb.linearVelocity;
        if (currentVelocity.sqrMagnitude <= minimumVelocityThreshold * minimumVelocityThreshold)
        {
            ApplyVelocity(GetSafeDirection(direction));
            return;
        }

        if (!Mathf.Approximately(currentVelocity.magnitude, speed))
        {
            ApplyVelocity(currentVelocity.normalized);
        }
    }

    private void ApplyVelocity(Vector2 newDirection)
    {
        direction = GetSafeDirection(newDirection);

        if (rb != null)
        {
            rb.linearVelocity = direction * Mathf.Max(speed, 0f);
        }
    }

    private Vector2 GetSafeDirection(Vector2 candidate)
    {
        return candidate.sqrMagnitude > 0.0001f ? candidate.normalized : Vector2.up;
    }

    private void EnsureComponents()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (circleCollider == null)
        {
            circleCollider = GetComponent<CircleCollider2D>();
        }
    }
}
