using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class BallMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float speed = 15f; // 영구적으로 유지할 등속도
    public Vector2 direction = new Vector2(1, 1); // 초기 시작 방향

    private Rigidbody2D rb;
    private Vector2 lastVelocity; // 충돌 직전의 속도를 기억할 변수

    void Start()
    {
        EnsureRigidbody();

        // 1. 완벽한 등속 운동과 터널링 방지를 위한 Rigidbody 강제 세팅
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f; // 탑뷰이므로 중력 제거
        rb.linearDamping = 0f; // 공기 저항 제거
        rb.angularDamping = 0f; // 회전 저항 제거
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 얇은 벽 뚫기 방지 (매우 중요)
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // 프레임 간 이동을 부드럽게 보정

        // 2. 마찰 0, 반발력 1을 가진 물리 재질(Physics Material)을 코드로 생성하여 적용
        PhysicsMaterial2D perfectMaterial = new PhysicsMaterial2D("PerfectBounce");
        perfectMaterial.friction = 0f;
        perfectMaterial.bounciness = 1f;
        GetComponent<CircleCollider2D>().sharedMaterial = perfectMaterial;

        // 3. 초기 속도 발사
        rb.linearVelocity = direction.normalized * speed;
    }

    void FixedUpdate()
    {
        // 매 물리 프레임마다 현재 속도를 기록
        // 유니티 물리 엔진이 충돌을 연산하여 속도를 깎아먹기 직전의 '순수한 입사 벡터'를 기억하기 위함입니다.
        lastVelocity = rb.linearVelocity;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. 충돌한 벽의 표면 수직선(법선, Normal) 방향을 가져옵니다.
        Vector2 surfaceNormal = collision.contacts[0].normal;

        // 2. 입사 벡터(마지막 속도)와 법선 벡터를 튕겨내어(Reflect) 완벽한 반사 방향을 구합니다.
        Vector2 reflectedDirection = Vector2.Reflect(lastVelocity.normalized, surfaceNormal).normalized;

        // 3. 물리 엔진이 계산한 불완전한 충돌 결과를 무시하고, 
        // 우리가 수학적으로 구한 완벽한 방향에 목표 속도를 강제로 덮어씌웁니다.
        rb.linearVelocity = reflectedDirection * speed;
    }

    public Vector2 GetCurrentDirection()
    {
        EnsureRigidbody();

        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.0001f)
        {
            return rb.linearVelocity.normalized;
        }

        return direction.normalized;
    }

    public void SetDirection(Vector2 newDirection)
    {
        EnsureRigidbody();

        direction = newDirection == Vector2.zero ? Vector2.up : newDirection.normalized;

        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }

    private void EnsureRigidbody()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }
}
