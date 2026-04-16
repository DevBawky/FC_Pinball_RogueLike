using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Bumper : MonoBehaviour
{
    [Header("Bumper Settings")]
    public float speedBoostMultiplier = 1.1f; 
    public float maxBallSpeed = 30f;          
    public int baseDamage = 10;               

    [Header("Visual & Feedback")]
    public float scalePunchAmount = 1.2f;     
    public float resetSpeed = 5f;             

    [Header("Events")]
    public UnityEvent onBumperHit;            

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        // 범퍼가 충돌 후 원래 크기로 부드럽게 복귀하도록 처리
        if (transform.localScale != originalScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * resetSpeed);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        BallMovement ball = collision.gameObject.GetComponent<BallMovement>();
        
        if (ball != null)
        {
            // 1. 타격감 (시각적 피드백)
            transform.localScale = originalScale * scalePunchAmount;

            // 2. 로그라이크 기믹: 공의 속도를 증가시키거나 추가 로직 적용
            IncreaseBallSpeed(ball);

            // 3. 외부 이벤트 호출 (사운드 재생, 점수 증가, 연계 효과 등)
            onBumperHit.Invoke();

            UIManager.Instance.SpawnFlyingDamage(transform.position, 10f);
        }
    }

    private void IncreaseBallSpeed(BallMovement ball)
    {
        if (ball.speed < maxBallSpeed)
        {
            ball.speed *= speedBoostMultiplier;
            ball.speed = Mathf.Min(ball.speed, maxBallSpeed);

            // 중요: BallMovement에서 이미 충돌 처리를 하며 방향을 바꿨을 수 있으므로,
            // 변경된 speed가 즉각 물리적으로 적용되도록 현재 방향을 유지한 채 속력만 갱신합니다.
            Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
            if (ballRb != null)
            {
                ballRb.linearVelocity = ballRb.linearVelocity.normalized * ball.speed;
            }
        }
    }
}