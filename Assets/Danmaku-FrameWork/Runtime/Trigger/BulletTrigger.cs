using System;
using UnityEngine;
public class BulletTrigger : MonoBehaviour,IPoolable
{
    [SerializeField] private MonoBehaviour conditionSource;
    private ITriggerCondition _condition;
    private bool _isTriggered;
    public event Action Triggered;
    public void Awake()
    {
        _condition=conditionSource as ITriggerCondition;
        if(_condition == null)
            throw new InvalidOperationException("conditionSource必须实现ITriggerCondition");
    }
    public void OnSpawn()
    {
        _isTriggered=false;
        _condition.ResetRuntimeState();
    }
    public void OnDespawn()
    {
        Triggered=null;
    }
    public void Update()
    {
        if(_isTriggered)return;
        if(!_condition.Tick(Time.deltaTime))return;
        _isTriggered=true;
        Triggered?.Invoke();
    }

}
