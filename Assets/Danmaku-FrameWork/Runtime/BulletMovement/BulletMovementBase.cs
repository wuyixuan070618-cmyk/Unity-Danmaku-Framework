using System;
using UnityEngine;

public abstract class BulletMovementBase : MonoBehaviour
{
    protected BulletDefinitionSO definitionSO;
    public BulletSpawnContext context;
    public float Damage { get; private set; }
    public Vector2 CurrentDirection{get;private set;}
    public virtual void Init(BulletDefinitionSO defSO,BulletSpawnContext ctx)
    {
        CurrentDirection=ctx.direction;
        definitionSO = defSO;
        if (defSO == null || defSO.movementConfig == null)
        {
            throw new InvalidOperationException("defSO配置未完成!");
        }
        Damage=definitionSO.damage;
        context =ctx;
        var sr=GetComponent<SpriteRenderer>();
        if(sr!=null&& defSO.sprite != null)
        {
            sr.sprite=defSO.sprite;
            sr.color=defSO.tintColor;
        }
        OnInitialize(defSO,ctx);
    }

    void Update()
    {
        Vector3 previousPosition=transform.position;
        Move(Time.deltaTime);
        if(!isActiveAndEnabled)return;
        Vector3 displacement=transform.position - previousPosition;
        if(displacement.sqrMagnitude > 1e-6f)
        {
            CurrentDirection=displacement.normalized;
        }
        if (definitionSO.alignWithMovement && displacement.sqrMagnitude > 1e-6f)
        {
            transform.rotation=Quaternion.FromToRotation(Vector3.up,displacement);
        }
    }
    protected abstract void Move(float dt);
    protected abstract void OnInitialize(BulletDefinitionSO defSO,BulletSpawnContext ctx);
    public virtual void TryReturnOrDestroy()
    {
        var pooled = GetComponent<PooledObject>();
        if (pooled != null && pooled.Pool != null && pooled.Prefab != null)
        {
            pooled.ReturnToPool();
        }
        else
            Destroy(gameObject);
    }
}
