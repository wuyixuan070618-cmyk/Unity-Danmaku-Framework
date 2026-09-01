using UnityEngine;
using System;
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class BulletMovementLaser : BulletMovementBase
{
    private LaserMoveConfigSO mvConfig;
    private RuntimeState _state;
    private RuntimeParameters _params;
    private SpriteRenderer _spriteRenderer;

    private Collider2D _collider;
    protected override void Move(float dt)
    {
        _state.ElapsedTime+=dt;
        if (_state.CurrentPhase == Phase.Warning)
        {
            float t = CalculateProgress(_state.ElapsedTime,_params.WarningDuration);
            if (t>=1f)
            {
                _state.CurrentPhase=Phase.Growing;
                _state.ElapsedTime=0f;
            }
        }
        else if (_state.CurrentPhase == Phase.Growing)
        {
            float t = CalculateProgress(_state.ElapsedTime,_params.GrowDuration);
            _state.CurrentLength=Mathf.Lerp(_params.InitialLength,_params.MaxLength,t);
            _state.CurrentWidth=Mathf.Lerp(_params.InitialWidth,_params.MaxWidth,t);
            _state.TargetScale.x=_state.CurrentWidth/_params.OriginalSize.x;
            _state.TargetScale.y=_state.CurrentLength/_params.OriginalSize.y;
            transform.localScale=_state.TargetScale;
            if (t >= 1f)
            {
                _state.CurrentPhase=Phase.Active;
                _state.ElapsedTime=0f;
                _collider.enabled=true;
            }
        }
        else if(_state.CurrentPhase == Phase.Active)
        {
            float t = CalculateProgress(_state.ElapsedTime,_params.ActiveDuration);
            if (t>=1f)
            {
                _state.CurrentPhase=Phase.Fading;
                _state.ElapsedTime=0f;
                _collider.enabled=false;
            }
        }
        else if(_state.CurrentPhase == Phase.Fading)
        {
            float t =CalculateProgress(_state.ElapsedTime,_params.FadingDuration);
            if(_params.AnimateLength)_state.CurrentLength=Mathf.Lerp(_params.MaxLength,0f,t);
            if(_params.AnimateWidth)_state.CurrentWidth=Mathf.Lerp(_params.MaxWidth,0f,t);
            _state.TargetScale.x=_state.CurrentWidth/_params.OriginalSize.x;
            _state.TargetScale.y=_state.CurrentLength/_params.OriginalSize.y;
            transform.localScale=_state.TargetScale;
            if (t>=1f)
            {
                _state.CurrentPhase=Phase.Warning;
                _state.ElapsedTime=0f;
                _collider.enabled=false;
                TryReturnOrDestroy();
            }
        }
    }
    protected override void OnInitialize(BulletDefinitionSO defSO,BulletSpawnContext ctx)
    {
        _spriteRenderer=GetComponent<SpriteRenderer>();
        if(_spriteRenderer==null||_spriteRenderer.sprite==null)
            throw new InvalidOperationException("spriteRenderer或sprite为空");
        _collider = GetComponent<Collider2D>();
        if(_collider==null)
            throw new InvalidOperationException("_collider为空");
        if(defSO==null||defSO.movementConfig==null)
            throw new InvalidOperationException("defSO或defSO.movementConfig为空");
        mvConfig=defSO.movementConfig as LaserMoveConfigSO;
        if(mvConfig==null)
            throw new InvalidOperationException("Definition配置类型不匹配");
        context=ctx;
        _params.WarningDuration=mvConfig.warningDuration;
        _params.GrowDuration=mvConfig.growDuration;
        _params.ActiveDuration=mvConfig.activeDuration;
        _params.FadingDuration=mvConfig.fadingDuration;
        _params.MaxLength=mvConfig.maxLength;
        _params.MaxWidth=mvConfig.maxWidth;
        _params.AnimateLength=mvConfig.animateLength;
        _params.AnimateWidth=mvConfig.animateWidth;
        _params.OriginalSize=_spriteRenderer.sprite.bounds.size;
        transform.rotation=context.rotation;
        _params.InitialLength=_params.AnimateLength?mvConfig.initialLength:mvConfig.maxLength;
        _params.InitialWidth=_params.AnimateWidth?mvConfig.initialWidth:mvConfig.maxWidth;
        _state.CurrentLength=_params.InitialLength;
        _state.CurrentWidth=_params.InitialWidth;
        transform.localScale=new Vector3(_state.CurrentWidth/_params.OriginalSize.x,_state.CurrentLength/_params.OriginalSize.y,1f);
        _state.TargetScale.z=1f;
        _state.ElapsedTime=0f;
        _state.CurrentPhase=Phase.Warning;
        _collider.enabled=false;
    }
    private struct RuntimeState
    {
        public float ElapsedTime;
        public float CurrentWidth;
        public float CurrentLength;
        public Phase CurrentPhase;
        public Vector3 TargetScale;
    }
    private struct RuntimeParameters
    {
        public float WarningDuration;
        public float GrowDuration;
        public float ActiveDuration;
        public float FadingDuration;
        public float MaxLength;
        public float MaxWidth;
        public float InitialLength;
        public float InitialWidth;
        public bool AnimateLength;
        public bool AnimateWidth;
        public Vector2 OriginalSize;        
    }
    private enum Phase
    {
        Warning,
        Growing,
        Active,
        Fading
    }
    private static float CalculateProgress(float elapsedTime, float duration)
    {
    return duration <= 0f
        ? 1f
        : Mathf.Clamp01(elapsedTime / duration);
    }
}
