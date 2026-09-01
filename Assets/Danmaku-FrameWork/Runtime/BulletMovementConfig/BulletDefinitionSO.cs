using System;
using UnityEngine;
public enum BulletMovementType { Linear,Polar,Sine,Bezier,Sniper,Laser}
[CreateAssetMenu(fileName ="BulletDef",menuName="STG/Bullet Definition")]
public class BulletDefinitionSO : ScriptableObject 
{
    [Header("Common Attributes")]
    public Sprite sprite;
    public Color tintColor=Color.white;
    public float damage=10f;
    public float collisionRadius = 0.5f;
    public int layer;
    public bool alignWithMovement;
    public float spriteAngleOffset;

    [Header("Movement Logic")]
    public MovementConfigSO movementConfig;
    public BulletMovementType MovementType => movementConfig!=null?movementConfig.Type:throw new InvalidOperationException("未给BulletMovementConfig设置BulletMovementType");
}