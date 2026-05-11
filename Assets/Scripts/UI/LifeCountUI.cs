using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LifeCountUI : MonoBehaviour
{
    public static LifeCountUI Instance;

    [Header("Life UI")]
    public Transform lifeIconParent;
    public GameObject lifeIconPrefab;
    [SerializeField] private TMP_Text currentLifeText;

    private readonly List<GameObject> lifeIcons = new List<GameObject>();
    private int maxLifeCount;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void Initialize(int maxLifeCount)
    {
        this.maxLifeCount = maxLifeCount;
        Clear();

        if (lifeIconParent == null || lifeIconPrefab == null)
        {
            return;
        }

        for (int i = 0; i < maxLifeCount; i++)
        {
            GameObject icon = Instantiate(lifeIconPrefab, lifeIconParent);
            lifeIcons.Add(icon);
        }
    }

    public void Refresh(int currentLifeCount)
    {
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            lifeIcons[i].SetActive(i < currentLifeCount);
        }

        if (currentLifeText != null)
        {
            currentLifeText.text = $"{currentLifeCount} / {maxLifeCount}";
        }
    }

    private void Clear()
    {
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (lifeIcons[i] != null)
            {
                Destroy(lifeIcons[i]);
            }
        }

        lifeIcons.Clear();
    }
}
