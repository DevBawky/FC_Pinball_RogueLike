using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;

[System.Serializable]
public class StageEnemyPool
{
    [Min(1)] public int stageNumber = 1;
    public EnemyData[] enemyPool;
    public EnemyData bossEnemyData;
}

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Enemy Data")]
    [SerializeField] private EnemyData currentEnemyData;
    [SerializeField] private EnemyData[] enemyPool;
    [SerializeField] private StageEnemyPool[] stageEnemyPools;
    [SerializeField] private EnemyData bossEnemyData;
    [SerializeField] private Image enemySpriteRenderer;

    public EnemyData CurrentEnemyData => currentEnemyData;

    [Header("Enemy Stats")]
    public float maxHealth = 5000f;
    public float CurrentHealth => currentHealth;
    private float currentHealth;        // 논리적인 실제 체력
    private float displayHealth;        // UI에 보여주기 위해 서서히 줄어드는 가짜 체력

    [Header("UI References (Health)")]
    public Image healthBarFill;       
    public TMP_Text healthText; 
    public float healthLerpSpeed = 10f; // 체력바가 줄어드는 속도

    [Header("UI References (Damage Text)")]
    public TMP_Text accumulatedDamageText; // 적 머리 위에 띄울 누적 대미지 텍스트
    public float damageTextPunchScale = 1.5f;     // 대미지 누적 시 커질 스케일 배수
    public float damageTextResetTime = 1.5f;      // 타격이 끝난 후 텍스트가 사라지는 시간

    private Coroutine punchCoroutine;
    private bool isDead = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (accumulatedDamageText != null)
        {
            accumulatedDamageText.gameObject.SetActive(true);
        }

        InitializeBattleEnemy();
    }

    [Header("Fade On Death")]
    [Tooltip("Root transform whose children will be faded out on death. If null, this GameObject's transform will be used.")]
    public Transform fadeRoot;
    [Tooltip("Duration in seconds for fading renderers to alpha=0.")]
    public float fadeDurationOnDeath = 1f;

    void Update()
    {
        // 보여지는 체력이 실제 체력과 다르다면 서서히(Lerp) 깎아줍니다.
        if (Mathf.Abs(displayHealth - currentHealth) > 0.5f)
        {
            displayHealth = Mathf.Lerp(displayHealth, currentHealth, Time.deltaTime * healthLerpSpeed);
            
            // Lerp되는 동안 체력바와 텍스트를 실시간으로 업데이트
            if (healthBarFill != null)
                healthBarFill.fillAmount = displayHealth / maxHealth;
                
            if (healthText != null)
                healthText.text = $"{Mathf.RoundToInt(displayHealth)} / {maxHealth}";

            UpdateAccumulatedDamageText();
        }
    }

    // 파티클이 도착할 때마다 이 함수가 호출됩니다.
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // 1. 실제 체력 감소 (UI는 Update에서 알아서 서서히 줄어듭니다)
        currentHealth -= damage;
        if (currentHealth <= 0) currentHealth = 0;

        // 2. 누적 대미지 연산 및 표시
        ShowAccumulatedDamage(damage);

        // 3. 사망 판정
        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            UpdateAccumulatedDamageText();
            Die();
        }
    }

    private void ShowAccumulatedDamage(float damage)
    {
        if (accumulatedDamageText == null) return;

        // 대미지 누적
        
        // 텍스트 활성화
        accumulatedDamageText.gameObject.SetActive(true);

        if (currentHealth <= 0)
        {
            accumulatedDamageText.color = Color.red; 
        }
        else
        {
            // 아직 살아있다면 정상적으로 누적 대미지 출력
            accumulatedDamageText.color = Color.white;
        }

        UpdateAccumulatedDamageText();

        if (punchCoroutine != null) StopCoroutine(punchCoroutine);
        
        // 막타일 때는 스케일을 조금 더 크게 줘도 맛있습니다.
        float currentPunchScale = (currentHealth <= 0) ? damageTextPunchScale * 1.5f : damageTextPunchScale;
        punchCoroutine = StartCoroutine(PunchScaleRoutine(currentPunchScale));

        // 일정 시간 뒤에 텍스트를 숨기는 코루틴 실행
    }

    private IEnumerator PunchScaleRoutine(float targetScale)
    {
        Transform textTransform = accumulatedDamageText.transform;
        
        // 확 커짐
        textTransform.localScale = Vector3.one * targetScale;
        
        // 0.15초 동안 원래 크기(Vector3.one)로 부드럽게 복귀
        float timer = 0f;
        while (timer < 0.15f)
        {
            timer += Time.deltaTime;
            textTransform.localScale = Vector3.Lerp(Vector3.one * targetScale, Vector3.one, timer / 0.15f);
            yield return null;
        }
        
        textTransform.localScale = Vector3.one;
    }

    // 파티클 공격이 모두 끝나고 나면 누적 대미지를 초기화하고 숨김
    // 초기 시작 시 UI를 즉시 맞추기 위한 헬퍼 함수
    private void UpdateHealthUIInstantly()
    {
        if (healthBarFill != null) healthBarFill.fillAmount = currentHealth / maxHealth;
        if (healthText != null) healthText.text = $"{Mathf.RoundToInt(currentHealth)} / {maxHealth}";
        UpdateAccumulatedDamageText();
    }

    private void UpdateAccumulatedDamageText()
    {
        if (accumulatedDamageText == null) return;

        accumulatedDamageText.gameObject.SetActive(true);

        string healthValue = $"{Mathf.RoundToInt(currentHealth)} / {Mathf.RoundToInt(maxHealth)}";
        if (isDead)
        {
            accumulatedDamageText.color = Color.red;
        }
        else
        {
            accumulatedDamageText.color = Color.white;
        }

        accumulatedDamageText.text = healthValue;
    }

    public void InitializeBattleEnemy(bool isBossBattle = false)
    {
        InitializeBattleEnemy(isBossBattle, 1);
    }

    public void InitializeBattleEnemy(bool isBossBattle, int stageNumber)
    {
        EnemyData selectedEnemyData = isBossBattle ? GetBossEnemyData(stageNumber) : GetRandomEnemyData(stageNumber);
        ApplyEnemyData(selectedEnemyData);
    }

    private EnemyData GetRandomEnemyData(int stageNumber)
    {
        StageEnemyPool stageEnemyPool = GetStageEnemyPool(stageNumber);
        EnemyData selectedEnemy = GetRandomEnemyFromPool(stageEnemyPool != null ? stageEnemyPool.enemyPool : null);
        if (selectedEnemy != null)
        {
            return selectedEnemy;
        }

        selectedEnemy = GetRandomEnemyFromPool(enemyPool);
        return selectedEnemy != null ? selectedEnemy : currentEnemyData;
    }

    private EnemyData GetRandomEnemyFromPool(EnemyData[] pool)
    {
        if (pool == null || pool.Length == 0)
        {
            return null;
        }

        int validCount = 0;
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] != null)
            {
                validCount++;
            }
        }

        if (validCount <= 0)
        {
            return null;
        }

        int selectedIndex = Random.Range(0, validCount);
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] == null)
            {
                continue;
            }

            if (selectedIndex == 0)
            {
                return pool[i];
            }

            selectedIndex--;
        }

        return null;
    }

    private EnemyData GetBossEnemyData(int stageNumber)
    {
        StageEnemyPool stageEnemyPool = GetStageEnemyPool(stageNumber);
        if (stageEnemyPool != null && stageEnemyPool.bossEnemyData != null)
        {
            return stageEnemyPool.bossEnemyData;
        }

        if (bossEnemyData != null)
        {
            return bossEnemyData;
        }

        Debug.LogWarning("Boss enemy data is not assigned. Falling back to a normal enemy.");
        return GetRandomEnemyData(stageNumber);
    }

    private StageEnemyPool GetStageEnemyPool(int stageNumber)
    {
        if (stageEnemyPools == null || stageEnemyPools.Length == 0)
        {
            return null;
        }

        int clampedStageNumber = Mathf.Max(1, stageNumber);
        for (int i = 0; i < stageEnemyPools.Length; i++)
        {
            StageEnemyPool stageEnemyPool = stageEnemyPools[i];
            if (stageEnemyPool != null && stageEnemyPool.stageNumber == clampedStageNumber)
            {
                return stageEnemyPool;
            }
        }

        return null;
    }

    public void ApplyEnemyData(EnemyData enemyData)
    {
        if (enemyData != null)
        {
            currentEnemyData = enemyData;
            maxHealth = enemyData.maxHealth;

            if (enemySpriteRenderer != null)
            {
                enemySpriteRenderer.sprite = enemyData.enemySprite;
                enemySpriteRenderer.enabled = enemyData.enemySprite != null;
            }
        }

        currentHealth = maxHealth;
        displayHealth = maxHealth;
        isDead = false;

        if (accumulatedDamageText != null)
        {
            accumulatedDamageText.gameObject.SetActive(true);
            accumulatedDamageText.color = Color.white;
            accumulatedDamageText.transform.localScale = Vector3.one;
        }

        UpdateHealthUIInstantly();
    }

    private void Die()
    {
        Debug.Log("현상금 수배범 처치 완료! 다음 스테이지로!");
        MainMenuUIManager.Instance.BeatEnemy();
    }

    public void FadeMap() => StartCoroutine(FadeChildrenToTransparent(fadeRoot, fadeDurationOnDeath));

    public void RestoreStageVisuals()
    {
        RestoreTilemaps();
        RestoreObjectLayerSpriteRenderers();
    }

    private IEnumerator FadeChildrenToTransparent(Transform root, float duration)
    {
        if (root == null) yield break;

        // Gather renderers/components to fade
        SpriteRenderer[] spriteRenderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        Tilemap[] tilemaps = root.GetComponentsInChildren<Tilemap>(true);

        Color[] spriteOriginal = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++) spriteOriginal[i] = spriteRenderers[i].color;

        // Read original colors from Tilemap components
        Color[] tilemapOriginal = new Color[tilemaps.Length];
        for (int i = 0; i < tilemaps.Length; i++) tilemapOriginal[i] = tilemaps[i].color;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Lerp alpha for sprite renderers
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                var sr = spriteRenderers[i];
                if (sr == null) continue;
                Color c = spriteOriginal[i];
                c.a = Mathf.Lerp(spriteOriginal[i].a, 0f, t);
                sr.color = c;
            }

            // Lerp alpha for Tilemap components
            for (int i = 0; i < tilemaps.Length; i++)
            {
                var tm = tilemaps[i];
                if (tm == null) continue;
                Color orig = tilemapOriginal[i];
                Color c = orig;
                c.a = Mathf.Lerp(orig.a, 0f, t);
                tm.color = c;
            }

            yield return null;
        }

        // Ensure fully transparent at the end
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            if (sr == null) continue;
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        for (int i = 0; i < tilemaps.Length; i++)
        {
            var tm = tilemaps[i];
            if (tm == null) continue;
            Color final = tilemapOriginal[i];
            final.a = 0f;
            tm.color = final;
        }
    }

    private void RestoreTilemaps()
    {
        Transform root = fadeRoot != null ? fadeRoot : transform;
        Tilemap[] tilemaps = root.GetComponentsInChildren<Tilemap>(true);

        for (int i = 0; i < tilemaps.Length; i++)
        {
            if (tilemaps[i] == null) continue;

            Color color = tilemaps[i].color;
            color.a = 1f;
            tilemaps[i].color = color;
        }
    }

    private void RestoreObjectLayerSpriteRenderers()
    {
        int objectLayerIndex = LayerMask.NameToLayer("Object");
        if (objectLayerIndex == -1)
        {
            return;
        }

        SpriteRenderer[] spriteRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
            if (spriteRenderer == null) continue;
            if (spriteRenderer.gameObject.layer != objectLayerIndex) continue;

            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }
}
