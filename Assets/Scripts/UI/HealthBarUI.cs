using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI Settings")]
    public Image fillImage;
    public float maxHealth = 100f;
    
    [Header("Animation Settings")]
    public float fillSpeed = 5f;

    private float currentHealth;
    private float targetFillAmount;

    void Start()
    {
        currentHealth = maxHealth;
        targetFillAmount = 1f;
        fillImage.fillAmount = 1f;
    }

    void Update()
    {
        if (fillImage.fillAmount != targetFillAmount)
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, Time.deltaTime * fillSpeed);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        targetFillAmount = currentHealth / maxHealth;
    }
}