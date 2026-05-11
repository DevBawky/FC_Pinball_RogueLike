using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUpgradePanelUI : MonoBehaviour
{
    [SerializeField] private ShopUpgradeType upgradeType;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TMP_Text purchaseButtonText;

    void Awake()
    {
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(OnClickPurchaseButton);
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

    public void Refresh()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text = GetDefaultTitle(upgradeType);
        }

        if (valueText != null)
        {
            valueText.text = GameManager.Instance.GetUpgradeDisplayText(upgradeType);
        }

        if (levelText != null)
        {
            levelText.text = GameManager.Instance.GetUpgradeLevel(upgradeType).ToString();
        }

        int upgradeCost = GameManager.Instance.GetUpgradeCost(upgradeType);
        bool canUpgrade = GameManager.Instance.CanUpgrade(upgradeType);
        bool canAfford = canUpgrade && GameManager.Instance.currentCoin >= upgradeCost;

        if (costText != null)
        {
            costText.text = canUpgrade ? upgradeCost.ToString() : "MAX";
        }

        if (purchaseButton != null)
        {
            purchaseButton.interactable = canAfford;
        }

        if (purchaseButtonText != null)
        {
            if (!canUpgrade)
            {
                purchaseButtonText.text = "MAX";
            }
            else if (canAfford)
            {
                purchaseButtonText.text = "UPGRADE";
            }
            else
            {
                purchaseButtonText.text = "NEED MORE";
            }
        }
    }

    private void OnClickPurchaseButton()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (GameManager.Instance.TryPurchaseUpgrade(upgradeType))
        {
            Refresh();
        }
    }

    private void RegisterListeners()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CoinChanged -= OnCoinChanged;
            GameManager.Instance.CoinChanged += OnCoinChanged;
            GameManager.Instance.ShopUpgradeChanged -= OnUpgradeChanged;
            GameManager.Instance.ShopUpgradeChanged += OnUpgradeChanged;
        }
    }

    private void UnregisterListeners()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CoinChanged -= OnCoinChanged;
            GameManager.Instance.ShopUpgradeChanged -= OnUpgradeChanged;
        }
    }

    private void OnCoinChanged(int _)
    {
        Refresh();
    }

    private void OnUpgradeChanged(ShopUpgradeType _)
    {
        Refresh();
    }

    private string GetDefaultTitle(ShopUpgradeType type)
    {
        switch (type)
        {
            case ShopUpgradeType.BallMaxHealthRatio:
                return "Ball Max Health";
            case ShopUpgradeType.WallCollisionDamageReduction:
                return "Wall Damage Reduction";
            case ShopUpgradeType.ScoreGain:
                return "Score Gain";
            default:
                return type.ToString();
        }
    }
}
