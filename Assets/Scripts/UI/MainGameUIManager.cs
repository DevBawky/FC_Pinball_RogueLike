using UnityEngine;

public class MainGameUIManager : MonoBehaviour
{
    public static MainGameUIManager Instance;

    [Header("References")]
    public Canvas mainCanvas;
    public GameObject flyingScorePrefab;   
    public GameObject FloatingPanel;
    [SerializeField] private int initialFlyingScorePoolSize = 20;

    [Header("Targets (어디로 날아갈 것인가)")]
    public RectTransform chipsTarget; // 좌측 상단 붉은색 패널 중심
    public RectTransform multTarget;  // 좌측 상단 푸른색 패널 중심

    [HideInInspector]
    public int activeScoreParticles = 0; // 현재 날아가고 있는 파티클 개수

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        GameObjectPoolManager.Prewarm(flyingScorePrefab, initialFlyingScorePoolSize);
    }

    public void SpawnFlyingScore(Vector3 worldHitPosition, ScoreType type, float value)
    {
        Transform targetPanel = (type == ScoreType.Chips) ? chipsTarget : multTarget;
        if (flyingScorePrefab == null || mainCanvas == null || targetPanel == null)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(type, value);
            }

            return;
        }

        activeScoreParticles++; 

        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldHitPosition);
        GameObject flyingObj = GameObjectPoolManager.Spawn(flyingScorePrefab, screenPosition, Quaternion.identity, mainCanvas.transform);
        if (flyingObj == null)
        {
            activeScoreParticles = Mathf.Max(0, activeScoreParticles - 1);

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(type, value);
            }

            return;
        }

        FlyingScoreUI flyingUI = flyingObj.GetComponent<FlyingScoreUI>();
        if (flyingUI != null)
        {
            // 타입에 따라 타겟을 다르게 넘겨줍니다.
            flyingUI.Initialize(targetPanel, type, value);
        }
        else
        {
            activeScoreParticles = Mathf.Max(0, activeScoreParticles - 1);

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(type, value);
            }

            GameObjectPoolManager.Release(flyingObj);
        }
    }
}
