using UnityEngine;
using System;
public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private ObjectPool pool;
    [SerializeField] private Transform defaultBulletParent;
    public void SetPool(ObjectPool p) => pool = p;
    public GameObject Spawn(BulletSpawnRequest request)
    {
        if(pool==null)
            throw new InvalidOperationException("pool为空");
        if (request.definition == null)
            throw new InvalidOperationException("bulletDefinition为空");
        return CreateBullet(request);
    }
    GameObject CreateBullet(BulletSpawnRequest request)
    {
        if (request.prefab == null || pool == null){
            throw new InvalidOperationException("bulletPrefab/pool为空");
        }

        // spawn点偏移（Line 模式下每条线从不同位置发出）


        GameObject bullet = pool.Get(
            request.prefab,
            request.context.position,
            request.context.rotation,
            parent:request.parent==null?defaultBulletParent:request.parent,
            beforeActivate: bulletObj =>
            {
                var mover = bulletObj.GetComponent<BulletMovementBase>();
                if(mover==null)
                    throw new InvalidOperationException("mover为空");
                mover.Init(request.definition, request.context);
                var replacer=bulletObj.GetComponent<FormReplacer>();
                if (replacer != null)
                {
                    replacer.Configure(this);
                }
            });
        return bullet;

        // Laser / Sniper 等需要额外设置（在具体组件中处理）
        // 目前 BulletPatternSO.Init 已经处理大部分情况
    }
}