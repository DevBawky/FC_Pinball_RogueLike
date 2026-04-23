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
    Coroutine _progressCoroutine;

    void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _gameStartButton.onClick.AddListener(onStartButtonPressed);
    }

    void onStartButtonPressed()
    {
        _mainmenuPanel.SetActive(false);
        _EnemySelectPanel.SetActive(true);
        ResetProgress();
        GenerateStageSelectPanels();
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
                StageType t = GetRandomStageType();
                ssp.Initialize(t);
            }
        }

        UpdateProgressText(_stagesCompleted / (float)_stagesBeforeBoss, _stagesCompleted);
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
        // Called when a stage is chosen. Increment progress towards boss.
        if (_stagesCompleted >= _stagesBeforeBoss) return;

        _stagesCompleted = Mathf.Clamp(_stagesCompleted + 1, 0, _stagesBeforeBoss);
        float from = _progressFillImage != null ? _progressFillImage.fillAmount : 0f;
        float to = Mathf.Clamp01(_stagesCompleted / (float)_stagesBeforeBoss);

        if (_progressCoroutine != null) StopCoroutine(_progressCoroutine);
        _progressCoroutine = StartCoroutine(AnimateProgressFill(from, to));

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

    void UpdateProgressText(float fillAmount, int completed)
    {
        if (_progressPercentageText == null) return;
        int percent = Mathf.RoundToInt(fillAmount * 100f);
        _progressPercentageText.text = percent + "% (" + completed + "/" + _stagesBeforeBoss + ")";
    }

    IEnumerator AnimateProgressFill(float from, float to)
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

        _EnemySelectPanel.GetComponent<Animator>().Play("GoMainGame");

        yield return new WaitForSeconds(0.55f);
        _EnemySelectPanel.SetActive(false);
        _MainGamePanel.SetActive(true);
        _stageMap.SetActive(true);
    }

    public void BeatEnemy(){
        StartCoroutine(End());
    }

    IEnumerator End()
    {
        _MainGamePanel.GetComponent<Animator>().Play("Battle_BeatEnemy");
        yield return new WaitForSeconds(2.2f);
        EnemyManager.Instance.FadeMap();
    }
}
