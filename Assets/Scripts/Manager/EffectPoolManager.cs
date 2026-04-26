using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPoolManager : MonoBehaviour
{
    private const int VisibleEffectSortingOrder = 20;

    private sealed class EffectPool
    {
        public readonly GameObject Prefab;
        public readonly Queue<GameObject> Available = new Queue<GameObject>();
        public readonly Transform Root;
        public int CreatedCount;

        public EffectPool(GameObject prefab, Transform root)
        {
            Prefab = prefab;
            Root = root;
        }
    }

    private static EffectPoolManager instance;
    private static bool isQuitting;

    private readonly Dictionary<GameObject, EffectPool> pools = new Dictionary<GameObject, EffectPool>();

    private static EffectPoolManager Instance
    {
        get
        {
            if (instance == null && !isQuitting)
            {
                GameObject managerObject = new GameObject("@_EffectPoolManager");
                instance = managerObject.AddComponent<EffectPoolManager>();
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

    public static void Play(GameObject prefab, Vector3 position, float duration)
    {
        Play(prefab, position, duration, Color.white);
    }

    public static void Play(GameObject prefab, Vector3 position, float duration, Color color)
    {
        if (prefab == null || !Application.isPlaying || isQuitting)
        {
            return;
        }

        Instance?.PlayInternal(prefab, position, Quaternion.identity, duration, color);
    }

    private void PrewarmInternal(GameObject prefab, int count)
    {
        EffectPool pool = GetOrCreatePool(prefab);

        while (pool.CreatedCount < count)
        {
            GameObject effectObject = CreateEffectObject(pool);
            pool.Available.Enqueue(effectObject);
        }
    }

    private void PlayInternal(GameObject prefab, Vector3 position, Quaternion rotation, float duration, Color color)
    {
        EffectPool pool = GetOrCreatePool(prefab);
        GameObject effectObject = pool.Available.Count > 0
            ? pool.Available.Dequeue()
            : CreateEffectObject(pool);

        Transform effectTransform = effectObject.transform;
        effectTransform.SetPositionAndRotation(position, rotation);
        ApplyColor(effectObject, color);
        effectObject.SetActive(true);
        RestartAnimators(effectObject);

        StartCoroutine(ReturnAfterDelay(effectObject, pool, Mathf.Max(0f, duration)));
    }

    private EffectPool GetOrCreatePool(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out EffectPool pool))
        {
            return pool;
        }

        GameObject rootObject = new GameObject($"{prefab.name} Pool");
        rootObject.transform.SetParent(transform);
        pool = new EffectPool(prefab, rootObject.transform);
        pools.Add(prefab, pool);
        return pool;
    }

    private GameObject CreateEffectObject(EffectPool pool)
    {
        GameObject effectObject = Instantiate(pool.Prefab, pool.Root);
        effectObject.SetActive(false);
        pool.CreatedCount++;
        return effectObject;
    }

    private IEnumerator ReturnAfterDelay(GameObject effectObject, EffectPool pool, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (effectObject == null)
        {
            yield break;
        }

        effectObject.SetActive(false);
        effectObject.transform.SetParent(pool.Root);
        pool.Available.Enqueue(effectObject);
    }

    private void RestartAnimators(GameObject effectObject)
    {
        Animator[] animators = effectObject.GetComponentsInChildren<Animator>();
        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].Rebind();
            animators[i].Update(0f);
        }
    }

    private void ApplyColor(GameObject effectObject, Color color)
    {
        SpriteRenderer[] renderers = effectObject.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].color = color;
            renderers[i].sortingOrder = Mathf.Max(renderers[i].sortingOrder, VisibleEffectSortingOrder);
        }
    }
}
