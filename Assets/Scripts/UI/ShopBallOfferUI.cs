using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopBallOfferUI : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float purchasedAlpha = 0.55f;
    [SerializeField] private TMP_Text ballNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Image ballImage;
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text buyButtonText;

    private Func<ShopBallOfferUI, BallData, bool> purchaseCallback;
    private BallData ballData;
    private CanvasGroup canvasGroup;
    private bool isPurchased;

    void OnEnable()
    {
        RegisterListeners();
        RefreshInteractable();
    }

    void OnDisable()
    {
        UnregisterListeners();
    }

    public void Initialize(BallData offeredBall, Func<ShopBallOfferUI, BallData, bool> onPurchaseRequested)
    {
        ballData = offeredBall;
        purchaseCallback = onPurchaseRequested;
        isPurchased = false;
        EnsureCanvasGroup();
        SetVisualAlpha(1f);

        if (ballNameText != null)
        {
            ballNameText.text = offeredBall != null ? offeredBall.ballName : "Unknown Ball";
        }

        if (descriptionText != null)
        {
            descriptionText.text = offeredBall != null ? offeredBall.description : string.Empty;
        }

        if (priceText != null)
        {
            priceText.text = offeredBall != null ? offeredBall.price.ToString() : "-";
        }

        if (ballImage != null)
        {
            ballImage.sprite = offeredBall != null ? offeredBall.ballSprite : null;
            ballImage.enabled = ballImage.sprite != null;
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(OnClickBuyButton);
            buyButton.onClick.AddListener(OnClickBuyButton);
        }

        RefreshInteractable();
    }

    public void MarkPurchased()
    {
        isPurchased = true;

        if (buyButton != null)
        {
            buyButton.interactable = false;
        }

        if (buyButtonText != null)
        {
            buyButtonText.text = "BOUGHT";
        }

        SetVisualAlpha(purchasedAlpha);
    }

    private void OnClickBuyButton()
    {
        if (isPurchased || purchaseCallback == null)
        {
            return;
        }

        purchaseCallback.Invoke(this, ballData);
    }

    private void RefreshInteractable()
    {
        if (buyButton == null || ballData == null || GameManager.Instance == null)
        {
            return;
        }

        if (isPurchased)
        {
            buyButton.interactable = false;
            SetVisualAlpha(purchasedAlpha);
            return;
        }

        bool canAfford = GameManager.Instance.currentCoin >= ballData.price;
        bool deckIsFull = DeckManager.Instance != null && DeckManager.Instance.IsDeckFull;
        buyButton.interactable = canAfford && !deckIsFull;
        SetVisualAlpha(1f);

        if (buyButtonText != null)
        {
            if (deckIsFull)
            {
                buyButtonText.text = "FULL";
            }
            else
            {
                buyButtonText.text = canAfford ? "BUY" : "NEED MORE";
            }
        }
    }

    private void EnsureCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void SetVisualAlpha(float alpha)
    {
        EnsureCanvasGroup();
        canvasGroup.alpha = alpha;
    }

    private void RegisterListeners()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CoinChanged -= OnCoinChanged;
            GameManager.Instance.CoinChanged += OnCoinChanged;
        }

        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.DeckChanged -= OnDeckChanged;
            DeckManager.Instance.DeckChanged += OnDeckChanged;
        }
    }

    private void UnregisterListeners()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CoinChanged -= OnCoinChanged;
        }

        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.DeckChanged -= OnDeckChanged;
        }
    }

    private void OnCoinChanged(int _)
    {
        RefreshInteractable();
    }

    private void OnDeckChanged()
    {
        RefreshInteractable();
    }
}
