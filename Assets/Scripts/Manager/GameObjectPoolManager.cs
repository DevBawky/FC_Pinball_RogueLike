using System.Collections.Generic;
using UnityEngine;

public class GameObjectPoolManager : MonoBehaviour
{
    private sealed class Pool
    {
        public readonly GameObject Prefab;
        public readonly Queue<GameObject> Available = new Queue<GameObject>();
        public readonly Transform Root;
        public int CreatedCount;

        public Pool(GameObject prefab, Transform root)
        {
            Prefab = prefab;
            Root = root;
        }
    }

    private static GameObjectPoolManager instance;
    private static bool isQuitting;

    private readonly Dictionary<GameObject, Pool> pools = new Dictionary<GameObject, Pool>();

    private static GameObjectPoolManager Instance
    {
        get
        {
            if (instance == null && !isQuitting)
            {
                GameObject managerObject = new GameObject("@_GameObjectPoolManager");
                instance = managerObject.AddComponent<GameObjectPoolManager>();
                DontDestroyOnLoad(managerObject);
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    public static void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0 || !Application.isPlaying || isQuitting)
        {
            return;
        }

        Instance?.PrewarmInternal(prefab, count);
    }

    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null || !Application.isPlaying || isQuitting)
        {
            return null;
        }

        return Instance?.SpawnInternal(prefab, position, rotation, parent);
    }

    public static GameObject SpawnFromInstance(GameObject instanceObject, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (instanceObject == null)
        {
            return null;
        }

        PooledObject pooledObject = instanceObject.GetComponent<PooledObject>();
        GameObject prefab = pooledObject != null ? pooledObject.OriginPrefab : null;

        return prefab != null
            ? Spawn(prefab, position, rotation, parent)
            : Instantiate(instanceObject, position, rotation, parent);
    }

    public static bool Release(GameObject instanceObject)
    {
        if (instanceObject == null || isQuitting)
        {
            return false;
        }

        PooledObject pooledObject = instanceObject.GetComponent<PooledObject>();
        if (pooledObject == null || pooledObject.OriginPrefab == null)
        {
            Destroy(instanceObject);
            return false;
        }

        Instance?.ReleaseInternal(instanceObject, pooledObject);
        return true;
    }

    private void PrewarmInternal(GameObject prefab, int count)
    {
        Pool pool = GetOrCreatePool(prefab);

        while (pool.CreatedCount < count)
        {
            GameObject pooledObject = CreateObject(pool);
            pool.Available.Enqueue(pooledObject);
        }
    }

    private GameObject SpawnInternal(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        Pool pool = GetOrCreatePool(prefab);
        GameObject instanceObject = null;

        while (pool.Available.Count > 0 && instanceObject == null)
        {
            instanceObject = pool.Available.Dequeue();
        }

        if (instanceObject == null)
        {
            instanceObject = CreateObject(pool);
        }

        PooledObject pooledObject = instanceObject.GetComponent<PooledObject>();
        if (pooledObject != null)
        {
            pooledObject.IsInPool = false;
        }

        Transform instanceTransform = instanceObject.transform;
        instanceTransform.SetParent(parent, false);
        instanceTransform.SetPositionAndRotation(position, rotation);
        instanceObject.SetActive(true);
        return instanceObject;
    }

    private void ReleaseInternal(GameObject instanceObject, PooledObject pooledObject)
    {
        if (pooledObject.IsInPool)
        {
            return;
        }

        Pool pool = GetOrCreatePool(pooledObject.OriginPrefab);
        pooledObject.IsInPool = true;
        instanceObject.SetActive(false);
        instanceObject.transform.SetParent(pool.Root, false);
        pool.Available.Enqueue(instanceObject);
    }

    private Pool GetOrCreatePool(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out Pool pool))
        {
            return pool;
        }

        GameObject rootObject = new GameObject($"{prefab.name} Pool");
        rootObject.transform.SetParent(transform);
        pool = new Pool(prefab, rootObject.transform);
        pools.Add(prefab, pool);
        return pool;
    }

    private GameObject CreateObject(Pool pool)
    {
        GameObject instanceObject = Instantiate(pool.Prefab, pool.Root);
        PooledObject pooledObject = instanceObject.GetComponent<PooledObject>();
        if (pooledObject == null)
        {
            pooledObject = instanceObject.AddComponent<PooledObject>();
        }

        pooledObject.Initialize(pool.Prefab);
        pooledObject.IsInPool = true;
        instanceObject.SetActive(false);
        pool.CreatedCount++;
        return instanceObject;
    }
}
