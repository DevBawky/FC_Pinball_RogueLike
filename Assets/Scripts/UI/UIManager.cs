using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("References")]
    public Canvas mainCanvas;               // UI가 생성될 부모 캔버스 (Screen Space - Overlay 권장)
    public GameObject flyingDamagePrefab;   // 날아갈 UI 프리팹
    public RectTransform healthBarTarget;   // UI가 날아갈 목적지 위치 (체력바의 아이콘 부분 등)
    public HealthBarUI healthBarScript;     // 체력 깎기 함수를 호출할 스크립트

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 범퍼나 공 오브젝트에서 충돌이 일어날 때 이 함수를 호출합니다.
    public void SpawnFlyingDamage(Vector3 worldHitPosition, float damage)
    {
        // 1. 카메라를 이용해 게임 월드의 좌표(충돌 위치)를 화면 UI 좌표(스크린 픽셀)로 변환
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldHitPosition);

        // 2. 캔버스 하위에 투사체 UI 프리팹 생성
        GameObject flyingObj = Instantiate(flyingDamagePrefab, mainCanvas.transform);

        // 3. UI의 시작 위치를 충돌한 스크린 위치로 세팅
        flyingObj.transform.position = screenPosition;

        // 4. 타겟 정보 및 데미지 전달하여 비행 시작
        DamageUI flyingUI = flyingObj.GetComponent<DamageUI>();
        if (flyingUI != null)
        {
            flyingUI.Initialize(healthBarTarget, healthBarScript, damage);
        }
    }
}