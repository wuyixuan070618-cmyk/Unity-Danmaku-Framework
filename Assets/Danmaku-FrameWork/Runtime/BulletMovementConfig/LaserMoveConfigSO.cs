using UnityEngine;
[CreateAssetMenu(fileName = "LaserMove",menuName ="STG/MovementPattern/Laser")]
public class LaserMoveConfigSO : MovementConfigSO
{
    public override BulletMovementType Type => BulletMovementType.Laser;
    public float warningDuration=1f;
    public float growDuration=0.5f;
    public float activeDuration=2f;
    public float fadingDuration=0.5f;
    public float maxLength=10f;
    public float maxWidth=0.5f; 
    public bool animateLength=false;
    public bool animateWidth=true;
    public float initialLength=0f;
    public float initialWidth=0.001f;
}