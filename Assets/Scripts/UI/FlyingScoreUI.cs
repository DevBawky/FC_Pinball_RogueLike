using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FlyingScoreUI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 1500f;         
    public float arrivalDistance = 20f; 

    [Header("ZigZag Settings")]
    public float waveFrequency = 15f;   
    public float waveAmplitude = 100f;  
    
    private float randomTimeOffset;
    private Transform targetTransform;
    
    private ScoreType scoreType;
    private float scoreValue;

    [Header("Colors")]
    public Color chipsColor = Color.red;
    public Color multColor = Color.blue;

    public void Initialize(Transform target, ScoreType type, float value)
    {
        GetComponent<Image>().color = (type == ScoreType.Chips) ? chipsColor : multColor;

        targetTransform = target;
        scoreType = type;
        scoreValue = value;

        randomTimeOffset = Random.Range(0f, Mathf.PI * 2f);
        StartCoroutine(FlyToTarget());
    }

    private IEnumerator FlyToTarget()
    {
        Vector3 currentBasePosition = transform.position;
        float initialDistance = Vector3.Distance(currentBasePosition, targetTransform.position);

        while (targetTransform != null && Vector3.Distance(currentBasePosition, targetTransform.position) > arrivalDistance)
        {
            currentBasePosition = Vector3.MoveTowards(currentBasePosition, targetTransform.position, speed * Time.deltaTime);
            Vector3 direction = (targetTransform.position - currentBasePosition).normalized;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f).normalized;
            float currentDistance = Vector3.Distance(currentBasePosition, targetTransform.position);
            float distanceRatio = currentDistance / initialDistance; 
            float waveOffset = Mathf.Sin((Time.time + randomTimeOffset) * waveFrequency) * (waveAmplitude * distanceRatio);

            transform.position = currentBasePosition + (perpendicular * waveOffset);
            yield return null;
        }

        // 목적지 도착 시 점수 올리기
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreType, scoreValue);
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (MainGameUIManager.Instance != null)
        {
            MainGameUIManager.Instance.activeScoreParticles =
                Mathf.Max(0, MainGameUIManager.Instance.activeScoreParticles - 1);
        }
    }
}
