using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Ball Offer")]
    [SerializeField] private List<BallData> ballShopPool = new List<BallData>();
    [SerializeField] private Transform ballOfferParent;
    [SerializeField] private GameObject ballOfferPrefab;
    [SerializeField] private int ballOfferCount = 4;

    [Header("Owned Deck")]
    [SerializeField] private List<Transform> ownedDeckLayouts = new List<Transform>();
    [SerializeField] private GameObject ownedBallEntryPrefab;
    [SerializeField] private int slotsPerLayout = 5;

    [Header("Upgrade Panels")]
    [SerializeField] private List<ShopUpgradePanelUI> upgradePanels = new List<ShopUpgradePanelUI>();

    private readonly List<ShopBallOfferUI> currentBallOfferUIs = new List<ShopBallOfferUI>();
    private readonly List<ShopOwnedBallEntryUI> currentOwnedBallEntryUIs = new List<ShopOwnedBallEntryUI>();

    void OnEnable()
    {
        RegisterListeners();
        GenerateBallOffer();
        RefreshOwnedDeckDisplay();
        RefreshUpgradePanels();
    }

    void OnDisable()
    {
        UnregisterListeners();
    }

    public void GenerateBallOffer()
    {
        ClearBallOffer();

        if (ballOfferPrefab == null || ballOfferParent == null || ballShopPool.Count == 0)
        {
            return;
        }

        List<BallData> shuffledPool = new List<BallData>(ballShopPool);
        Shuffle(shuffledPool);

        int offerSpawnCount = Mathf.Min(ballOfferCount, shuffledPool.Count);

        for (int i = 0; i < offerSpawnCount; i++)
        {
            BallData offeredBall = shuffledPool[i];
            GameObject offerObject = Instantiate(ballOfferPrefab, ballOfferParent);
            ShopBallOfferUI offerUI = offerObject.GetComponent<ShopBallOfferUI>();

            if (offerUI != null)
            {
                offerUI.Initialize(offeredBall, TryBuyBallOffer);
                currentBallOfferUIs.Add(offerUI);
            }

            DissolveRevealPanelUI revealUI = offerObject.GetComponent<DissolveRevealPanelUI>();
            if (revealUI == null)
            {
                revealUI = offerObject.AddComponent<DissolveRevealPanelUI>();
            }

            revealUI.PlayReveal();
        }
    }

    public void RefreshOwnedDeckDisplay()
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
    }

    public void RefreshUpgradePanels()
    {
        for (int i = 0; i < upgradePanels.Count; i++)
        {
            if (upgradePanels[i] != null)
            {
                upgradePanels[i].Refresh();
            }
        }
    }

    public bool TryBuyBallOffer(ShopBallOfferUI offerUI, BallData offeredBall)
    {
        if (offerUI == null || offeredBall == null || GameManager.Instance == null || DeckManager.Instance == null)
        {
            return false;
        }

        if (!GameManager.Instance.TrySpendCoin(offeredBall.price))
        {
            return false;
        }

        if (!DeckManager.Instance.AddBallToDeck(offeredBall))
        {
            GameManager.Instance.AddCoin(offeredBall.price);
            return false;
        }

        offerUI.MarkPurchased();
        return true;
    }

    public bool TryDeleteOwnedBall(ShopOwnedBallEntryUI entryUI, BallData ownedBall)
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
        return true;
    }

    private void RegisterListeners()
    {
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.DeckChanged -= OnDeckChanged;
            DeckManager.Instance.DeckChanged += OnDeckChanged;
        }
    }

    private void UnregisterListeners()
    {
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.DeckChanged -= OnDeckChanged;
        }
    }

    private void OnDeckChanged()
    {
        RefreshOwnedDeckDisplay();
    }

    private void ClearBallOffer()
    {
        currentBallOfferUIs.Clear();

        if (ballOfferParent == null)
        {
            return;
        }

        for (int i = ballOfferParent.childCount - 1; i >= 0; i--)
        {
            Destroy(ballOfferParent.GetChild(i).gameObject);
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

    private void Shuffle(List<BallData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            BallData temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
