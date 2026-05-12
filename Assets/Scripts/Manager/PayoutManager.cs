using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PayoutManager : MonoBehaviour
{
    public static PayoutManager Instance;

    [Header("Text")]
    public TMP_Text remainingLifeRewardText;
    public TMP_Text interestRewardText;
    public TMP_Text totalRewardText;
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

    private int pendingPayoutAmount;
    private bool isPayoutReady;
    private bool isCoinListenerRegistered;

    void Awake()
    {
        if (Instance == null) Instance = this;

        if (payoutButton != null)
        {
            payoutButton.onClick.AddListener(OnClickPayoutButton);
        }
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

        int remainingLifeReward = GameManager.Instance.GetRemainingLifeReward();
        int interestReward = GameManager.Instance.GetInterestReward();
        int totalReward = remainingLifeReward + interestReward;

        SetTextsToInitialState();
        SetPayoutButtonVisible(false);

        yield return StartCoroutine(CountUpText(remainingLifeRewardText, remainingLifeReward, totalCoinAppearDuration));
        yield return new WaitForSeconds(sectionDelay);

        yield return StartCoroutine(CountUpText(interestRewardText, interestReward, totalCoinAppearDuration));
        yield return new WaitForSeconds(sectionDelay);

        yield return StartCoroutine(CountUpText(totalRewardText, totalReward, totalCoinAppearDuration));

        pendingPayoutAmount = totalReward;
        isPayoutReady = true;

        UpdatePayoutButtonText();
        SetPayoutButtonVisible(true);

        payoutRoutine = null;
    }

    private IEnumerator CountUpText(TMP_Text targetText, int targetAmount, float duration)
    {
        if (targetText == null)
        {
            yield break;
        }

        if (targetAmount <= 0 || duration <= 0f)
        {
            targetText.text = FormatRewardAmount(targetAmount);
            yield break;
        }

        float elapsed = 0f;
        int previousValue = -1;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            int value = Mathf.RoundToInt(Mathf.Lerp(0, targetAmount, t));

            if (value != previousValue)
            {
                targetText.text = FormatRewardAmount(value);
                previousValue = value;
            }

            yield return null;
        }

        targetText.text = FormatRewardAmount(targetAmount);
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
            remainingLifeRewardText.text = FormatRewardAmount(0);
        }

        if (interestRewardText != null)
        {
            interestRewardText.text = FormatRewardAmount(0);
        }

        if (totalRewardText != null)
        {
            totalRewardText.text = FormatRewardAmount(0);
        }

        UpdatePayoutButtonText();

        if (currentCoinText != null && GameManager.Instance != null)
        {
            RefreshCoinText(GameManager.Instance.currentCoin);
        }
    }

    private void UpdatePayoutButtonText()
    {
        if (payoutButtonText != null)
        {
            payoutButtonText.text = "PAY OUT";
        }
    }

    private void SetPayoutButtonVisible(bool value)
    {
        if (payoutButton != null)
        {
            payoutButton.gameObject.SetActive(value);
            payoutButton.interactable = value;
        }
    }

    private void SetPayoutButtonInteractable(bool value)
    {
        if (payoutButton != null)
        {
            payoutButton.interactable = value;
        }
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

    private string FormatRewardAmount(int amount)
    {
        return $"${amount}";
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
