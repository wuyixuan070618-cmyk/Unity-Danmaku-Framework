using UnityEngine;
using System;
public class SummonerSpawner : MonoBehaviour
{
    [SerializeField] private ObjectPool pool;
    [SerializeField] private Transform defaultSummonerParent;
    [SerializeField] private BulletSpawner bulletSpawner;
    public void SetPool(ObjectPool p) => pool = p;
    public GameObject Spawn(SummonerSpawnRequest request)
    {
        if(pool==null)
            throw new InvalidOperationException("pool为空");
        if(bulletSpawner==null)
            throw new InvalidOperationException("spawner为空");
        return CreateSummoner(request);
    }
    GameObject CreateSummoner(SummonerSpawnRequest request)
    {
        if (request.prefab == null || pool == null){
            throw new InvalidOperationException("summonerPrefab/pool为空");
        }

        // spawn点偏移（Line 模式下每条线从不同位置发出）


        GameObject summoner = pool.Get(
            request.prefab,
            request.position,
            request.rotation,
            parent:request.parent==null?defaultSummonerParent:request.parent,
            beforeActivate: summonerObj =>
            {
                var summoner = summonerObj.GetComponent<BulletSummoner>();
                if(summoner==null)
                    throw new InvalidOperationException("summoner为空");
                summoner.Configure(bulletSpawner,request.target,request.owner);
            });

        if (summoner == null) return null;
        return summoner;
        // Laser / Sniper 等需要额外设置（在具体组件中处理）
        // 目前 BulletPatternSO.Init 已经处理大部分情况
    }
}