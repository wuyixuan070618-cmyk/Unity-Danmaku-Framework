using UnityEngine;

[CreateAssetMenu(fileName = "LinearMove",menuName = "STG/MovementPattern/Linear")]
public class LinearMoveConfigSO : MovementConfigSO
{
    public override BulletMovementType Type => BulletMovementType.Linear;
    public float speed = 5f;
    public float accel=0f;
    public float duration=5f;
}