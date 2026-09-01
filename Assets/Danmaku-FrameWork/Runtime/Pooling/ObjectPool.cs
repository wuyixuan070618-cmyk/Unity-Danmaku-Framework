using System;
using System.Collections.Generic;
using UnityEngine;
public interface IPoolable
{
    void OnSpawn();
    void OnDespawn();
}
[DisallowMultipleComponent]
public class ObjectPool : MonoBehaviour 
{
    [System.Serializable]
    public class PoolItem 
    {
        public GameObject prefab;
        public int initial=16;
    }
    public PoolItem[] pools;
    Transform _root;
    Transform _inactiveFactoryRoot;

    readonly Dictionary<GameObject,Queue<GameObject>> _map=new Dictionary<GameObject,Queue<GameObject>>();
    private void Awake()
    {
        _root=new GameObject("ObjectRoot").transform;
        _root.SetParent(transform, false);

        // 在 inactive 层级中创建新对象，避免 Instantiate 时提前触发 OnEnable。
        _inactiveFactoryRoot = new GameObject("InactiveFactoryRoot").transform;
        _inactiveFactoryRoot.SetParent(transform, false);
        _inactiveFactoryRoot.gameObject.SetActive(false);
        PrewarmAll();
    }
    private void OnDestroy()
    {
        if(_root!=null)Destroy(_root.gameObject);
        if(_inactiveFactoryRoot!=null)Destroy(_inactiveFactoryRoot.gameObject);
    }
    public void PrewarmAll()
    {
        foreach(var p in pools)
        {
            if (p == null || p.prefab == null) continue;
            if (!_map.TryGetValue(p.prefab, out var q))
            {
                q = new Queue<GameObject>();
                _map[p.prefab] = q;
            }
            for(int i = 0; i < Mathf.Max(0, p.initial); i++)
            {
                var go = CreateNew(p.prefab);
                go.SetActive(false);
                q.Enqueue(go);
            }
        }
    }
    GameObject CreateNew(GameObject prefab)
    {
        // 父节点处于 inactive 状态，所以默认激活的 prefab 在构造期间也不会 OnEnable。
        var go = Instantiate(prefab, _inactiveFactoryRoot);
        go.name = prefab.name;
        var pooled = go.GetComponent<PooledObject>();
        if (pooled == null) pooled = go.AddComponent<PooledObject>();
        pooled.Prefab=prefab;
        pooled.Pool = this;
        go.SetActive(false);
        go.transform.SetParent(_root, worldPositionStays: false);
        pooled.isInPool = true;
        return go;
    }
    public GameObject Get(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent = null,
        Action<GameObject> beforeActivate = null)
    {
        if (prefab == null) return null;
        if(!_map.TryGetValue(prefab, out var q) || q == null)
        {
            q=new Queue<GameObject>();
            _map[prefab]=q;
        }
        GameObject item = q.Count > 0 ? q.Dequeue() : CreateNew(prefab);
        var pooled=item.GetComponent<PooledObject>();
        item.transform.SetParent(parent ?? _root, worldPositionStays: false);
        item.transform.position = position;
        item.transform.rotation = rotation;

        // 调用方在对象仍处于 inactive 时注入本轮所需配置。
        try
        {
            beforeActivate?.Invoke(item);
        }
        catch
        {
            item.transform.SetParent(_root, worldPositionStays: false);
            q.Enqueue(item);
            throw;
        }
        pooled.isInPool = false;
        item.SetActive(true);

        foreach (var comp in item.GetComponents<IPoolable>())
        {
            comp.OnSpawn();
        }
        return item;
    }

    public void Return(GameObject prefab,GameObject instance)
    {
        if (instance == null || prefab == null) return;
        var pooled = instance.GetComponent<PooledObject>();
        if (pooled == null || pooled.Prefab != prefab)
        {
            Destroy(instance); return;
        }
        if(pooled.isInPool)
        {
            Debug.LogError("重复回池触发!");
            return;
        }
        pooled.isInPool = true;
        foreach (var comp in instance.GetComponents<IPoolable>())
            comp.OnDespawn();
        instance.SetActive(false);
        instance.transform.SetParent(_root, worldPositionStays: false);
        if (!_map.TryGetValue(prefab,out var q) || q == null)
        {
            q = new Queue<GameObject>();
            _map[prefab]=q;
        }
        q.Enqueue(instance);
    }
    public void Return(GameObject instance)
    {
        if (instance == null)return;
        var pooled = instance.GetComponent<PooledObject>();
        if (pooled == null || pooled.Prefab == null)
        {
            Destroy(instance); return;
        }
        Return(pooled.Prefab, instance);
    }
}
