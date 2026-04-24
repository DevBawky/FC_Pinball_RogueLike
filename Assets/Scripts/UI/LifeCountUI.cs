using System.Collections.Generic;
using UnityEngine;

public class LifeCountUI : MonoBehaviour
{
    public static LifeCountUI Instance;

    [Header("Life UI")]
    public Transform lifeIconParent;
    public GameObject lifeIconPrefab;

    private readonly List<GameObject> lifeIcons = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void Initialize(int maxLifeCount)
    {
        Clear();

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