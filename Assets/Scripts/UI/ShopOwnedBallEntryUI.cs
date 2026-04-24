using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopOwnedBallEntryUI : MonoBehaviour
{
    [SerializeField] private Image ballImage;
    [SerializeField] private Button deleteButton;
    [SerializeField] private TMP_Text deletePriceText;

    private Func<ShopOwnedBallEntryUI, BallData, bool> deleteCallback;
    private BallData ballData;

    void Reset()
    {
        if (ballImage == null)
        {
            ballImage = GetComponent<Image>();
        }

        if (deleteButton == null)
        {
            deleteButton = GetComponentInChildren<Button>(true);
        }

        if (deletePriceText == null)
        {
            deletePriceText = GetComponentInChildren<TMP_Text>(true);
        }
    }

    void OnEnable()
    {
        RegisterListeners();
        Refresh();
    }

    void OnDisable()
    {
        UnregisterListeners();
    }

    public void Initialize(BallData ownedBall, Func<ShopOwnedBallEntryUI, BallData, bool> onDeleteRequested)
    {
        ballData = ownedBall;
        deleteCallback = onDeleteRequested;

        if (ballImage != null)
        {
            ballImage.sprite = ownedBall != null ? ownedBall.ballSprite : null;
            ballImage.enabled = ballImage.sprite != null;
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveListener(OnClickDeleteButton);
            deleteButton.onClick.AddListener(OnClickDeleteButton);
        }

        Refresh();
    }

    private void Refresh()
    {
        if (GameManager.Instance == null || DeckManager.Instance == null)
        {
            return;
        }

        int deleteCost = GameManager.Instance.GetDeckDeleteCost();
        bool hasEnoughCoin = GameManager.Instance.currentCoin >= deleteCost;
        bool canDelete = ballData != null && DeckManager.Instance.currentDeck.Count > 1;

        if (deletePriceText != null)
        {
            deletePriceText.text = canDelete ? deleteCost.ToString() : "KEEP 1";
        }

        if (deleteButton != null)
        {
            deleteButton.interactable = canDelete && hasEnoughCoin;
        }
    }

    private void OnClickDeleteButton()
    {
        if (deleteCallback == null)
        {
            return;
        }

        deleteCallback.Invoke(this, ballData);
    }

    private void RegisterListeners()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CoinChanged -= OnCoinChanged;
            GameManager.Instance.CoinChanged += OnCoinChanged;
            GameManager.Instance.DeckDeleteCostChanged -= OnDeckDeleteCostChanged;
            GameManager.Instance.DeckDeleteCostChanged += OnDeckDeleteCostChanged;
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
            GameManager.Instance.DeckDeleteCostChanged -= OnDeckDeleteCostChanged;
        }

        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.DeckChanged -= OnDeckChanged;
        }
    }

    private void OnCoinChanged(int _)
    {
        Refresh();
    }

    private void OnDeckDeleteCostChanged()
    {
        Refresh();
    }

    private void OnDeckChanged()
    {
        Refresh();
    }
}
