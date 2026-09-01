using UnityEngine;
[CreateAssetMenu(fileName ="SineMove",menuName ="STG/MovementPattern/Sine")]
public class SineMoveConfigSO:MovementConfigSO
{
    public override BulletMovementType Type => BulletMovementType.Sine;
    public float amplitude = 1f;
    public float phase = 0f;
    public float frequency=1f;
    public float speed =5f;
    public float accel=0f;
    public float duration=1f;
}