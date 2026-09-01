using UnityEngine;
using System;
public class BulletMovementSine : BulletMovementBase
{
    private SineMoveConfigSO mvConfig;
    private RuntimeState _state;
    private float elapsedTime=0f;
    protected override void Move(float dt)
    {
        elapsedTime +=dt;
        if (elapsedTime < _state.Duration)
        {
            _state.Speed+=_state.Accel*dt;
            _state.ForwardDistance+=_state.Speed*dt;
            _state.PerpendicularValue=_state.Amplitude*Mathf.Sin(2*Mathf.PI*_state.Frequency*_state.ForwardDistance+_state.Phase*Mathf.PI);
            transform.position=context.position+(Vector3)(_state.ForwardDistance*context.direction+_state.PerpendicularDir*_state.PerpendicularValue);
            
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
        mvConfig=defSO.movementConfig as SineMoveConfigSO;
        if(mvConfig==null)
            throw new InvalidOperationException("Definition配置类型不匹配");
        context=ctx;
        _state.Speed=mvConfig.speed;
        _state.Accel=mvConfig.accel;
        _state.Duration=mvConfig.duration;
        _state.Amplitude=mvConfig.amplitude;
        _state.Frequency=mvConfig.frequency;
        _state.Phase=mvConfig.phase;
        _state.PerpendicularDir=new Vector2(-ctx.direction.y,ctx.direction.x).normalized;
        _state.ForwardDistance=0f;
        _state.PerpendicularValue=0f;
        elapsedTime=0f;
_state.PerpendicularValue=_state.Amplitude*Mathf.Sin(2*Mathf.PI*_state.Frequency*_state.ForwardDistance+_state.Phase*Mathf.PI);
            transform.position=context.position+(Vector3)(_state.ForwardDistance*context.direction+_state.PerpendicularDir*_state.PerpendicularValue);
    }
    private struct RuntimeState
    {
        public float Speed;
        public float Accel;
        public float Duration;
        public float Amplitude;
        public float Frequency;
        public float Phase;
        public Vector2 PerpendicularDir;
        public float ForwardDistance;
        public float PerpendicularValue;
    }
}
