using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum StageType
{
    Battle,
    Treasure,
    Event,
    Shop,
    BossBattle
}

[System.Serializable]
public class Stage
{
    public StageType stageType;
    public string stageDescription;

    public Stage(StageType type, string description)
    {
        stageType = type;
        stageDescription = description;
    }
}

public class StageSelectPrefab : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Image _panelImage;
    [SerializeField] TMP_Text _roomTypeText;
    [SerializeField] TMP_Text _descriptionText;
    [SerializeField] Button _goStageButton;

    [Header("Type Sprites")]
    [SerializeField] Sprite _normalSprite;
    [SerializeField] Sprite _treasureSprite;
    [SerializeField] Sprite _eventSprite;
    [SerializeField] Sprite _shopSprite;

    [SerializeField] Sprite _bossSprite;

    public StageType CurrentStageType { get; private set; }
    public string StageDescription { get; private set; }

    public void Initialize(StageType type, string description = null)
    {
        CurrentStageType = type;
        if (_roomTypeText != null)
            _roomTypeText.text = type.ToString();

        StageDescription = string.IsNullOrEmpty(description) ? GetDefaultDescription(type) : description;

        if (_goStageButton != null)
        {
            _goStageButton.onClick.RemoveListener(onSelectStage);
            _goStageButton.onClick.AddListener(onSelectStage);
            _goStageButton.interactable = true;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (_descriptionText != null)
            _descriptionText.text = string.IsNullOrEmpty(StageDescription) ? CurrentStageType.ToString() : StageDescription;

        if (_panelImage != null)
        {
            Sprite s = GetSpriteForType(CurrentStageType);
            if (s != null)
                _panelImage.sprite = s;
        }
    }

    Sprite GetSpriteForType(StageType type)
    {
        switch (type)
        {
            case StageType.Battle: return _normalSprite;
            case StageType.Treasure: return _treasureSprite;
            case StageType.Event: return _eventSprite;
            case StageType.Shop: return _shopSprite;
            case StageType.BossBattle   : return _bossSprite;
            default: return null;
        }
    }

    string GetDefaultDescription(StageType type)
    {
        switch (type)
        {
            case StageType.Battle: return "Defeat enemies and earn new rewards!";
            case StageType.Treasure: return "Congratulations! If you're lucky, you can earn a top-tier reward!";
            case StageType.Event: return "What will happen? Nobody knows!";
            case StageType.Shop: return "Shop";
            case StageType.BossBattle: return "Boss Room";
            default: return "";
        }
    }

    void onSelectStage(){
        if (_goStageButton != null)
        {
            _goStageButton.onClick.RemoveListener(onSelectStage);
            _goStageButton.interactable = false;
        }

        if (MainMenuUIManager.Instance != null)
        {
            MainMenuUIManager.Instance.OnStageSelected(CurrentStageType);
        }
    }
}
