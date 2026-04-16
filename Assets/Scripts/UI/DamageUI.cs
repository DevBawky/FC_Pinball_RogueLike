using UnityEngine;
using System.Collections;

public class DamageUI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 1500f;         // 날아가는 직진 속도
    public float arrivalDistance = 10f; // 도착으로 판정할 거리

    [Header("ZigZag (Sine Wave) Settings")]
    public float waveFrequency = 15f;   // 지그재그 꺾이는 속도 (높을수록 빨리 떰)
    public float waveAmplitude = 100f;  // 지그재그 좌우 폭 (픽셀 단위)
    
    // 랜덤으로 위아래/좌우 방향을 결정하기 위한 오프셋
    private float randomTimeOffset;

    private Transform targetTransform;
    private HealthBarUI targetHealthBar;
    private float damageAmount;

    public void Initialize(Transform target, HealthBarUI healthBar, float damage)
    {
        targetTransform = target;
        targetHealthBar = healthBar;
        damageAmount = damage;

        // 생성될 때마다 Sine 함수의 시작 지점을 랜덤하게 주어, 여러 개가 날아갈 때 각기 다른 궤적을 그리게 합니다.
        randomTimeOffset = Random.Range(0f, Mathf.PI * 2f);

        StartCoroutine(FlyToTarget());
    }

    private IEnumerator FlyToTarget()
    {
        // 1. 가상의 직진 중심점 (실제 UI의 position은 이 중심점을 기준으로 흔들림)
        Vector3 currentBasePosition = transform.position;

        // 2. 목적지에 가까워질수록 흔들림을 줄이기 위해 처음 거리를 기록
        float initialDistance = Vector3.Distance(currentBasePosition, targetTransform.position);

        while (targetTransform != null && Vector3.Distance(currentBasePosition, targetTransform.position) > arrivalDistance)
        {
            // --- [1단계] 목적지를 향해 똑바로 전진하는 가상의 중심점 이동 ---
            currentBasePosition = Vector3.MoveTowards(currentBasePosition, targetTransform.position, speed * Time.deltaTime);

            // --- [2단계] 현재 이동 방향을 구하고, 그 방향의 수직(직각) 벡터 구하기 ---
            Vector3 direction = (targetTransform.position - currentBasePosition).normalized;
            // 2D 공간에서 직각(Perpendicular) 벡터를 구하는 공식 (-y, x)
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f).normalized;

            // --- [3단계] 목적지에 가까워질수록 지그재그 폭(진폭)을 줄여주는 비율 계산 ---
            float currentDistance = Vector3.Distance(currentBasePosition, targetTransform.position);
            // 1.0(시작) -> 0.0(도착)으로 서서히 줄어듦
            float distanceRatio = currentDistance / initialDistance; 

            // --- [4단계] Sine 함수를 이용한 지그재그 오프셋 계산 ---
            // Time.time에 주파수를 곱해 파동을 만들고, 점차 줄어드는 진폭을 곱함
            float waveOffset = Mathf.Sin((Time.time + randomTimeOffset) * waveFrequency) * (waveAmplitude * distanceRatio);

            // --- [5단계] 실제 UI 위치 = 직진 중심점 + (수직 방향 * Sine 오프셋) ---
            transform.position = currentBasePosition + (perpendicular * waveOffset);

            yield return null;
        }

        // 목적지 도착 시 데미지 처리
        if (targetHealthBar != null)
        {
            targetHealthBar.TakeDamage(damageAmount);
        }

        Destroy(gameObject);
    }
}