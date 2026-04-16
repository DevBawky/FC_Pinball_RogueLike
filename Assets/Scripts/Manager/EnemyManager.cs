using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Enemy Stats")]
    public float maxHealth = 5000f; // 스테이지 보스의 체력
    private float currentHealth;

    [Header("UI References")]
    public Image healthBarFill;       // 체력바 (Filled 이미지)
    public TextMeshProUGUI healthText; // "5000 / 5000" 표시용 텍스트

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    // ScoreManager에서 최종 대미지가 확정되면 이 함수를 호출합니다.
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        // 체력이 0 이하로 떨어졌을 때 (승리)
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            UpdateUI();
            Die();
            return;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(currentHealth)} / {maxHealth}";
        }
    }

    private void Die()
    {
        Debug.Log("적을 물리쳤습니다! 스테이지 클리어!");
        // TODO: 보스 파괴 연출, 결과 화면 띄우기, 다음 스테이지로 이동 등
    }
}