using UnityEngine;

public struct BulletSpawnRequest
{
    public GameObject prefab;
    public BulletDefinitionSO definition;
    public BulletSpawnContext context;
    public Transform parent;
}