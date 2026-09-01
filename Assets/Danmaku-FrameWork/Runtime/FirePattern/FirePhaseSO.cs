using System;
using UnityEngine;
[CreateAssetMenu(menuName = "STG/Fire Phase")]
public class FirePhaseSO: ScriptableObject
{
    [Header("时间")]
    [Range(0,10)]
    public float duration=3f;//状态持续时间,0为无限,直到状态切换
    [Header("发射形状")]
    public FireShape shape;
    [Range(1,36)]
    public int bulletCount=12;
    [Range(10,360)]
    public float angleSpread =30f;
    [Range(-180,180)]
    public float offsetAngle =0f;
    [Header("瞄准模式")]
    public AimType aimType;
    [Header("子弹")]
    public GameObject bulletPrefab;      // 子弹预制体
    public BulletDefinitionSO bulletDefinition; // 运动参数 SO（speed, curve, duration...）
    [Header("发射节奏")]
    [Range(0.01f,10)]
    public float fireInterval=0.5f;
    [Range(0f,10f)]
    public float rotationSpeed=0f;
    public bool randomizeAngle=false;
    [Range(0,10)]
    public float randomRange = 5f;
}
