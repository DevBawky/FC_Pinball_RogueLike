using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeightedSpawnObjectEntry
{
    public GameObject prefab;
    [Min(0f)] public float weight = 1f;
}

public class BattleObjectSpawner : MonoBehaviour
{
    [Header("Spawn Area")]
    [SerializeField] private BoxCollider2D spawnArea;
    [SerializeField] private Transform spawnedObjectParent;

    [Header("Spawn Settings")]
    [SerializeField] private int minSpawnCount = 3;
    [SerializeField] private int maxSpawnCount = 5;
    [SerializeField] private int maxSpawnAttemptsPerObject = 12;
    [SerializeField] private float minDistanceBetweenObjects = 1.2f;

    [Header("Weighted Prefabs")]
    [SerializeField] private List<WeightedSpawnObjectEntry> spawnEntries = new List<WeightedSpawnObjectEntry>();

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    public void SpawnForBattle()
    {
        ClearSpawnedObjects();

        int targetSpawnCount = GetRandomSpawnCount();

        if (spawnArea == null || spawnEntries.Count == 0 || targetSpawnCount <= 0)
        {
            return;
        }

        Bounds bounds = spawnArea.bounds;
        List<Vector2> usedPositions = new List<Vector2>();
        int totalAttempts = 0;
        int maxTotalAttempts = Mathf.Max(1, targetSpawnCount * Mathf.Max(1, maxSpawnAttemptsPerObject) * 3);

        while (spawnedObjects.Count < targetSpawnCount && totalAttempts < maxTotalAttempts)
        {
            totalAttempts++;

            GameObject prefabToSpawn = GetWeightedRandomPrefab();
            if (prefabToSpawn == null)
            {
                continue;
            }

            if (!TryGetSpawnPosition(bounds, usedPositions, out Vector2 spawnPosition))
            {
                continue;
            }

            Transform parent = spawnedObjectParent != null ? spawnedObjectParent : transform;
            GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, parent);
            spawnedObjects.Add(spawnedObject);
            usedPositions.Add(spawnPosition);
        }

        if (spawnedObjects.Count < targetSpawnCount)
        {
            Debug.LogWarning(
                $"[BattleObjectSpawner] Requested {targetSpawnCount} objects, but only spawned {spawnedObjects.Count}. " +
                $"Check spawn area size or lower minDistanceBetweenObjects.");
        }
    }

    public void ClearSpawnedObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(spawnedObjects[i]);
            }
        }

        spawnedObjects.Clear();
    }

    private GameObject GetWeightedRandomPrefab()
    {
        float totalWeight = 0f;

        for (int i = 0; i < spawnEntries.Count; i++)
        {
            WeightedSpawnObjectEntry entry = spawnEntries[i];
            if (entry == null || entry.prefab == null || entry.weight <= 0f)
            {
                continue;
            }

            totalWeight += entry.weight;
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        for (int i = 0; i < spawnEntries.Count; i++)
        {
            WeightedSpawnObjectEntry entry = spawnEntries[i];
            if (entry == null || entry.prefab == null || entry.weight <= 0f)
            {
                continue;
            }

            cumulativeWeight += entry.weight;
            if (randomValue <= cumulativeWeight)
            {
                return entry.prefab;
            }
        }

        return spawnEntries[spawnEntries.Count - 1] != null ? spawnEntries[spawnEntries.Count - 1].prefab : null;
    }

    private bool TryGetSpawnPosition(Bounds bounds, List<Vector2> usedPositions, out Vector2 spawnPosition)
    {
        for (int attempt = 0; attempt < Mathf.Max(1, maxSpawnAttemptsPerObject); attempt++)
        {
            Vector2 candidate = new Vector2(
                UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                UnityEngine.Random.Range(bounds.min.y, bounds.max.y));

            if (IsFarEnough(candidate, usedPositions))
            {
                spawnPosition = candidate;
                return true;
            }
        }

        spawnPosition = Vector2.zero;
        return false;
    }

    private bool IsFarEnough(Vector2 candidate, List<Vector2> usedPositions)
    {
        for (int i = 0; i < usedPositions.Count; i++)
        {
            if (Vector2.Distance(candidate, usedPositions[i]) < minDistanceBetweenObjects)
            {
                return false;
            }
        }

        return true;
    }

    private int GetRandomSpawnCount()
    {
        int clampedMin = Mathf.Max(0, minSpawnCount);
        int clampedMax = Mathf.Max(clampedMin, maxSpawnCount);

        if (clampedMin == clampedMax)
        {
            return clampedMin;
        }

        return UnityEngine.Random.Range(clampedMin, clampedMax + 1);
    }
}
