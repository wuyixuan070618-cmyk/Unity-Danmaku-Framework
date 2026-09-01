using UnityEngine;
[CreateAssetMenu(fileName ="SniperMove",menuName ="STG/MovementPattern/Sniper")]
public class SniperMoveConfigSO : MovementConfigSO
{
    public override BulletMovementType Type=>BulletMovementType.Sniper;
    public float speed = 5f;
    public float accel = 0f;
    public float duration=1f;
}