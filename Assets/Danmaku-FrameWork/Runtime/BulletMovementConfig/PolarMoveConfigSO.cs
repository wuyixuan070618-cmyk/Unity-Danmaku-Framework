using UnityEngine;

[CreateAssetMenu(fileName ="PolarMove",menuName ="STG/MovementPattern/Polar")]
public class PolarMoveConfigSO : MovementConfigSO
{
    public override BulletMovementType Type => BulletMovementType.Polar;
    [Header("初始状态")]
    public float initialRadius=0f;
    public float initialAngle=0f;
    public float radialSpeed = 5f;
    public float angularSpeed = 90f;
    [Space(10)]
    [Header("加速度")]
    public float radialAccel=0f;
    public float angularAccel=0f;
}