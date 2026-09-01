using UnityEngine;
using System;
public class BulletMovementLinear : BulletMovementBase
{
    private LinearMoveConfigSO mvConfig;
    private RuntimeState _state;
    private float elapsedTime=0f;
    protected override void Move(float dt)
    {
        elapsedTime +=dt;
        if (elapsedTime < _state.Duration)
        {
            _state.Speed+=_state.Accel*dt;
            transform.Translate(_state.Speed*dt*context.direction,Space.World);
        }
        else
        {

            TryReturnOrDestroy();
        }
    }
    protected override void OnInitialize(BulletDefinitionSO defSO,BulletSpawnContext ctx)
    {
        if(defSO==null||defSO.movementConfig==null)
            throw new InvalidOperationException("defSO或defSO.movementConfig为空");
        mvConfig=defSO.movementConfig as LinearMoveConfigSO;
        if(mvConfig==null)
            throw new InvalidOperationException("Definition配置类型不匹配");
        context=ctx;
        _state.Speed=mvConfig.speed;
        _state.Accel=mvConfig.accel;
        _state.Duration=mvConfig.duration;
        elapsedTime=0f;
    }
    private struct RuntimeState
    {
        public float Speed;
        public float Accel;
        public float Duration;
    }
}
