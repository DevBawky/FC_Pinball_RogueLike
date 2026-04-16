using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class RussianRouletteCylinder : MonoBehaviour
{
    [Header("Roulette Settings")]
    [Range(1, 6)]
    public int deadlyChambers = 1;
    public int totalChambers = 6;
    public float damageMultiplier = 2.0f;

    [Header("Visual Feedback")]
    public Transform cylinderVisual;
    public float visualPunchScale = 1.3f;
    private Vector3 originalScale;

    [Header("Events")]
    public UnityEvent onSuccess;
    public UnityEvent onFail;

    void Start()
    {
        if (cylinderVisual != null)
        {
            originalScale = cylinderVisual.localScale;
        }
        else
        {
            originalScale = transform.localScale;
        }
    }

    void Update()
    {
        Transform targetTransform = cylinderVisual != null ? cylinderVisual : transform;
        if (targetTransform.localScale != originalScale)
        {
            targetTransform.localScale = Vector3.Lerp(targetTransform.localScale, originalScale, Time.deltaTime * 10f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        BallMovement ball = collision.gameObject.GetComponent<BallMovement>();
        if (ball != null)
        {
            Transform targetTransform = cylinderVisual != null ? cylinderVisual : transform;
            targetTransform.Rotate(0, 0, Random.Range(120f, 360f));
            targetTransform.localScale = originalScale * visualPunchScale;

            int roll = Random.Range(0, totalChambers);

            if (roll < deadlyChambers)
            {
                TriggerFail(ball);
            }
            else
            {
                TriggerSuccess(ball);
            }
        }
    }

    private void TriggerSuccess(BallMovement ball)
    {
        // ball.damage *= damageMultiplier;
        
        Debug.Log($"[찰칵] 생존! 공의 대미지가 {damageMultiplier}배로 증폭되었습니다.");
        
        onSuccess.Invoke();
    }

    private void TriggerFail(BallMovement ball)
    {
        Destroy(ball.gameObject);
        
        Debug.Log("[탕!] 룰렛에 당첨되어 공이 파괴되었습니다.");
        
        onFail.Invoke();
    }
}