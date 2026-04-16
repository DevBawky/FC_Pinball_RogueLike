using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Turn States")]
    public bool isTurnActive = false;   // 플레이어가 발사해서 턴이 진행 중인지
    public bool isSpawning = false;     // 런처가 아직 공을 생성(장전) 중인지
    private bool isCalculating = false; // 정산 중인지

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        // 핵심 변경점: 턴이 진행 중이고, 공 스폰이 완전히 끝났으며, 정산 중이 아닐 때만 감지합니다.
        if (isTurnActive && !isSpawning && !isCalculating)
        {
            GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");

            // 공도 다 파괴되었고, 날아가는 파티클도 없으면 정산 시작
            if (balls.Length == 0 && UIManager.Instance.activeScoreParticles == 0)
            {
                isCalculating = true;
                ScoreManager.Instance.OnAllBallsDestroyed();
            }
        }
    }
    
    // --- 런처(BallLauncher)에서 호출해 줄 함수들 ---

    public void OnFireStarted()
    {
        isTurnActive = true;
        isSpawning = true;
        isCalculating = false;
    }

    public void OnFireFinished()
    {
        isSpawning = false;
    }

    // 다음 라운드를 시작할 때 호출
    public void StartNewTurn()
    {
        // 1. 상태 플래그들 초기화
        isTurnActive = false;
        isSpawning = false;
        isCalculating = false;

        // 2. 점수 매니저 초기화 (텍스트 숨기기, 점수 0/1로 리셋)
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetRound();
        }

        // 3. 덱 매니저에게 다음 라운드용 공 5개 장전 요청
        if (DeckManager.Instance != null)
        {
            // DeckManager에 작성해둔 StartRound()가 실행되면서 
            // 새로운 BallData 5개를 뽑고 UI(MagazineUI)를 갱신합니다.
            DeckManager.Instance.StartRound();
        }

        Debug.Log("새 라운드 시작: 공 장전 완료.");
    }
}