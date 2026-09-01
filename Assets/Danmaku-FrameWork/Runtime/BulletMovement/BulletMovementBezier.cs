using UnityEngine;
using System;
public class BulletMovementBezier : BulletMovementBase
{
    private BezierMoveConfigSO mvConfig;

    private RuntimeState _state;

    private Vector3[] _evalBuffer = null;


    protected override void OnInitialize(BulletDefinitionSO defSO, BulletSpawnContext ctx)
    {
        if(defSO==null||defSO.movementConfig==null)
            throw new InvalidOperationException("defSO或defSO.movementConfig为空");
        mvConfig=defSO.movementConfig as BezierMoveConfigSO;
        if(mvConfig==null)
            throw new InvalidOperationException("Definition配置类型不匹配");
        _state=new RuntimeState
        {
          duration=Mathf.Max(0.0001f,mvConfig.duration),
          controlPoints=mvConfig.controlPoints!= null ? (Vector3[])mvConfig.controlPoints.Clone() : new Vector3[0],
          easing=mvConfig.easing,
          elapsed=0f
        };
        if (_state.controlPoints.Length < 2)
        {
            throw new InvalidOperationException("Bezier 至少需要两个控制点");
        }
        context=ctx;
        Vector3 dir3D=(Vector3)ctx.direction==Vector3.zero?Vector3.right:new Vector3(ctx.direction.x,ctx.direction.y,0).normalized;

        Quaternion rot = Vector3.Dot(Vector3.right,dir3D)!=-1?Quaternion.FromToRotation(Vector3.right, dir3D):Quaternion.AngleAxis(180f,Vector3.forward);
        for (int i = 0; i <_state.controlPoints.Length; i++)
        {
            Vector3 rel = _state.controlPoints[i];
            Vector3 rotated = rot * rel;
            _state.controlPoints[i] = rotated;
        }
         // 分配复用缓冲区
        _evalBuffer = new Vector3[_state.controlPoints.Length];

        _state.elapsed = 0f;
    }
    struct RuntimeState
    {
        public float duration;
        public Vector3[] controlPoints;
        public AnimationCurve easing;
        public float elapsed;
    }
    protected override void Move(float dt)
    {
        _state.elapsed += dt;
        float tRaw = Mathf.Clamp01(_state.elapsed / Mathf.Max(0.0001f, _state.duration));
        float mappedT = _state.easing != null ? _state.easing.Evaluate(tRaw) : tRaw;
        Vector3 pLocal = EvaluateBezierNonAlloc(_state.controlPoints, mappedT, _evalBuffer);
        Vector3 p =  context.position + pLocal;
        transform.position = p;
        if (_state.elapsed > _state.duration)
        {
            TryReturnOrDestroy();
            return;
        }
    }

    // 非分配的贝塞尔求值：使用外部缓冲 tmp（长度至少等于 pts.Length）
    static Vector3 EvaluateBezierNonAlloc(Vector3[] pts, float t, Vector3[] tmp)
    {
        int n = pts != null ? pts.Length : 0;
        if (n == 0) return Vector3.zero;
        if (n == 1) return pts[0];
        if (tmp == null || tmp.Length < n)
        {
            Vector3[] tmpAlloc = new Vector3[n];
            for (int i = 0; i < n; i++) tmpAlloc[i] = pts[i];
            for (int k = 1; k < n; k++)
                for (int i = 0; i < n - k; i++)
                    tmpAlloc[i] = Vector3.Lerp(tmpAlloc[i], tmpAlloc[i + 1], t);
            return tmpAlloc[0];
        }

        for (int i = 0; i < n; i++) tmp[i] = pts[i];
        for (int k = 1; k < n; k++)
            for (int i = 0; i < n - k; i++)
                tmp[i] = Vector3.Lerp(tmp[i], tmp[i + 1], t);
        return tmp[0];
    }

}