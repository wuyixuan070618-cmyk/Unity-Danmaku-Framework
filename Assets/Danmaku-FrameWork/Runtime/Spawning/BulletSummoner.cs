using System;
using System.Collections;
using UnityEngine;
public class BulletSummoner:MonoBehaviour
{
    [SerializeField] private FireSequenceSO sequence;
    [SerializeField] private Transform aimReferenceOverride;

    [Range(-180,180)]
    [SerializeField] private float defaultOffsetAngle=0;
    private Transform EffectiveAimReference=>aimReferenceOverride!=null?aimReferenceOverride:transform.parent;
    private Transform targetTrans;
    private Transform ownerTrans;
    private int phaseIndex=0;
    private float phaseElapsed = 0f;
    private float rotationAccum=0f;
    private Coroutine fireRoutine;
    private BulletSpawner bulletSpawner;

    public FireSequenceSO Sequence => sequence;
    public Transform AimReferenceOverride => aimReferenceOverride;
    public float DefaultOffsetAngle => defaultOffsetAngle;
    public Transform TargetTransform => targetTrans;
    public Transform OwnerTransform => ownerTrans;
    public int CurrentPhaseIndex => phaseIndex;
    public float RotationAccum => rotationAccum;

    void OnEnable()
    {
        StartFiring();
    }
    void OnDisable()
    {
        phaseIndex=0;
        phaseElapsed=0;
        rotationAccum=0;
        StopFiring();
    }
    void Update()
    {
        if(sequence==null||sequence.phases.Length==0)return;
        var phase =sequence.phases[phaseIndex];
        if (phase.duration > 0)
        {
            phaseElapsed+=Time.deltaTime;
            if (phaseElapsed >= phase.duration)
            {
                SwitchToPhase((phaseIndex+1)%sequence.phases.Length);
            }
        }
    }
    void SwitchToPhase(int index)
    {
        phaseIndex=index;
        phaseElapsed=0f;
        rotationAccum=0f;
        StopFiring();
        StartFiring();
    }
    public void StartFiring()
    {
        if (sequence == null || fireRoutine != null||sequence.phases.Length==0||sequence.phases[phaseIndex]==null) return;
        if(bulletSpawner==null)throw new InvalidOperationException("bulletSpawner为空");
        fireRoutine = StartCoroutine(FireRoutine());
    }

    public void StopFiring()
    {
        if (fireRoutine == null) return;
        StopCoroutine(fireRoutine);
        fireRoutine = null;
    }
    IEnumerator FireRoutine()
    {
        float lastShotTime=Time.time;
        float currentTime=Time.time;
        float elapsedSinceLastShot;
        while (true)
        {
            var phase = sequence.phases[phaseIndex];
            
            // 旋转累计
            currentTime=Time.time;
            elapsedSinceLastShot=currentTime-lastShotTime;
            rotationAccum += phase.rotationSpeed * elapsedSinceLastShot;
            lastShotTime=currentTime;
            // ① 计算基准方向
            Vector2 baseDir = GetBaseDirection(phase.aimType, phase.offsetAngle+defaultOffsetAngle);

            // ② 发射每一颗子弹
            for (int i = 0; i < phase.bulletCount; i++)
            {

                Vector2 dir = BulletFireMath.CalcBulletDirection(phase.shape, baseDir, i, phase.bulletCount,sequence.phases[phaseIndex].angleSpread,rotationAccum);
                dir = BulletFireMath.ApplyRandom(phase, dir);
                Vector3 spawnPos = transform.position;
                var spawnContext=new BulletSpawnContext(
                    spawnPos,
                    dir,
                    owner: ownerTrans==null?transform:ownerTrans,
                    target:targetTrans
                );
                bulletSpawner.Spawn(new BulletSpawnRequest
                {
                    prefab=phase.bulletPrefab,
                    definition=phase.bulletDefinition,
                    context=spawnContext,
                });

            }
            
            yield return new WaitForSeconds(phase.fireInterval);
        }
    }
    public void Configure(BulletSpawner spawner,Transform target=null,Transform owner=null)
    {
        bulletSpawner=spawner;
        targetTrans=target??null;
        ownerTrans=owner??transform;
    }
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

    Vector2 GetBaseDirection(AimType aim, float offsetAngle)
    {
        if(aim==AimType.AimToTarget){
            if ( targetTrans== null) return Quaternion.Euler(0, 0, offsetAngle) * Vector2.down;
            Vector2 toPlayer = (targetTrans.position - transform.position).normalized;
            if (toPlayer.sqrMagnitude < 1e-6f) toPlayer = Vector2.down;
            return Quaternion.Euler(0, 0, offsetAngle) * toPlayer;
        }
        if (aim == AimType.OppositeToParent)
        {
            return EffectiveAimReference==null ? Quaternion.Euler(0, 0, offsetAngle) * Vector2.down
            : Quaternion.Euler(0, 0, offsetAngle) * ((Vector2)(transform.position-EffectiveAimReference.position)).normalized;
        }
        if(aim==AimType.None) 
        {
            Vector2 baseDirction=transform.rotation*Vector2.right;
            return Quaternion.Euler(0f,0f,offsetAngle)*baseDirction;
        }
        return Vector2.right;
    }

}
