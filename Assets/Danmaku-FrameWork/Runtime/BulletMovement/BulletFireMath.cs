using UnityEngine;
public static class BulletFireMath
{


    public static Vector2 CalcBulletDirection(FireShape shape, Vector2 baseDir, int index, int total,float angleSpread,float rotationAccum)
    {
        return shape switch
        {
            FireShape.Fan => FanDir(baseDir, index, total, angleSpread,rotationAccum),
            FireShape.Circle => CircleDir(baseDir,index, total,rotationAccum),
            FireShape.Line => baseDir,   // 直线：全部同一方向，靠 spawnPos 区分
            _ => baseDir
        };
    }
    static Vector2 FanDir(Vector2 _base, int idx, int total, float spread,float rotationAccum)
    {
        float half = (total - 1) / 2f;
        float angle = (idx - half) * (spread / Mathf.Max(1, total - 1));
        return Quaternion.Euler(0, 0, angle+rotationAccum) * _base;
    }

    static Vector2 CircleDir(Vector2 baseDir,int idx, int total,float rotationAccum)
    {
        float angle = idx * 360f / total;
        return Quaternion.Euler(0, 0, angle + rotationAccum) * baseDir;
    }

    public static Vector2 ApplyRandom(FirePhaseSO phase, Vector2 dir)
    {
        if (!phase.randomizeAngle) return dir;
        float r = UnityEngine.Random.Range(-phase.randomRange, phase.randomRange);
        return Quaternion.Euler(0, 0, r) * dir;
    }

}