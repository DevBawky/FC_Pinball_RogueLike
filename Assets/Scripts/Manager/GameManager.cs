using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GamePhase
    {
        MainMenu,
        StageSelection,
        Battle,
        PayOut,
        Shop,
        Maintenance
    }

    [Header("Game Phase")]
    public GamePhase currentPhase = GamePhase.MainMenu;

    [Header("Turn States")]
    public bool isTurnActive = false;
    public bool isSpawning = false;
    public bool isCalculating = false;

    [Header("Battle Life")]
    public int maxLifeCount = 5;
    public int currentLifeCount = 5;

    [Header("Coin")]
    public int currentCoin = 0;

    [Header("UI Panels")]
    public GameObject stageSelectionPanel;
    public GameObject mainGamePanel;
    public GameObject payoutPanel;
    public GameObject shopPanel;

    void Awake()
    {
        if (Instance == null) Instance = this;

        currentPhase = GamePhase.MainMenu;
    }

    void Start()
    {
        UpdatePanels();
    }

    void Update()
    {
        if (currentPhase == GamePhase.Battle && isTurnActive && !isSpawning && !isCalculating)
        {
            GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");

            if (balls.Length == 0 && MainGameUIManager.Instance.activeScoreParticles == 0)
            {
                isCalculating = true;
                ScoreManager.Instance.OnAllBallsDestroyed();
            }
        }
    }

    public void OnFireStarted()
    {
        isTurnActive = true;
        isSpawning = true;
        isCalculating = false;

        if (MainGameUIManager.Instance != null)
        {
            MainGameUIManager.Instance.activeScoreParticles = 0;
        }
    }

    public void OnFireFinished()
    {
        isSpawning = false;
    }

    public bool TryConsumeLife()
    {
        if (currentPhase != GamePhase.Battle) return false;
        if (currentLifeCount <= 0) return false;

        currentLifeCount--;

        if (LifeCountUI.Instance != null)
        {
            LifeCountUI.Instance.Refresh(currentLifeCount);
        }

        return true;
    }

    public void StartNewTurn()
    {
        Debug.Log("--- 전투 종료: PayOut 패널로 이동합니다 ---");

        isTurnActive = false;
        isSpawning = false;
        isCalculating = false;

        currentPhase = GamePhase.PayOut;
        UpdatePanels();

        if (PayoutManager.Instance != null)
        {
            PayoutManager.Instance.StartPayout();
        }
    }

    public void StartBattle()
    {
        Debug.Log("--- 배틀 시작 ---");

        currentPhase = GamePhase.Battle;

        currentLifeCount = maxLifeCount;

        UpdatePanels();

        if (LifeCountUI.Instance != null)
        {
            LifeCountUI.Instance.Initialize(maxLifeCount);
            LifeCountUI.Instance.Refresh(currentLifeCount);
        }

        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.StartRound();
        }
    }

    public void EnterShop()
    {
        currentPhase = GamePhase.Shop;
        UpdatePanels();
    }

    public void ReturnToStageSelection()
    {
        currentPhase = GamePhase.StageSelection;
        UpdatePanels();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetRound();
        }
    }

    private void UpdatePanels()
    {
        if (stageSelectionPanel != null) stageSelectionPanel.SetActive(currentPhase == GamePhase.StageSelection);
        if (mainGamePanel != null) mainGamePanel.SetActive(currentPhase == GamePhase.Battle);
        if (payoutPanel != null) payoutPanel.SetActive(currentPhase == GamePhase.PayOut);
        if (shopPanel != null) shopPanel.SetActive(currentPhase == GamePhase.Shop || currentPhase == GamePhase.Maintenance);
    }

    public void PrepareNextAttack()
    {
        if (currentPhase == GamePhase.Battle)
        {
            isTurnActive = false;
            isSpawning = false;
            isCalculating = false;

            if (currentLifeCount <= 0)
            {
                StartNewTurn();
                return;
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetRound();
            }

            if (DeckManager.Instance != null)
            {
                DeckManager.Instance.StartRound();
            }

            Debug.Log("--- 대미지 정산 완료. 적이 살아있습니다. 다음 공격을 장전합니다! ---");
        }
    }

    public int GetRemainingLifeReward()
    {
        return currentLifeCount;
    }

    public int GetInterestReward()
    {
        return currentCoin / 5;
    }

    public void AddCoin(int amount)
    {
        currentCoin += amount;
    }
}