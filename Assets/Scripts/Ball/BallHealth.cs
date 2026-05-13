using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class BallHealth : MonoBehaviour
{
    [Header("Health Settings (Durability)")]
    public float maxHealth = 15;
    private float currentHealth;

    [Header("Damage Settings")]
    public float damagePerBounce = 1f;

    [Header("Effect Settings")]
    [SerializeField] private GameObject boomEffectPrefab;
    [SerializeField] private GameObject sparkEffectPrefab;
    [SerializeField] private float effectDuration = 0.2f;
    [SerializeField] private int initialEffectPoolSize = 6;

    [Header("Events")]
    public UnityEvent<float, float> onHealthChanged;
    public UnityEvent onTakeDamage;
    public UnityEvent onDeath;

    int targetLayerIndex, wallLayerIndex;
    private BallController ballController;
    private bool boomEffectPlayed;

    void Start()
    {
        currentHealth = maxHealth;
        ballController = GetComponent<BallController>();
        EffectPoolManager.Prewarm(boomEffectPrefab, initialEffectPoolSize);
        EffectPoolManager.Prewarm(sparkEffectPrefab, initialEffectPoolSize);

        // 게임 시작 시 "Object" 문자열에 해당하는 레이어 번호를 미리 찾아 캐싱해 둡니다.
        targetLayerIndex = LayerMask.NameToLayer("Object");
        wallLayerIndex = LayerMask.NameToLayer("Wall");
        
        if (targetLayerIndex == -1)
        {
            Debug.LogWarning("주의: 'Object'라는 이름의 레이어가 존재하지 않습니다. 유니티 에디터 우측 상단 Layer에서 추가해주세요.");
        }
        
        // 시작할 때 UI 등에 초기 체력 상태를 전달
        onHealthChanged.Invoke(currentHealth, maxHealth);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Vector3 hitPoint = collision.contactCount > 0
            ? collision.GetContact(0).point
            : collision.transform.position;

        bool isObjectCollision = collision.gameObject.layer == targetLayerIndex;
        bool isWallCollision = collision.gameObject.layer == wallLayerIndex;

        if (!isWallCollision)
        {
            EffectPoolManager.Play(sparkEffectPrefab, hitPoint, effectDuration, GetEffectColor());
        }

        if (isObjectCollision)
        {
            TakeDamage(damagePerBounce);
            ballController?.TryTriggerSpecialAbility(BallSpecialAbilityCollisionType.Object, hitPoint, collision.gameObject);
        }
        else if(isWallCollision)
        {
            float wallDamage = damagePerBounce / 2f;
            if (GameManager.Instance != null)
            {
                wallDamage = GameManager.Instance.ApplyWallCollisionDamageReduction(wallDamage);
            }

            TakeDamage(wallDamage);
            ballController?.TryTriggerSpecialAbility(BallSpecialAbilityCollisionType.Wall, hitPoint, collision.gameObject);
        }
    }

    // 외부(러시안 룰렛, 가시 함정 등)에서 강제로 큰 데미지를 줄 때도 사용할 수 있도록 public으로 열어둡니다.
    public void TakeDamage(float damage)
    {
        // 이미 파괴 처리 중이라면 중복 실행 방지
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // 체력이 음수가 되지 않도록 방지

        // 이벤트 호출 (UI 갱신 및 피격 이펙트)
        onHealthChanged.Invoke(currentHealth, maxHealth);
        onTakeDamage.Invoke();

        // 체력이 0이 되었을 때의 처리
        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void ResetHealth()
    {
        boomEffectPlayed = false;
        currentHealth = maxHealth;
        onHealthChanged.Invoke(currentHealth, maxHealth);
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void SetHealth(float newCurrentHealth, float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Clamp(newCurrentHealth, 0f, maxHealth);
        onHealthChanged.Invoke(currentHealth, maxHealth);
    }

    // 힐링 범퍼나 특수 아이템을 먹었을 때 체력을 회복하는 기능
    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // 최대 체력 초과 방지

        onHealthChanged.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        // 파괴 이벤트 호출 (폭발 파티클 재생, 게임 오버/턴 종료 매니저 호출 등)
        onDeath.Invoke();
        PlayBoomEffect();

        // 공 오브젝트 파괴
        GameObjectPoolManager.Release(gameObject);
    }

    public void Kill()
    {
        if (currentHealth <= 0f)
        {
            return;
        }

        currentHealth = 0f;
        onHealthChanged.Invoke(currentHealth, maxHealth);
        Die();
    }

    private void OnDestroy()
    {
        PlayBoomEffect();
    }

    private void PlayBoomEffect()
    {
        if (!Application.isPlaying || boomEffectPlayed)
        {
            return;
        }

        boomEffectPlayed = true;
        EffectPoolManager.Play(boomEffectPrefab, transform.position, effectDuration, GetEffectColor());
    }

    private Color GetEffectColor()
    {
        return ballController != null && ballController.ballData != null
            ? ballController.ballData.effectColor
            : Color.white;
    }
}
