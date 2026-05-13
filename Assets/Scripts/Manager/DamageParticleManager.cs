using System.Collections;
using UnityEngine;

public class DamageParticleManager : MonoBehaviour
{
    public static DamageParticleManager Instance;

    [Header("References")]
    public GameObject damageParticlePrefab;
    public Canvas mainCanvas;
    public RectTransform startTarget;
    public RectTransform endTarget;

    [Header("Particle Settings")]
    public int maxParticles = 50;
    public float particleBaseDamage = 100f;
    public float particleSpeed = 2500f;
    public float spawnInterval = 0.05f;
    [SerializeField] private int initialParticlePoolSize = 50;

    private int activeParticles;
    private float remainderDamage;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        GameObjectPoolManager.Prewarm(damageParticlePrefab, initialParticlePoolSize);
    }

    public void FireDamageParticles(float totalDamage)
    {
        if (!CanPlayDamageParticles())
        {
            Debug.LogWarning("Damage particle references are missing. Applying damage directly.");
            ApplyDamageDirectly(totalDamage);
            return;
        }

        StartCoroutine(FireRoutine(totalDamage));
    }

    private bool CanPlayDamageParticles()
    {
        return damageParticlePrefab != null
            && mainCanvas != null
            && startTarget != null
            && endTarget != null;
    }

    private void ApplyDamageDirectly(float totalDamage)
    {
        activeParticles = 0;

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.TakeDamage(totalDamage);

            if (EnemyManager.Instance.CurrentHealth <= 0f)
            {
                return;
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PrepareNextAttack();
        }
    }

    private IEnumerator FireRoutine(float totalDamage)
    {
        if (totalDamage <= 0f)
        {
            activeParticles = 0;

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ReduceTotalDamageText(0f, 0f, 0f);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.PrepareNextAttack();
            }

            yield break;
        }

        int particleCount = Mathf.FloorToInt(totalDamage / particleBaseDamage);
        float actualDamagePerParticle = particleBaseDamage;

        if (particleCount > maxParticles)
        {
            particleCount = maxParticles;
            actualDamagePerParticle = totalDamage / maxParticles;
        }

        if (particleCount == 0 && totalDamage > 0)
        {
            particleCount = 1;
            actualDamagePerParticle = totalDamage;
        }

        remainderDamage = totalDamage - particleCount * actualDamagePerParticle;
        activeParticles = particleCount;

        if (particleCount <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PrepareNextAttack();
            }

            yield break;
        }

        float currentDisplayDamage = totalDamage;

        for (int i = 0; i < particleCount; i++)
        {
            float damageToDeal = actualDamagePerParticle;
            if (i == particleCount - 1)
            {
                damageToDeal += remainderDamage;
            }

            SpawnParticle(damageToDeal);

            if (ScoreManager.Instance != null)
            {
                float nextDisplayDamage = currentDisplayDamage - damageToDeal;
                ScoreManager.Instance.ReduceTotalDamageText(currentDisplayDamage, nextDisplayDamage, spawnInterval);
                currentDisplayDamage = nextDisplayDamage;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnParticle(float damageAmount)
    {
        GameObject particleObject = GameObjectPoolManager.Spawn(
            damageParticlePrefab,
            startTarget.position,
            Quaternion.identity,
            mainCanvas.transform);

        if (particleObject == null)
        {
            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.TakeDamage(damageAmount);
            }

            activeParticles = Mathf.Max(0, activeParticles - 1);
            if (activeParticles <= 0)
            {
                CompleteDamageSequence();
            }

            return;
        }

        particleObject.transform.position = startTarget.position;
        StartCoroutine(MoveParticleRoutine(particleObject, damageAmount));
    }

    private IEnumerator MoveParticleRoutine(GameObject particleObject, float damageAmount)
    {
        Vector3 startPos = particleObject.transform.position;
        Vector3 targetPos = endTarget.position;
        Vector3 randomOffset = new Vector3(Random.Range(-200f, 200f), Random.Range(-100f, 100f), 0f);

        float journeyTime = Vector3.Distance(startPos, targetPos) / particleSpeed;
        float timer = 0f;

        while (timer < journeyTime && particleObject != null)
        {
            timer += Time.deltaTime;
            float percent = journeyTime > 0f ? timer / journeyTime : 1f;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, percent);
            currentPos += randomOffset * Mathf.Sin(percent * Mathf.PI);

            particleObject.transform.position = currentPos;
            yield return null;
        }

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.TakeDamage(damageAmount);
        }

        GameObjectPoolManager.Release(particleObject);
        activeParticles--;

        if (activeParticles <= 0)
        {
            CompleteDamageSequence();
        }
    }

    private void CompleteDamageSequence()
    {
        if (EnemyManager.Instance != null && EnemyManager.Instance.CurrentHealth <= 0f)
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PrepareNextAttack();
        }
    }
}
