using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Turn States")]
    public bool isTurnActive = false;   // 런처가 발사를 시작하면 true
    public bool isSpawning = false;     // 코루틴에서 공을 쏘고 있는 중이면 true
    public bool isCalculating = false;  // 정산 코루틴이 돌고 있으면 true

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        // 턴이 시작되었고, 장전된 공을 모두 쏘았으며(isSpawning == false), 아직 정산 중이 아닐 때만 검사
        if (isTurnActive && !isSpawning && !isCalculating)
        {
            GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
            
            // 디버깅용: 만약 정산이 안 된다면, 콘솔창에서 파티클 갯수가 0이 안 되거나 공 갯수가 0이 아닌지 확인하세요!
            // Debug.Log($"남은 공: {balls.Length}, 남은 파티클: {UIManager.Instance.activeScoreParticles}");

            // 1. 맵 위에 공이 하나도 없고
            // 2. 날아가는 파티클도 모두 패널에 도착했다면 정산 시작
            if (balls.Length == 0 && MainGameUIManager.Instance.activeScoreParticles == 0)
            {
                isCalculating = true;
                ScoreManager.Instance.OnAllBallsDestroyed();
            }
        }
    }
    
    // BallLauncher에서 발사를 시작할 때 호출
    public void OnFireStarted()
    {
        isTurnActive = true;
        isSpawning = true;
        isCalculating = false;
        
        // 턴 시작 시 만약의 버그(이전 턴의 파티클 카운터 꼬임 등)를 방지하기 위해 0으로 강제 초기화
        if (MainGameUIManager.Instance != null)
        {
            MainGameUIManager.Instance.activeScoreParticles = 0;
        }
    }

    // BallLauncher에서 5발을 모두 스폰하고 나면 호출
    public void OnFireFinished()
    {
        isSpawning = false;
    }

    // 파티클이 적을 타격하고 난 뒤(턴 종료 직후) 다음 턴 장전을 위해 호출
    public void StartNewTurn()
    {
        Debug.Log("--- 새 턴(라운드) 장전 시작 ---");
        
        // 1. 모든 플래그 완벽하게 초기화 (다음 발사를 대기하는 상태)
        isTurnActive = false;
        isSpawning = false;
        isCalculating = false;

        // 2. 점수판 초기화
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetRound();
        }

        // 3. 덱 매니저에게 다음 공 5개 장전 지시
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.StartRound();
        }
    }
}