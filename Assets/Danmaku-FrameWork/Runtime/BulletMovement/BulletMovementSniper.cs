using System;
using UnityEngine;

public class BulletMovementSniper : BulletMovementBase
{
    // Start is called before the first frame update

    private SniperMoveConfigSO mvConfig;
    private RuntimeState _state;

    protected override void Move(float dt)
    {
        _state.ElapsedTime+=dt;
        if(_state.ElapsedTime>_state.Duration)
        {
            TryReturnOrDestroy();
            return;
        }
        _state.Speed+=_state.Accel*dt;
        transform.Translate(dt*_state.Speed*_state.Direction,Space.World);
    }
    protected override void OnInitialize(BulletDefinitionSO defSO, BulletSpawnContext ctx)
    {
        if(defSO==null||defSO.movementConfig==null)
            throw new InvalidOperationException("defSO或defSO.movementConfig为空");
        mvConfig=defSO.movementConfig as SniperMoveConfigSO;
        if(mvConfig==null)
            throw new InvalidOperationException("Definition配置类型不匹配");
        _state=new RuntimeState
        {
            Speed=mvConfig.speed,
            Accel = mvConfig.accel,
            Direction = ctx.target!=null?(ctx.target.position-transform.position).normalized:ctx.direction.normalized,
            ElapsedTime = 0f,
            Duration = mvConfig.duration
            
        };
    }
    struct RuntimeState
    {
        public float Speed;
        public float Accel;
        public Vector2 Direction;
        public float ElapsedTime;
        public float Duration;
    }
}
