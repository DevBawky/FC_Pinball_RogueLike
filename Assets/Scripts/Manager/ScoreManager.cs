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

    private IEnumerator CalculateFinalScoreRoutine()
    {
        // 1. 모든 파티클 도착 후 0.5초 대기 (정적)
        yield return new WaitForSeconds(0.5f);

        // 2. 최종 대미지 계산 및 화면 표시
        float totalDamage = currentChips * currentMult;
        if (totalDamageText != null)
        {
            totalDamageText.gameObject.SetActive(true);
            totalDamageText.text = Mathf.RoundToInt(totalDamage).ToString();
            
            // 연출: 텍스트 펀치 효과
            totalDamageText.transform.localScale = Vector3.one * 1.5f;
            float timer = 0f;
            while(timer < 0.2f)
            {
                totalDamageText.transform.localScale = Vector3.Lerp(totalDamageText.transform.localScale, Vector3.one, timer / 0.2f);
                timer += Time.deltaTime;
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.5f);

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.TakeDamage(totalDamage);
        }
        
        // 적이 대미지를 입고 반응할 시간(체력바 깎이는 연출 등)을 위해 살짝 대기
        yield return new WaitForSeconds(0.5f);

        // 5. 다음 라운드(턴) 장전 및 점수판 초기화
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewTurn();
        }
    }
}