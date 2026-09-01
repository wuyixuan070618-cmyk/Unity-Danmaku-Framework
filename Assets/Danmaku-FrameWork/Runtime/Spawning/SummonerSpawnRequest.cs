using UnityEngine;
public struct SummonerSpawnRequest
{
    public GameObject prefab;
    public Vector3 position;
    public Quaternion rotation;
    public Transform parent;
    public Transform owner;
    public Transform target;
}