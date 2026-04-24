using System;
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
    private int specialAbilityTriggerCount;
    private bool canUseSpecialAbility = true;

    public event Action<BallSpecialAbilityTriggerContext> SpecialAbilityTriggered;

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
        specialAbilityTriggerCount = 0;
        canUseSpecialAbility = true;

        if (data.ballSprite != null)
        {
            spriteRenderer.sprite = data.ballSprite;
        }

        // 이동 스탯 및 랜덤 방향 세팅
        movement.speed = data.baseSpeed;
        movement.direction = initialDirection; // 런처가 만들어준 랜덤 방향이 여기에 들어갑니다!

        float upgradedMaxHealth = data.maxHealth;
        if (GameManager.Instance != null)
        {
            upgradedMaxHealth = GameManager.Instance.GetUpgradedBallMaxHealth(data.maxHealth);
        }

        health.maxHealth = upgradedMaxHealth;
        health.ResetHealth(); 
        
        currentScoreType = data.scoreType;
        currentScoreValue = data.scoreValue;
        if (GameManager.Instance != null)
        {
            currentScoreValue = GameManager.Instance.GetUpgradedScoreValue(data.scoreType, data.scoreValue);
        }
    }

    public void OnHitObject(Vector3 hitPosition)
    {
        if (MainGameUIManager.Instance != null)
        {
            MainGameUIManager.Instance.SpawnFlyingScore(hitPosition, currentScoreType, currentScoreValue);
        }
    }

    public bool TryTriggerSpecialAbility(
        BallSpecialAbilityCollisionType collisionType,
        Vector3 hitPosition,
        GameObject hitObject = null)
    {
        if (!canUseSpecialAbility || ballData == null || !ballData.CanTriggerSpecialAbility(collisionType, specialAbilityTriggerCount))
        {
            return false;
        }

        specialAbilityTriggerCount++;

        int remainingTriggerCount = -1;
        if (ballData.specialAbility.HasLimitedTriggerCount)
        {
            remainingTriggerCount = Mathf.Max(ballData.specialAbility.maxTriggerCount - specialAbilityTriggerCount, 0);
        }

        BallSpecialAbilityTriggerContext context = new BallSpecialAbilityTriggerContext(
            this,
            ballData,
            collisionType,
            hitPosition,
            hitObject,
            specialAbilityTriggerCount,
            remainingTriggerCount);

        SpecialAbilityTriggered?.Invoke(context);
        ballData.specialAbility.abilityAsset.Activate(context);

        Debug.Log(
            $"[Ball Special Ability] '{ballData.ballName}' triggered '{ballData.specialAbility.abilityName}' on {collisionType} hit ({specialAbilityTriggerCount}/{GetMaxTriggerLabel()}).");

        return true;
    }

    private string GetMaxTriggerLabel()
    {
        if (ballData == null || !ballData.HasSpecialAbility || !ballData.specialAbility.HasLimitedTriggerCount)
        {
            return "INF";
        }

        return ballData.specialAbility.maxTriggerCount.ToString();
    }

    public Vector2 GetCurrentDirection()
    {
        return movement != null ? movement.GetCurrentDirection() : Vector2.up;
    }

    public void SetTravelDirection(Vector2 newDirection)
    {
        if (movement != null)
        {
            movement.SetDirection(newDirection);
        }
    }

    public BallController SpawnDuplicateBall(Vector2 initialDirection, float spawnOffset = 0f)
    {
        if (ballData == null)
        {
            return null;
        }

        Vector3 spawnPosition = transform.position;
        if (initialDirection != Vector2.zero && spawnOffset > 0f)
        {
            spawnPosition += (Vector3)(initialDirection.normalized * spawnOffset);
        }

        GameObject clone = Instantiate(gameObject, spawnPosition, transform.rotation);
        BallController cloneController = clone.GetComponent<BallController>();

        if (cloneController != null)
        {
            cloneController.InitializeBall(ballData, initialDirection);
            cloneController.CopyRuntimeStatsFrom(this, initialDirection);
            cloneController.DisableSpecialAbility();
        }

        return cloneController;
    }

    public void CopyRuntimeStatsFrom(BallController source, Vector2 initialDirection)
    {
        if (source == null)
        {
            return;
        }

        currentScoreType = source.currentScoreType;
        currentScoreValue = source.currentScoreValue;

        if (movement != null && source.movement != null)
        {
            movement.speed = source.movement.speed;
            movement.SetDirection(initialDirection);
        }

        if (health != null && source.health != null)
        {
            health.damagePerBounce = source.health.damagePerBounce;
            health.SetHealth(source.health.GetCurrentHealth(), source.health.maxHealth);
        }
    }

    public void DisableSpecialAbility()
    {
        canUseSpecialAbility = false;
    }
}
