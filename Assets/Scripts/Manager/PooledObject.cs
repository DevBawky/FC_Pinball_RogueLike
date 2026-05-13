using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public GameObject OriginPrefab { get; private set; }
    public bool IsInPool { get; set; }

    public void Initialize(GameObject originPrefab)
    {
        OriginPrefab = originPrefab;
        IsInPool = false;
    }
}
