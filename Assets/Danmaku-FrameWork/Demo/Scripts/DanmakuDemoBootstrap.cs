using System;
using UnityEngine;
public class DanmakuDemoBootstrap : MonoBehaviour
{
    [SerializeField] GameObject summonerPrefab;
    [SerializeField] Transform spawnerRoot; 
    [SerializeField] SummonerSpawner _summonerSpawner;
    private GameObject _activeSummoner;
    private float timer=0f;
    public Vector3 Position
    {
        get => transform.position;
        set=>transform.position=value;
    }

    protected void Awake()
    {
        if(summonerPrefab==null)
            throw new InvalidOperationException("未设定spawner");
    }
    public void Start()
    {
        GameObject summonerObj;
        summonerObj=_summonerSpawner.Spawn(new SummonerSpawnRequest{
                prefab=summonerPrefab,
                position=transform.position,
                rotation=Quaternion.identity,
                parent=transform,
                owner=transform,
                target=null
                }); 
        if(summonerObj!=null)_activeSummoner=summonerObj;
        else Debug.Log("生成失败");
    }
    protected void Update()
    {
        timer+=Time.deltaTime;
    }
}
