using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUIManager : MonoBehaviour
{
    public static MainMenuUIManager Instance {get; private set;}

    [SerializeField] GameObject _mainmenuPanel;
    [SerializeField] GameObject _EnemySelectPanel;
    [Header("Main Game")]
    [SerializeField] GameObject _MainGamePanel;
    [SerializeField] GameObject _stageMap;

    [Header("Button")]
    [SerializeField] Button _gameStartButton;
    [Header("Stage Selection")]
    [SerializeField] Transform _stageLayout;
    [SerializeField] GameObject _stageSelectPrefab;
    [SerializeField] int _stagesToGenerate = 3;

    [Header("Weights (relative)")]
    [SerializeField] float _normalWeight = 60f;
    [SerializeField] float _treasureWeight = 30f;
    [SerializeField] float _eventWeight = 10f;
    [SerializeField] float _shopWeight = 5f;

    [Header("Progress UI")]
    [SerializeField] Image _progressFillImage;
    [SerializeField] TMP_Text _progressPercentageText;
    [SerializeField] int _stagesBeforeBoss = 5;
    [SerializeField] float _progressLerpDuration = 0.5f;

    int _stagesCompleted = 0;
    int _selectedStageNumber = 1;
    StageType _selectedStageType = StageType.Battle;
    Coroutine _progressCoroutine;

    void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

// MainMenuUIManager.cs 내부의 Start 함수만 아래와 같이 수정해 주세요.

    void Start()
    {
        if (_mainmenuPanel != null) _mainmenuPanel.SetActive(true);
        if (_EnemySelectPanel != null) _EnemySelectPanel.SetActive(false);
        if (_MainGamePanel != null) _MainGamePanel.SetActive(false);

        _gameStartButton.onClick.AddListener(onStartButtonPressed);
    }

    void onStartButtonPressed()
    {
        _mainmenuPanel.SetActive(false);
        _EnemySelectPanel.SetActive(true);
        ResetProgress();
        GenerateStageSelectPanels();
        MainGameUIManager.Instance.FloatingPanel.SetActive(true);
        // GameStart 버튼을 누르면 게임 매니저에게도 스테이지 선택(턴 시작)을 알립니다.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToStageSelection();
        }
    }

    void GenerateStageSelectPanels()
    {
        if (_stageLayout == null || _stageSelectPrefab == null)
        {
            Debug.LogWarning("Stage layout or prefab is not assigned.");
            return;
        }

        for (int i = _stageLayout.childCount - 1; i >= 0; i--)
        {
            var child = _stageLayout.GetChild(i);
            Destroy(child.gameObject);
        }

        for (int i = 0; i < _stagesToGenerate; i++)
        {
            GameObject go = Instantiate(_stageSelectPrefab, _stageLayout);
            StageSelectPrefab ssp = go.GetComponent<StageSelectPrefab>();
            if (ssp != null)
            {
                StageType t = ShouldGenerateBossStages() ? StageType.BossBattle : GetRandomStageType();
                ssp.Initialize(t);
            }
        }

        UpdateProgressText(_stagesCompleted / (float)_stagesBeforeBoss, _stagesCompleted);
    }

    public void RefreshStageSelectionFromShop()
    {
        if (_EnemySelectPanel != null)
        {
            _EnemySelectPanel.SetActive(true);
        }

        if (_MainGamePanel != null)
        {
            _MainGamePanel.SetActive(false);
        }

        if (_stageMap != null)
        {
            _stageMap.SetActive(false);
        }

        GenerateStageSelectPanels();
    }

    StageType GetRandomStageType()
    {
        float total = _normalWeight + _treasureWeight + _eventWeight;
        if (total <= 0f) return StageType.Battle;
        float r = Random.Range(0f, total);
        if (r < _normalWeight) return StageType.Battle;
        if (r < _normalWeight + _treasureWeight) return StageType.Treasure;
        return StageType.Event;
    }

    public void OnStageSelected(StageType type)
    {
        _selectedStageType = type;
        _selectedStageNumber = Mathf.Max(1, _stagesCompleted + 1);

        if (type == StageType.BossBattle)
        {
            _selectedStageNumber = Mathf.Max(1, _stagesBeforeBoss + 1);
            if (_progressCoroutine != null) StopCoroutine(_progressCoroutine);
            _progressCoroutine = StartCoroutine(AnimateProgressFill(
                _progressFillImage != null ? _progressFillImage.fillAmount : 1f,
                _progressFillImage != null ? _progressFillImage.fillAmount : 1f,
                true));
            return;
        }

        // Called when a stage is chosen. Increment progress towards boss.
        if (_stagesCompleted >= _stagesBeforeBoss) return;

        _stagesCompleted = Mathf.Clamp(_stagesCompleted + 1, 0, _stagesBeforeBoss);
        float from = _progressFillImage != null ? _progressFillImage.fillAmount : 0f;
        float to = Mathf.Clamp01(_stagesCompleted / (float)_stagesBeforeBoss);

        if (_progressCoroutine != null) StopCoroutine(_progressCoroutine);
        _progressCoroutine = StartCoroutine(AnimateProgressFill(from, to, false));

        UpdateProgressText(to, _stagesCompleted);

        if (_stagesCompleted >= _stagesBeforeBoss)
        {
            Debug.Log("Reached boss threshold. Next encounter should be Boss.");
        }
    }

    void ResetProgress()
    {
        _stagesCompleted = 0;
        if (_progressCoroutine != null) { StopCoroutine(_progressCoroutine); _progressCoroutine = null; }
        if (_progressFillImage != null) _progressFillImage.fillAmount = 0f;
        UpdateProgressText(0f, 0);
    }

    bool ShouldGenerateBossStages()
    {
        return _stagesBeforeBoss > 0 && _stagesCompleted >= _stagesBeforeBoss;
    }

    void UpdateProgressText(float fillAmount, int completed)
    {
        if (_progressPercentageText == null) return;
        int percent = Mathf.RoundToInt(fillAmount * 100f);
        _progressPercentageText.text = percent + "% (" + completed + "/" + _stagesBeforeBoss + ")";
    }

    IEnumerator AnimateProgressFill(float from, float to, bool resetProgressAfterStageStart)
    {
        if (_progressFillImage == null) yield break;
        float elapsed = 0f;
        while (elapsed < _progressLerpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _progressLerpDuration);
            _progressFillImage.fillAmount = Mathf.Lerp(from, to, t);
            yield return null;
        }
        _progressFillImage.fillAmount = to;
        _progressCoroutine = null;

        if (_EnemySelectPanel != null)
        {
            _EnemySelectPanel.GetComponent<Animator>().Play("GoMainGame");
            yield return new WaitForSeconds(0.55f);
            _EnemySelectPanel.SetActive(false);
        }

        // 맵(배경)은 UI 매니저가 켜줍니다.
        if (_stageMap != null) _stageMap.SetActive(true);

        StartBattleFromUI();

        if (resetProgressAfterStageStart)
        {
            ResetProgress();
        }
    }

    public void BeatEnemy()
    {
        StartCoroutine(End());
    }

    IEnumerator End()
    {
        if (_MainGamePanel != null)
        {
            _MainGamePanel.GetComponent<Animator>().Play("Battle_BeatEnemy");
        }
        
        yield return new WaitForSeconds(2.2f);
        
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.FadeMap();
        }

        // 맵 페이드 아웃 연출이 끝날 때까지 대기 (EnemyManager의 기본 fadeDuration이 1초입니다)
        yield return new WaitForSeconds(1.5f);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToPayout();
        }
    }

    // GameStart 버튼에서 호출: 실제 턴/배틀을 시작합니다.
    public void StartBattleFromUI()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartBattle(_selectedStageType, _selectedStageNumber);
        }
    }
}
