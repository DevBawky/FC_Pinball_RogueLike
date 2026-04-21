using UnityEngine;
using TMPro;
using System.Collections; // TextMeshPro 사용

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI Text References")]
    public TextMeshProUGUI chipsText; // 붉은색 패널의 텍스트
    public TextMeshProUGUI multText;  // 푸른색 패널의 텍스트
    public TextMeshProUGUI totalDamageText; // 최종 합산 대미지가 표시될 텍스트

    [Header("Current Score")]
    public float currentChips = 0f;
    public float currentMult = 1f;

    // UI에 보여질(서서히 올라갈) 가짜 점수
    private float displayChips = 0f;
    private float displayMult = 1f;

    [Header("Animation")]
    public float rollSpeed = 10f; // 숫자가 굴러가는 속도

    [SerializeField] HealthBarUI _enemyHP;
    Coroutine textReduceCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void OnAllBallsDestroyed()
    {
        StartCoroutine(CalculateFinalScoreRoutine());
    }

    public void ResetRound()
    {
        currentChips = 0;
        currentMult = 1;
        
        // UI 텍스트 초기화
        chipsText.text = "0";
        multText.text = "1.0";
        
        // 최종 대미지 텍스트 숨기기
        if (totalDamageText != null)
        {
            totalDamageText.text = "0";
        }
    }

    void Update()
    {
        // 칩(합) UI 숫자 부드럽게 올리기
        if (Mathf.Abs(displayChips - currentChips) > 0.5f)
        {
            displayChips = Mathf.Lerp(displayChips, currentChips, Time.deltaTime * rollSpeed);
            chipsText.text = Mathf.RoundToInt(displayChips).ToString();
        }

        // 배수(곱) UI 숫자 부드럽게 올리기 (배수는 소수점 한자리 표시)
        if (Mathf.Abs(displayMult - currentMult) > 0.05f)
        {
            displayMult = Mathf.Lerp(displayMult, currentMult, Time.deltaTime * rollSpeed);
            multText.text = displayMult.ToString("F1");
        }
    }

    // 날아온 파티클이 도착했을 때 UIManager가 이 함수를 호출합니다.
    public void AddScore(ScoreType type, float value)
    {
        if (type == ScoreType.Chips)
        {
            currentChips += value;
        }
        else if (type == ScoreType.Multiplier)
        {
            currentMult += value;
        }
        
        // 텍스트가 쿵 하고 커지는 펀치 연출을 원한다면 여기에 추가 가능
    }

    // 턴이 끝났을 때 결산 (추후 구현)
    public float CalculateTotalDamage()
    {
        return currentChips * currentMult;
    }

    // ScoreManager.cs 내부의 코루틴 수정

    private IEnumerator CalculateFinalScoreRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        float totalDamage = currentChips * currentMult;
        
        if (totalDamageText != null)
        {
            totalDamageText.gameObject.SetActive(true);
            totalDamageText.text = Mathf.RoundToInt(totalDamage).ToString();
            
            totalDamageText.transform.localScale = Vector3.one * 1.5f;
            float timer = 0f;
            while(timer < 0.2f)
            {
                totalDamageText.transform.localScale = Vector3.Lerp(totalDamageText.transform.localScale, Vector3.one, timer / 0.2f);
                timer += Time.deltaTime;
                yield return null;
            }
        }

        // 잭팟 텍스트를 감상할 시간 0.5초 부여
        yield return new WaitForSeconds(0.5f);

        if (DamageParticleManager.Instance != null)
        {
            DamageParticleManager.Instance.FireDamageParticles(totalDamage);
        }

    }

    public void ReduceTotalDamageText(float startValue, float endValue, float duration)
    {
        // 이미 숫자가 줄어들고 있다면 멈추고 새로운 목표치로 갱신
        if (textReduceCoroutine != null) StopCoroutine(textReduceCoroutine);
        textReduceCoroutine = StartCoroutine(ReduceTextRoutine(startValue, endValue, duration));
    }
    
    private IEnumerator ReduceTextRoutine(float start, float end, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            
            // start에서 end로 지정된 시간(duration)동안 부드럽게 감소
            float current = Mathf.Lerp(start, end, timer / duration);
            
            if (totalDamageText != null) 
            {
                totalDamageText.text = Mathf.RoundToInt(current).ToString();
            }
            yield return null;
        }
        
        // 목표치에 완벽하게 도달하도록 보정
        if (totalDamageText != null) 
        {
            totalDamageText.text = Mathf.RoundToInt(end).ToString();
        }
    }
}