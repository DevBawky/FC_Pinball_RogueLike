using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageParticleManager : MonoBehaviour
{
    public static DamageParticleManager Instance;

    [Header("References")]
    public GameObject damageParticlePrefab; // 적에게 날아갈 파티클(이미지) 프리팹
    public Canvas mainCanvas;               // 파티클이 생성될 캔버스
    public RectTransform startTarget;       // 출발지 (Total Damage 텍스트 위치)
    public RectTransform endTarget;         // 도착지 (적의 체력바 또는 적 스프라이트 위치)

    [Header("Particle Settings")]
    public int maxParticles = 50;           // 최대 파티클 개수 (너무 많으면 렉 발생 방지)
    public float particleBaseDamage = 100f; // 파티클 1개당 기본 대미지
    public float particleSpeed = 2500f;     // 날아가는 속도
    public float spawnInterval = 0.05f;     // 파티클 생성 간격 (따다다닥!)

    private int activeParticles = 0;        // 현재 날아가고 있는 파티클 수
    private float remainderDamage = 0f;     // 파티클로 나누어 떨어지지 않은 남은 대미지

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // ScoreManager에서 최종 대미지가 정해지면 이 함수를 호출합니다.
    public void FireDamageParticles(float totalDamage)
    {
        StartCoroutine(FireRoutine(totalDamage));
    }

    private IEnumerator FireRoutine(float totalDamage)
    {
        int particleCount = Mathf.FloorToInt(totalDamage / particleBaseDamage);
        float actualDamagePerParticle = particleBaseDamage;

        if (particleCount > maxParticles)
        {
            particleCount = maxParticles;
            actualDamagePerParticle = totalDamage / maxParticles;
        }
        if (particleCount == 0 && totalDamage > 0)
        {
            particleCount = 1;
            actualDamagePerParticle = totalDamage;
        }

        remainderDamage = totalDamage - (particleCount * actualDamagePerParticle);
        activeParticles = particleCount;

        float currentDisplayDamage = totalDamage;

        for (int i = 0; i < particleCount; i++)
        {
            float damageToDeal = actualDamagePerParticle;
            if (i == particleCount - 1)
            {
                damageToDeal += remainderDamage;
            }

            // 1. 파티클 발사!
            SpawnParticle(damageToDeal);

            if (ScoreManager.Instance != null)
            {
                float nextDisplayDamage = currentDisplayDamage - damageToDeal;
                
                // 파티클 생성 간격(spawnInterval) 동안 서서히 숫자가 줄어들도록 지시
                ScoreManager.Instance.ReduceTotalDamageText(currentDisplayDamage, nextDisplayDamage, spawnInterval);
                
                currentDisplayDamage = nextDisplayDamage; // 현재 보여지는 값을 갱신
            }

            yield return new WaitForSeconds(spawnInterval); 
        }
    }

    private void SpawnParticle(float damageAmount)
    {
        GameObject pObj = Instantiate(damageParticlePrefab, mainCanvas.transform);
        pObj.transform.position = startTarget.position; // 출발지에서 스폰

        // 목표를 향해 날아가는 로직 (간단한 MoveTowards 사용, LeanTween 등을 쓰면 더 예쁩니다)
        StartCoroutine(MoveParticleRoutine(pObj, damageAmount));
    }

    private IEnumerator MoveParticleRoutine(GameObject pObj, float damageAmount)
    {
        // 화면을 뚫고 지나가는 느낌을 주려면 곡선(Bezier)이나 지그재그(이전의 FlyingScoreUI처럼) 
        // 움직임을 추가해도 좋지만, 여기서는 적에게 빠르게 직선/살짝 휘어서 꽂히는 느낌으로 구현합니다.

        Vector3 startPos = pObj.transform.position;
        Vector3 targetPos = endTarget.position;
        
        // 날아가는 동안 약간의 곡선(아치형)을 주기 위한 랜덤 오프셋
        Vector3 randomOffset = new Vector3(Random.Range(-200f, 200f), Random.Range(-100f, 100f), 0);

        float journeyTime = Vector3.Distance(startPos, targetPos) / particleSpeed;
        float timer = 0f;

        while (timer < journeyTime && pObj != null)
        {
            timer += Time.deltaTime;
            float percent = timer / journeyTime;
            
            // 위치 보간 (직선 + 살짝 퍼졌다가 모이는 곡선 느낌)
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, percent);
            currentPos += randomOffset * Mathf.Sin(percent * Mathf.PI); // 아치형 궤적

            pObj.transform.position = currentPos;
            yield return null;
        }

        // 3. 목적지 도착! 적에게 대미지를 입히고 파티클 파괴
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.TakeDamage(damageAmount);
            
            // TODO: 이곳에 "타격 파티클 이펙트"나 "카메라 쉐이크"를 넣으면 아주 좋습니다.
        }

        Destroy(pObj);
        activeParticles--;

        // 모든 파티클이 타격을 끝마쳤다면 다음 턴 준비를 시작합니다.
        if (activeParticles <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewTurn();
            }
        }
    }
}