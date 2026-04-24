using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopBallOfferUI : MonoBehaviour
{
    [SerializeField] private TMP_Text ballNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Image ballImage;
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text buyButtonText;

    private Func<ShopBallOfferUI, BallData, bool> purchaseCallback;
    private BallData ballData;

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

    private void OnClickBuyButton()
    {
        if (purchaseCallback == null)
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

        bool canAfford = GameManager.Instance.currentCoin >= ballData.price;
        bool deckIsFull = DeckManager.Instance != null && DeckManager.Instance.IsDeckFull;
        buyButton.interactable = canAfford && !deckIsFull;

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
