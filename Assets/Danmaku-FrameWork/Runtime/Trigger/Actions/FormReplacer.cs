using System;
using UnityEngine;
public class FormReplacer : MonoBehaviour,ITriggerAction
{
    [SerializeField] private GameObject nextPrefab;
    [SerializeField] private BulletDefinitionSO nextDefinition;
    private BulletTrigger _trigger;
    private BulletSpawner _bulletSpawner;
    private BulletMovementBase _currentMover;

    void Awake()
    {
        _trigger=GetComponent<BulletTrigger>();
        if(_trigger==null)
            throw new InvalidOperationException("FormReplacer 的 trigger 为空");
        _currentMover=GetComponent<BulletMovementBase>();
        if(_currentMover==null)
            throw new InvalidOperationException("FormReplacer 的 currentMover 为空");
    }

    void OnEnable()
    {
        _trigger.Triggered+=Execute;
    }
    void OnDisable()
    {
        _trigger.Triggered-=Execute;
    }
    public void Execute()
    {
        if(_bulletSpawner==null)
            throw new InvalidOperationException("FormReplacer 的 Spawner 为空");
        GameObject newBullet=_bulletSpawner.Spawn(new BulletSpawnRequest
        {
            prefab=nextPrefab,
            definition=nextDefinition,
            context=new BulletSpawnContext(
                _currentMover.transform.position,
                _currentMover.CurrentDirection,
                _currentMover.context.owner,
                _currentMover.context.target
            ),
            parent=_currentMover.transform.parent
        });
        if(newBullet==null)
            throw new InvalidOperationException("Replacer生成过程发生错误");
        _currentMover.TryReturnOrDestroy();
    }
    public void Configure(BulletSpawner bulletSpawner)
    {
        _bulletSpawner=bulletSpawner;
    }
}
