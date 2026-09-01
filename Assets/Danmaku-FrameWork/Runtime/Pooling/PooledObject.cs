using UnityEngine;

public class PooledObject : MonoBehaviour
{
    [HideInInspector] public GameObject Prefab;
    [HideInInspector] public ObjectPool Pool;
    [HideInInspector] public bool isInPool = false;
    public void ReturnToPool()
    {
        if (Pool != null && Prefab != null)
            Pool.Return(Prefab, gameObject);
        else
            Destroy(gameObject);
    }
}
