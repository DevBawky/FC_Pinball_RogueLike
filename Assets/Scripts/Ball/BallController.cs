using UnityEngine;

[RequireComponent(typeof(BallMovement))]
[RequireComponent(typeof(BallHealth))]
[RequireComponent(typeof(SpriteRenderer))]
public class BallController : MonoBehaviour
{
    [Header("장착된 공 데이터")]
    public BallData ballData;

    [Header("현재 인게임 스탯 (Balatro Style)")]
    public ScoreType currentScoreType; // 에러 방지를 위해 BallData.ScoreType으로 명시
    public float currentScoreValue; 

    private BallMovement movement;
    private BallHealth health;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        movement = GetComponent<BallMovement>();
        health = GetComponent<BallHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // 테스트용으로 인스펙터에 데이터가 있을 경우 기본 방향(1,1)으로 실행되게 유지합니다.
        if (ballData != null && movement.direction == Vector2.zero)
        {
            InitializeBall(ballData, new Vector2(1, 1));
        }
    }

    // 변경점: Vector2 initialDirection 매개변수가 추가되었습니다.
    public void InitializeBall(BallData data, Vector2 initialDirection)
    {
        this.ballData = data;

        if (data.ballSprite != null)
        {
            spriteRenderer.sprite = data.ballSprite;
        }

        // 이동 스탯 및 랜덤 방향 세팅
        movement.speed = data.baseSpeed;
        movement.direction = initialDirection; // 런처가 만들어준 랜덤 방향이 여기에 들어갑니다!

        health.maxHealth = data.maxHealth;
        health.ResetHealth(); 
        
        currentScoreType = data.scoreType;
        currentScoreValue = data.scoreValue; 
    }

    public void OnHitObject(Vector3 hitPosition)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SpawnFlyingScore(hitPosition, currentScoreType, currentScoreValue);
        }
    }
}