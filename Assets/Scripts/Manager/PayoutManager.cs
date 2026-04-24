using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PayoutManager : MonoBehaviour
{
    public static PayoutManager Instance;

    [Header("Coin UI")]
    public Transform remainingLifeCoinParent;
    public Transform interestCoinParent;
    public GameObject coinIconPrefab;

    [Header("Text")]
    public TMP_Text remainingLifeRewardText;
    public TMP_Text interestRewardText;
    public TMP_Text payoutButtonText;
    public TMP_Text currentCoinText;

    [Header("Button")]
    public Button payoutButton;

    [Header("Timing")]
    public float totalCoinAppearDuration = 1.2f;
    public float sectionDelay = 0.25f;
    public float coinCountUpDuration = 0.5f;

    private Coroutine payoutRoutine;
    private Coroutine coinCountRoutine;

    private int pendingPayoutAmount = 0;
    private bool isPayoutReady = false;
    private bool isCoinListenerRegistered = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        payoutButton.onClick.AddListener(OnClickPayoutButton);
    }

    void Start()
    {
        TryRegisterCoinListener();

        if (GameManager.Instance != null)
        {
            RefreshCoinText(GameManager.Instance.currentCoin);
        }
    }

    void OnEnable()
    {
        TryRegisterCoinListener();
    }

    void OnDisable()
    {
        UnregisterCoinListener();
    }

    public void StartPayout()
    {
        if (payoutRoutine != null)
        {
            StopCoroutine(payoutRoutine);
        }

        payoutRoutine = StartCoroutine(PayoutRoutine());
    }

    private IEnumerator PayoutRoutine()
    {
        isPayoutReady = false;
        pendingPayoutAmount = 0;

        ClearCoinIcons();

        int remainingLifeReward = GameManager.Instance.GetRemainingLifeReward();
        int interestReward = GameManager.Instance.GetInterestReward();
        int totalReward = remainingLifeReward + interestReward;

        SetTextsToInitialState();
        SetPayoutButtonInteractable(false);

        int displayedTotal = 0;

        float coinInterval = CalculateCoinInterval(totalReward);

        yield return StartCoroutine(SpawnCoinSection(
            remainingLifeCoinParent,
            remainingLifeReward,
            displayedTotal,
            coinInterval,
            value =>
            {
                displayedTotal = value;
                UpdatePayoutButtonText(displayedTotal);
            }
        ));

        yield return new WaitForSeconds(sectionDelay);

        yield return StartCoroutine(SpawnCoinSection(
            interestCoinParent,
            interestReward,
            displayedTotal,
            coinInterval,
            value =>
            {
                displayedTotal = value;
                UpdatePayoutButtonText(displayedTotal);
            }
        ));

        pendingPayoutAmount = displayedTotal;
        isPayoutReady = true;

        SetPayoutButtonInteractable(true);

        payoutRoutine = null;
    }

    private float CalculateCoinInterval(int totalReward)
    {
        if (totalReward <= 0)
        {
            return 0f;
        }

        return totalCoinAppearDuration / totalReward;
    }

    private IEnumerator SpawnCoinSection(
        Transform parent,
        int amount,
        int startTotal,
        float coinInterval,
        System.Action<int> onTotalChanged)
    {
        int total = startTotal;

        for (int i = 0; i < amount; i++)
        {
            Instantiate(coinIconPrefab, parent);

            total++;
            onTotalChanged?.Invoke(total);

            if (coinInterval > 0f)
            {
                yield return new WaitForSeconds(coinInterval);
            }
        }
    }

    public void OnClickPayoutButton()
    {
        if (!isPayoutReady) return;

        isPayoutReady = false;
        SetPayoutButtonInteractable(false);

        if (payoutButtonText != null)
        {
            payoutButtonText.text = pendingPayoutAmount > 0 ? "PAYING..." : "PAID";
        }

        if (coinCountRoutine != null)
        {
            StopCoroutine(coinCountRoutine);
        }

        coinCountRoutine = StartCoroutine(CountUpCoinAndEnterShopRoutine());
    }

    private void SetTextsToInitialState()
    {
        if (remainingLifeRewardText != null)
        {
            remainingLifeRewardText.text = "Remaining Lives";
        }

        if (interestRewardText != null)
        {
            interestRewardText.text = "Interest";
        }

        if (payoutButtonText != null)
        {
            payoutButtonText.text = "PAY OUT $0";
        }

        if (currentCoinText != null)
        {
            RefreshCoinText(GameManager.Instance.currentCoin);
        }
    }

    private void UpdatePayoutButtonText(int amount)
    {
        if (payoutButtonText != null)
        {
            payoutButtonText.text = $"PAY OUT ${amount}";
        }
    }

    private void SetPayoutButtonInteractable(bool value)
    {
        if (payoutButton != null)
        {
            payoutButton.interactable = value;
        }
    }

    private void ClearCoinIcons()
    {
        ClearChildren(remainingLifeCoinParent);
        ClearChildren(interestCoinParent);
    }

    private IEnumerator CountUpCoinAndEnterShopRoutine()
    {
        int amountToAdd = pendingPayoutAmount;

        pendingPayoutAmount = 0;

        if (amountToAdd > 0)
        {
            float stepDelay = coinCountUpDuration / amountToAdd;

            for (int i = 1; i <= amountToAdd; i++)
            {
                GameManager.Instance.AddCoin(1);

                yield return new WaitForSeconds(stepDelay);
            }
        }

        if (payoutButtonText != null)
        {
            payoutButtonText.text = "PAID";
        }

        coinCountRoutine = null;
        GameManager.Instance.EnterShop();
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    private void RefreshCoinText(int currentCoin)
    {
        if (currentCoinText != null)
        {
            currentCoinText.text = $"{currentCoin}";
        }
    }

    private void TryRegisterCoinListener()
    {
        if (isCoinListenerRegistered)
        {
            return;
        }

        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.CoinChanged += RefreshCoinText;
        isCoinListenerRegistered = true;
    }

    private void UnregisterCoinListener()
    {
        if (!isCoinListenerRegistered)
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CoinChanged -= RefreshCoinText;
        }

        isCoinListenerRegistered = false;
    }
}
