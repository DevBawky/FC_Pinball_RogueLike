using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckDeletePanelUI : MonoBehaviour
{
    [SerializeField] private List<Transform> ownedDeckLayouts = new List<Transform>();
    [SerializeField] private GameObject ownedBallEntryPrefab;
    [SerializeField] private TMP_Text currentDeleteCostText;
    [SerializeField] private int slotsPerLayout = 5;

    private readonly List<ShopOwnedBallEntryUI> currentOwnedBallEntryUIs = new List<ShopOwnedBallEntryUI>();
    private Coroutine refreshAfterEnableRoutine;

    void OnEnable()
    {
        RegisterListeners();
        RefreshDisplay();

        if (refreshAfterEnableRoutine != null)
        {
            StopCoroutine(refreshAfterEnableRoutine);
        }

        refreshAfterEnableRoutine = StartCoroutine(RefreshAfterEnableRoutine());
    }

    void OnDisable()
    {
        if (refreshAfterEnableRoutine != null)
        {
            StopCoroutine(refreshAfterEnableRoutine);
            refreshAfterEnableRoutine = null;
        }

        UnregisterListeners();
    }

    public void RefreshDisplay()
    {
        RefreshCostText();
        RefreshOwnedDeckDisplay();
    }

    private void RefreshOwnedDeckDisplay()
    {
        ClearOwnedDeckDisplay();

        if (ownedBallEntryPrefab == null || ownedDeckLayouts.Count == 0 || DeckManager.Instance == null)
        {
            return;
        }

        int safeSlotsPerLayout = Mathf.Max(slotsPerLayout, 1);
        int maxDisplayCount = ownedDeckLayouts.Count * safeSlotsPerLayout;
        int ownedBallCount = Mathf.Min(DeckManager.Instance.currentDeck.Count, maxDisplayCount);

        for (int i = 0; i < ownedBallCount; i++)
        {
            int layoutIndex = i / safeSlotsPerLayout;
            if (layoutIndex >= ownedDeckLayouts.Count)
            {
                break;
            }

            Transform targetLayout = ownedDeckLayouts[layoutIndex];
            if (targetLayout == null)
            {
                continue;
            }

            GameObject entryObject = Instantiate(ownedBallEntryPrefab, targetLayout);
            ShopOwnedBallEntryUI entryUI = entryObject.GetComponent<ShopOwnedBallEntryUI>();

            if (entryUI != null)
            {
                entryUI.Initialize(DeckManager.Instance.currentDeck[i], TryDeleteOwnedBall);
                currentOwnedBallEntryUIs.Add(entryUI);
            }
        }

        ForceRebuildOwnedDeckLayouts();
    }

    private bool TryDeleteOwnedBall(ShopOwnedBallEntryUI entryUI, BallData ownedBall)
    {
        if (entryUI == null || ownedBall == null || GameManager.Instance == null || DeckManager.Instance == null)
        {
            return false;
        }

        int deleteCost = GameManager.Instance.GetDeckDeleteCost();
        if (!GameManager.Instance.TrySpendCoin(deleteCost))
        {
            return false;
        }

        if (!DeckManager.Instance.RemoveBallFromDeck(ownedBall))
        {
            GameManager.Instance.AddCoin(deleteCost);
            return false;
        }

        GameManager.Instance.RegisterDeckDeletePurchase();
        RefreshDisplay();
        return true;
    }

    private void RefreshCostText()
    {
        if (currentDeleteCostText == null || GameManager.Instance == null)
        {
            return;
        }

        currentDeleteCostText.text = GameManager.Instance.GetDeckDeleteCost().ToString();
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
        RefreshCostText();
    }

    private void OnDeckDeleteCostChanged()
    {
        RefreshDisplay();
    }

    private void OnDeckChanged()
    {
        RefreshDisplay();
    }

    private IEnumerator RefreshAfterEnableRoutine()
    {
        yield return null;

        RegisterListeners();
        RefreshDisplay();
        refreshAfterEnableRoutine = null;
    }

    private void ForceRebuildOwnedDeckLayouts()
    {
        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < ownedDeckLayouts.Count; i++)
        {
            RectTransform layoutRect = ownedDeckLayouts[i] as RectTransform;
            if (layoutRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
            }
        }
    }

    private void ClearOwnedDeckDisplay()
    {
        currentOwnedBallEntryUIs.Clear();

        for (int i = 0; i < ownedDeckLayouts.Count; i++)
        {
            Transform layout = ownedDeckLayouts[i];
            if (layout == null)
            {
                continue;
            }

            for (int childIndex = layout.childCount - 1; childIndex >= 0; childIndex--)
            {
                Destroy(layout.GetChild(childIndex).gameObject);
            }
        }
    }
}
