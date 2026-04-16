using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("References")]
    public Canvas mainCanvas;
    public GameObject flyingScorePrefab;   

    [Header("Targets (어디로 날아갈 것인가)")]
    public RectTransform chipsTarget; // 좌측 상단 붉은색 패널 중심
    public RectTransform multTarget;  // 좌측 상단 푸른색 패널 중심

    [HideInInspector]
    public int activeScoreParticles = 0; // 현재 날아가고 있는 파티클 개수

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void SpawnFlyingScore(Vector3 worldHitPosition, ScoreType type, float value)
    {
        activeScoreParticles++; 

        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldHitPosition);
        GameObject flyingObj = Instantiate(flyingScorePrefab, mainCanvas.transform);
        flyingObj.transform.position = screenPosition;

        FlyingScoreUI flyingUI = flyingObj.GetComponent<FlyingScoreUI>();
        if (flyingUI != null)
        {
            // 타입에 따라 타겟을 다르게 넘겨줍니다.
            Transform targetPanel = (type == ScoreType.Chips) ? chipsTarget : multTarget;
            flyingUI.Initialize(targetPanel, type, value);
        }
    }
}