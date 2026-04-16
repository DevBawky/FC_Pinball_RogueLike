using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Bumper : MonoBehaviour
{
    [Header("Bumper Settings")]
    public float speedBoostMultiplier = 1.1f; 
    public float maxBallSpeed = 30f;          

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
        // 이제 BallMovement 대신 모든 정보를 쥐고 있는 BallController를 가져옵니다.
        BallController ballCtrl = collision.gameObject.GetComponent<BallController>();
        
        if (ballCtrl != null)
        {
            // 1. 타격감 (시각적 피드백)
            transform.localScale = originalScale * scalePunchAmount;

            // 2. 로그라이크 기믹: 공의 속도 증가
            BallMovement movement = collision.gameObject.GetComponent<BallMovement>();
            if (movement != null)
            {
                IncreaseBallSpeed(movement);
            }

            // 3. 점수 파티클 날리기 (기존 SpawnFlyingDamage 대체)
            // 공이 가진 속성(칩 or 배수)에 맞춰 해당 UI 패널로 파티클이 날아갑니다.
            ballCtrl.OnHitObject(transform.position);

            // 4. 외부 이벤트 호출 (사운드 재생 등)
            onBumperHit.Invoke();
        }
    }

    private void IncreaseBallSpeed(BallMovement ball)
    {
        if (ball.speed < maxBallSpeed)
        {
            ball.speed *= speedBoostMultiplier;
            ball.speed = Mathf.Min(ball.speed, maxBallSpeed);

            Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
            if (ballRb != null)
            {
                ballRb.linearVelocity = ballRb.linearVelocity.normalized * ball.speed;
            }
        }
    }
}