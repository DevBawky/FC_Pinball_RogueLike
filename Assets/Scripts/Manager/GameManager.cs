using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public event Action<int> CoinChanged;

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
    [SerializeField, Min(0.05f)] private float ballCountCheckInterval = 0.25f;
    [SerializeField, Min(0f)] private float scoreParticleWaitTimeout = 3f;
    private float scoreParticleWaitTimer = 0f;
    private Coroutine ballCountMonitorRoutine;

    [Header("Battle Life")]
    public int maxLifeCount = 5;
    public int currentLifeCount = 5;

    [Header("Coin")]
    public int currentCoin = 0;
    public event Action<ShopUpgradeType> ShopUpgradeChanged;
    public event Action DeckDeleteCostChanged;

    [Header("Shop Upgrades - Ball Max Health")]
    [SerializeField] private int ballMaxHealthUpgradeLevel = 0;
    [SerializeField] private float[] ballMaxHealthRatioByLevel = { 0f, 0.05f, 0.10f, 0.15f };
    [SerializeField] private int[] ballMaxHealthUpgradeCosts = { 3, 6, 10 };

    [Header("Shop Upgrades - Wall Damage Reduction")]
    [SerializeField] private int wallCollisionDamageReductionLevel = 0;
    [SerializeField] private float[] wallCollisionDamageReductionByLevel = { 0f, 0.05f, 0.10f, 0.15f };
    [SerializeField] private int[] wallCollisionDamageReductionCosts = { 3, 6, 10 };

    [Header("Shop Upgrades - Score Gain")]
    [SerializeField] private int scoreGainUpgradeLevel = 0;
    [SerializeField] private ScoreUpgradeValue[] scoreGainByLevel =
    {
        new ScoreUpgradeValue(0, 0f),
        new ScoreUpgradeValue(1, 0.1f),
        new ScoreUpgradeValue(2, 0.2f),
        new ScoreUpgradeValue(3, 0.3f)
    };
    [SerializeField] private int[] scoreGainUpgradeCosts = { 4, 7, 11 };

    [Header("Shop - Deck Delete")]
    [SerializeField] private int deckDeleteBaseCost = 5;
    [SerializeField] private int deckDeleteCostIncrease = 3;
    [SerializeField] private int deckDeletePurchaseCount = 0;

    [Header("UI Panels")]
    public GameObject stageSelectionPanel;
    public GameObject mainGamePanel;
    public GameObject playerPanel;
    public GameObject payoutPanel;
    public GameObject shopPanel;

    [Header("Battle Content")]
    [SerializeField] private BattleObjectSpawner battleObjectSpawner;
    [SerializeField] private BattleCameraShake battleCameraShake;
    private Coroutine startBattleRoutine;
    private StageType currentBattleStageType = StageType.Battle;
    private int currentBattleStageNumber = 1;

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
        if (ballCountMonitorRoutine == null
            && currentPhase == GamePhase.Battle
            && isTurnActive
            && !isSpawning
            && !isCalculating)
        {
            CheckForTurnCompletion(Time.deltaTime);
        }
    }

    public void OnFireStarted()
    {
        isTurnActive = true;
        isSpawning = true;
        isCalculating = false;
        scoreParticleWaitTimer = 0f;

        if (MainGameUIManager.Instance != null)
        {
            MainGameUIManager.Instance.activeScoreParticles = 0;
        }

        StartBattleCameraShake();
        StartBallCountMonitor();
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

    public void GoToPayout()
    {
        Debug.Log("--- 전투 종료: PayOut 패널로 이동합니다 ---");

        isTurnActive = false;
        isSpawning = false;
        isCalculating = false;
        scoreParticleWaitTimer = 0f;
        StopBallCountMonitor();
        StopBattleCameraShakeImmediately();

        currentPhase = GamePhase.PayOut;
        UpdatePanels();

        if (PayoutManager.Instance != null)
        {
            PayoutManager.Instance.StartPayout();
        }
    }

    public void StartBattle()
    {
        StartBattle(StageType.Battle);
    }

    public void StartBattle(StageType stageType)
    {
        StartBattle(stageType, 1);
    }

    public void StartBattle(StageType stageType, int stageNumber)
    {
        Debug.Log("--- 배틀 시작 ---");

        currentBattleStageType = stageType;
        currentBattleStageNumber = Mathf.Max(1, stageNumber);
        currentPhase = GamePhase.Battle;
        isTurnActive = false;
        isSpawning = false;
        isCalculating = false;
        scoreParticleWaitTimer = 0f;
        StopBallCountMonitor();

        currentLifeCount = maxLifeCount;

        UpdatePanels();

        if (startBattleRoutine != null)
        {
            StopCoroutine(startBattleRoutine);
        }

        startBattleRoutine = StartCoroutine(StartBattleRoutine());
    }

    public void EnterShop()
    {
        currentPhase = GamePhase.Shop;
        UpdatePanels();
    }

    public void ReturnToStageSelection()
    {
        currentPhase = GamePhase.StageSelection;
        isTurnActive = false;
        isSpawning = false;
        isCalculating = false;
        scoreParticleWaitTimer = 0f;
        StopBallCountMonitor();
        StopBattleCameraShakeImmediately();
        UpdatePanels();

        if (MainGameUIManager.Instance != null)
        {
            MainGameUIManager.Instance.activeScoreParticles = 0;
        }

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RestoreStageVisuals();
        }

        if (battleObjectSpawner != null)
        {
            battleObjectSpawner.ClearSpawnedObjects();
        }

        if (MainMenuUIManager.Instance != null)
        {
            MainMenuUIManager.Instance.RefreshStageSelectionFromShop();
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetRound();
        }
    }

    private void UpdatePanels()
    {
        DissolveRevealPanelUI.SetActiveWithDissolve(stageSelectionPanel, currentPhase == GamePhase.StageSelection);
        DissolveRevealPanelUI.SetActiveWithDissolve(mainGamePanel, currentPhase == GamePhase.Battle);
        UpdatePlayerPanel();
        DissolveRevealPanelUI.SetActiveWithDissolve(payoutPanel, currentPhase == GamePhase.PayOut);
        DissolveRevealPanelUI.SetActiveWithDissolve(shopPanel, currentPhase == GamePhase.Shop || currentPhase == GamePhase.Maintenance);
    }

    private void UpdatePlayerPanel()
    {
        if (playerPanel == null)
        {
            return;
        }

        if (currentPhase == GamePhase.MainMenu)
        {
            playerPanel.SetActive(false);
            return;
        }

        if (!playerPanel.activeSelf)
        {
            DissolveRevealPanelUI.SetActiveWithDissolve(playerPanel, true);
        }
    }

    private IEnumerator StartBattleRoutine()
    {
        yield return null;

        if (MainGameUIManager.Instance != null)
        {
            MainGameUIManager.Instance.activeScoreParticles = 0;
        }

        if (LifeCountUI.Instance != null)
        {
            LifeCountUI.Instance.Initialize(maxLifeCount);
            LifeCountUI.Instance.Refresh(currentLifeCount);
        }

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.InitializeBattleEnemy(currentBattleStageType == StageType.BossBattle, currentBattleStageNumber);
        }

        if (battleObjectSpawner != null)
        {
            battleObjectSpawner.SpawnForBattle();
        }

        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.StartRound();
        }

        startBattleRoutine = null;
    }

    public void PrepareNextAttack()
    {
        if (currentPhase == GamePhase.Battle)
        {
            isTurnActive = false;
            isSpawning = false;
            isCalculating = false;
            scoreParticleWaitTimer = 0f;
            StopBallCountMonitor();
            StopBattleCameraShakeImmediately();

            if (currentLifeCount <= 0)
            {
                GoToPayout();
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

    private IEnumerator BallCountMonitorRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(ballCountCheckInterval);

        while (currentPhase == GamePhase.Battle && isTurnActive)
        {
            if (!isSpawning && !isCalculating)
            {
                CheckForTurnCompletion(ballCountCheckInterval);

                if (isCalculating || !isTurnActive || currentPhase != GamePhase.Battle)
                {
                    break;
                }
            }

            yield return wait;
        }

        ballCountMonitorRoutine = null;
    }

    private void StartBallCountMonitor()
    {
        StopBallCountMonitor();
        ballCountMonitorRoutine = StartCoroutine(BallCountMonitorRoutine());
    }

    private void StopBallCountMonitor()
    {
        if (ballCountMonitorRoutine == null)
        {
            return;
        }

        StopCoroutine(ballCountMonitorRoutine);
        ballCountMonitorRoutine = null;
    }

    private void CheckForTurnCompletion(float elapsedTime)
    {
        if (CountActiveBalls() == 0)
        {
            StopBattleCameraShake();
            TryStartFinalDamageCalculation(elapsedTime);
        }
        else
        {
            scoreParticleWaitTimer = 0f;
        }
    }

    private int CountActiveBalls()
    {
        BallController[] balls = FindObjectsByType<BallController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        return balls.Length;
    }

    private void TryStartFinalDamageCalculation(float elapsedTime)
    {
        int activeScoreParticles = MainGameUIManager.Instance != null
            ? MainGameUIManager.Instance.activeScoreParticles
            : 0;

        if (activeScoreParticles > 0)
        {
            scoreParticleWaitTimer += elapsedTime;

            if (scoreParticleWaitTimer < scoreParticleWaitTimeout)
            {
                return;
            }

            Debug.LogWarning($"Score particles did not finish within {scoreParticleWaitTimeout:0.##} seconds. Forcing final damage calculation.");

            if (MainGameUIManager.Instance != null)
            {
                MainGameUIManager.Instance.activeScoreParticles = 0;
            }
        }

        scoreParticleWaitTimer = 0f;
        isCalculating = true;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnAllBallsDestroyed();
        }
        else
        {
            Debug.LogWarning("ScoreManager is missing. Preparing the next attack without final damage calculation.");
            PrepareNextAttack();
        }
    }

    private void StartBattleCameraShake()
    {
        BattleCameraShake shake = GetBattleCameraShake();

        if (shake != null)
        {
            shake.StartShake();
        }
    }

    private void StopBattleCameraShake()
    {
        BattleCameraShake shake = GetBattleCameraShake();

        if (shake != null)
        {
            shake.StopShake();
        }
    }

    private void StopBattleCameraShakeImmediately()
    {
        BattleCameraShake shake = GetBattleCameraShake();

        if (shake != null)
        {
            shake.StopShakeImmediately();
        }
    }

    private BattleCameraShake GetBattleCameraShake()
    {
        if (battleCameraShake == null)
        {
            battleCameraShake = FindFirstObjectByType<BattleCameraShake>();
        }

        return battleCameraShake;
    }

    public int GetInterestReward()
    {
        return currentCoin / 5;
    }

    public float GetBallMaxHealthRatio()
    {
        return GetCurrentValue(ballMaxHealthRatioByLevel, ballMaxHealthUpgradeLevel);
    }

    public float GetUpgradedBallMaxHealth(float baseMaxHealth)
    {
        return baseMaxHealth * (1f + GetBallMaxHealthRatio());
    }

    public float GetWallCollisionDamageReductionRatio()
    {
        return GetCurrentValue(wallCollisionDamageReductionByLevel, wallCollisionDamageReductionLevel);
    }

    public float ApplyWallCollisionDamageReduction(float baseWallDamage)
    {
        return baseWallDamage * (1f - GetWallCollisionDamageReductionRatio());
    }

    public ScoreUpgradeValue GetScoreGainUpgradeValue()
    {
        return GetCurrentValue(scoreGainByLevel, scoreGainUpgradeLevel);
    }

    public float GetUpgradedScoreValue(ScoreType scoreType, float baseScoreValue)
    {
        ScoreUpgradeValue scoreUpgrade = GetScoreGainUpgradeValue();

        if (scoreType == ScoreType.Chips)
        {
            return baseScoreValue + scoreUpgrade.chipsBonus;
        }

        return baseScoreValue + scoreUpgrade.multiplierBonus;
    }

    public int GetUpgradeLevel(ShopUpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case ShopUpgradeType.BallMaxHealthRatio:
                return ballMaxHealthUpgradeLevel;
            case ShopUpgradeType.WallCollisionDamageReduction:
                return wallCollisionDamageReductionLevel;
            case ShopUpgradeType.ScoreGain:
                return scoreGainUpgradeLevel;
            default:
                return 0;
        }
    }

    public bool CanUpgrade(ShopUpgradeType upgradeType)
    {
        return GetUpgradeLevel(upgradeType) < GetMaxUpgradeLevel(upgradeType);
    }

    public bool TryUpgrade(ShopUpgradeType upgradeType)
    {
        if (!CanUpgrade(upgradeType))
        {
            return false;
        }

        switch (upgradeType)
        {
            case ShopUpgradeType.BallMaxHealthRatio:
                ballMaxHealthUpgradeLevel++;
                break;
            case ShopUpgradeType.WallCollisionDamageReduction:
                wallCollisionDamageReductionLevel++;
                break;
            case ShopUpgradeType.ScoreGain:
                scoreGainUpgradeLevel++;
                break;
        }

        ShopUpgradeChanged?.Invoke(upgradeType);
        return true;
    }

    public int GetUpgradeCost(ShopUpgradeType upgradeType)
    {
        int currentLevel = GetUpgradeLevel(upgradeType);
        int[] costs = GetUpgradeCosts(upgradeType);

        if (costs == null || currentLevel >= costs.Length)
        {
            return -1;
        }

        return costs[currentLevel];
    }

    public bool TryPurchaseUpgrade(ShopUpgradeType upgradeType)
    {
        int upgradeCost = GetUpgradeCost(upgradeType);
        if (upgradeCost < 0)
        {
            return false;
        }

        if (!TrySpendCoin(upgradeCost))
        {
            return false;
        }

        if (TryUpgrade(upgradeType))
        {
            return true;
        }

        AddCoin(upgradeCost);
        return false;
    }

    public int GetDeckDeleteCost()
    {
        return deckDeleteBaseCost + (deckDeletePurchaseCount * deckDeleteCostIncrease);
    }

    public bool CanPurchaseDeckDelete()
    {
        if (DeckManager.Instance == null)
        {
            return false;
        }

        return DeckManager.Instance.currentDeck.Count > 1 && currentCoin >= GetDeckDeleteCost();
    }

    public void RegisterDeckDeletePurchase()
    {
        deckDeletePurchaseCount++;
        DeckDeleteCostChanged?.Invoke();
    }

    public string GetUpgradeDisplayText(ShopUpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case ShopUpgradeType.BallMaxHealthRatio:
                return GetPercentUpgradeDisplay(ballMaxHealthRatioByLevel, ballMaxHealthUpgradeLevel);
            case ShopUpgradeType.WallCollisionDamageReduction:
                return GetPercentUpgradeDisplay(wallCollisionDamageReductionByLevel, wallCollisionDamageReductionLevel);
            case ShopUpgradeType.ScoreGain:
                return GetScoreUpgradeDisplay(scoreGainByLevel, scoreGainUpgradeLevel);
            default:
                return string.Empty;
        }
    }

    public void AddCoin(int amount)
    {
        currentCoin += amount;
        CoinChanged?.Invoke(currentCoin);
    }

    public bool TrySpendCoin(int amount)
    {
        if (amount < 0)
        {
            return false;
        }

        if (currentCoin < amount)
        {
            return false;
        }

        currentCoin -= amount;
        CoinChanged?.Invoke(currentCoin);
        return true;
    }

    public void NotifyCoinChanged()
    {
        CoinChanged?.Invoke(currentCoin);
    }

    private int GetMaxUpgradeLevel(ShopUpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case ShopUpgradeType.BallMaxHealthRatio:
                return Mathf.Max(ballMaxHealthRatioByLevel.Length - 1, 0);
            case ShopUpgradeType.WallCollisionDamageReduction:
                return Mathf.Max(wallCollisionDamageReductionByLevel.Length - 1, 0);
            case ShopUpgradeType.ScoreGain:
                return Mathf.Max(scoreGainByLevel.Length - 1, 0);
            default:
                return 0;
        }
    }

    private int[] GetUpgradeCosts(ShopUpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case ShopUpgradeType.BallMaxHealthRatio:
                return ballMaxHealthUpgradeCosts;
            case ShopUpgradeType.WallCollisionDamageReduction:
                return wallCollisionDamageReductionCosts;
            case ShopUpgradeType.ScoreGain:
                return scoreGainUpgradeCosts;
            default:
                return null;
        }
    }

    private float GetCurrentValue(float[] values, int level)
    {
        if (values == null || values.Length == 0)
        {
            return 0f;
        }

        return values[Mathf.Clamp(level, 0, values.Length - 1)];
    }

    private ScoreUpgradeValue GetCurrentValue(ScoreUpgradeValue[] values, int level)
    {
        if (values == null || values.Length == 0)
        {
            return default;
        }

        return values[Mathf.Clamp(level, 0, values.Length - 1)];
    }

    private string GetPercentUpgradeDisplay(float[] values, int level)
    {
        float currentValue = GetCurrentValue(values, level);

        if (level >= Mathf.Max(values.Length - 1, 0))
        {
            return $"{FormatPercent(currentValue)} (MAX)";
        }

        float nextValue = GetCurrentValue(values, level + 1);
        return $"{FormatPercent(currentValue)} -> {FormatPercent(nextValue)}";
    }

    private string GetScoreUpgradeDisplay(ScoreUpgradeValue[] values, int level)
    {
        ScoreUpgradeValue currentValue = GetCurrentValue(values, level);

        if (level >= Mathf.Max(values.Length - 1, 0))
        {
            return $"{FormatScoreUpgrade(currentValue)} (MAX)";
        }

        ScoreUpgradeValue nextValue = GetCurrentValue(values, level + 1);
        return $"{FormatScoreUpgrade(currentValue)} -> {FormatScoreUpgrade(nextValue)}";
    }

    private string FormatPercent(float value)
    {
        return $"{value * 100f:0.#}%";
    }

    private string FormatScoreUpgrade(ScoreUpgradeValue value)
    {
        return $"+{value.chipsBonus}, x{value.multiplierBonus:0.##}";
    }
}
