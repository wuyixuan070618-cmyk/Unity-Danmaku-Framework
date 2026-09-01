using UnityEngine;
[CreateAssetMenu(fileName ="BezierMove",menuName ="STG/MovementPattern/Bezier")]
public class BezierMoveConfigSO : MovementConfigSO
{
    public override BulletMovementType Type => BulletMovementType.Bezier;
    public float duration = 1f;
    public Vector3[] controlPoints=new Vector3[] { Vector3.zero, Vector3.right * 2f };
    public AnimationCurve easing=AnimationCurve.Linear(0,0,1,1);
}