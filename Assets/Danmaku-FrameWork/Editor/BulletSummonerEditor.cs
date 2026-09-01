using UnityEditor;
using UnityEngine;

public static class BulletSummonerEditor
{
    private const float PreviewRadius = 1f;
    private const float DefaultPathDuration = 3f;

    [DrawGizmo(GizmoType.Selected)]
    private static void DrawSummonerPreview(BulletSummoner summoner, GizmoType gizmoType)
    {
        FireSequenceSO sequence = summoner.Sequence;
        if (sequence == null || sequence.phases == null || sequence.phases.Length == 0)
            return;

        int phaseIndex = Mathf.Clamp(summoner.CurrentPhaseIndex, 0, sequence.phases.Length - 1);
        FirePhaseSO phase = sequence.phases[phaseIndex];
        if (phase == null || phase.bulletPrefab == null)
            return;

        Vector3 origin = summoner.transform.position;
        Vector2 baseDirection = CalculateBaseDirection(summoner, phase);
        if (baseDirection.sqrMagnitude < 1e-6f)
            return;

        DrawBaseDirection(origin, baseDirection);

        if (phase.shape == FireShape.Fan)
        {
            DrawFan(origin, baseDirection, phase.angleSpread, PreviewRadius);
        }

        DrawBulletDirections(
            origin,
            baseDirection,
            phase,
            summoner.RotationAccum);

        DrawPathPreview(phase.bulletDefinition, origin, baseDirection);
    }

    private static Vector2 CalculateBaseDirection(BulletSummoner summoner, FirePhaseSO phase)
    {
        Vector2 direction;

        switch (phase.aimType)
        {
            case AimType.AimToTarget:
                Transform target = summoner.TargetTransform;
                direction = target == null
                    ? Vector2.down
                    : (Vector2)(target.position - summoner.transform.position);
                if (direction.sqrMagnitude < 1e-6f)
                    direction = Vector2.down;
                break;

            case AimType.OppositeToParent:
                Transform reference = summoner.AimReferenceOverride != null
                    ? summoner.AimReferenceOverride
                    : summoner.transform.parent;
                direction = reference == null
                    ? Vector2.down
                    : (Vector2)(summoner.transform.position - reference.position);
                if (direction.sqrMagnitude < 1e-6f)
                    direction = Vector2.down;
                break;

            case AimType.None:
                direction = summoner.transform.rotation * Vector2.right;
                break;

            default:
                direction = Vector2.right;
                break;
        }

        float offsetAngle = phase.offsetAngle + summoner.DefaultOffsetAngle;
        return Quaternion.Euler(0f, 0f, offsetAngle) * direction.normalized;
    }

    private static void DrawBaseDirection(Vector3 origin, Vector2 direction)
    {
        Vector3 tip = origin + (Vector3)direction * PreviewRadius;
        using (new Handles.DrawingScope(Color.green))
        {
            Handles.DrawLine(origin, tip, 2f);
            DrawArrowhead(tip, direction, 0.3f);
        }
    }

    private static void DrawArrowhead(Vector3 tip, Vector2 direction, float size)
    {
        Vector3 right = Quaternion.Euler(0f, 0f, 150f) * direction.normalized * size;
        Vector3 left = Quaternion.Euler(0f, 0f, -150f) * direction.normalized * size;
        Handles.DrawLine(tip, tip + right, 2f);
        Handles.DrawLine(tip, tip + left, 2f);
    }

    private static void DrawFan(
        Vector3 origin,
        Vector2 baseDirection,
        float spread,
        float radius)
    {
        const int stepCount = 20;
        float halfSpread = spread * 0.5f;
        Vector3 previous = origin;

        using (new Handles.DrawingScope(new Color(1f, 0f, 0f, 0.35f)))
        {
            for (int i = 0; i <= stepCount; i++)
            {
                float angle = -halfSpread + spread * i / stepCount;
                Vector3 current = origin
                                + (Vector3)(Quaternion.Euler(0f, 0f, angle)
                                * baseDirection * radius);
                Handles.DrawLine(origin, current);
                if (i > 0)
                    Handles.DrawLine(previous, current);
                previous = current;
            }
        }
    }

    private static void DrawBulletDirections(
        Vector3 origin,
        Vector2 baseDirection,
        FirePhaseSO phase,
        float rotationAccum)
    {
        int bulletCount = Mathf.Max(1, phase.bulletCount);

        for (int i = 0; i < bulletCount; i++)
        {
            Vector2 direction = BulletFireMath.CalcBulletDirection(
                phase.shape,
                baseDirection,
                i,
                bulletCount,
                phase.angleSpread,
                rotationAccum);
            Vector3 end = origin + (Vector3)direction * PreviewRadius;

            using (new Handles.DrawingScope(Color.yellow))
                Handles.DrawLine(origin, end, 1.5f);

            using (new Handles.DrawingScope(Color.blue))
                Handles.DrawWireDisc(end, Vector3.forward, 0.1f);
        }
    }

    private static void DrawPathPreview(
        BulletDefinitionSO definition,
        Vector3 origin,
        Vector2 direction)
    {
        if (definition == null || definition.movementConfig == null)
            return;

        using (new Handles.DrawingScope(Color.white))
        {
            switch (definition.MovementType)
            {
                case BulletMovementType.Linear:
                    Handles.DrawLine(origin, origin + (Vector3)direction * 5f, 1.5f);
                    break;

                case BulletMovementType.Bezier:
                    DrawBezierPath(definition.movementConfig as BezierMoveConfigSO, origin, direction);
                    break;

                case BulletMovementType.Sine:
                    BulletMovementPreviewUtility.DrawSinePath(definition, origin, direction);
                    break;

                case BulletMovementType.Polar:
                    DrawPolarPath(definition.movementConfig as PolarMoveConfigSO, origin, direction);
                    break;

                case BulletMovementType.Laser:
                    Handles.DrawWireCube(
                        origin + (Vector3)direction * 10f,
                        new Vector3(0.3f, 20f, 0f));
                    break;

                case BulletMovementType.Sniper:
                    Vector3 corner = origin + (Vector3)direction * 3f;
                    Handles.DrawLine(origin, corner, 1.5f);
                    Handles.DrawLine(corner, corner + Vector3.down * 3f, 1.5f);
                    break;
            }
        }
    }

    private static void DrawBezierPath(
        BezierMoveConfigSO config,
        Vector3 origin,
        Vector2 baseDirection)
    {
        if (config == null || config.controlPoints == null || config.controlPoints.Length < 2)
            return;

        Vector3[] rotatedPoints = (Vector3[])config.controlPoints.Clone();
        Vector2 normalizedDirection = baseDirection.normalized;
        Quaternion rotation = Vector3.Dot(Vector3.right, normalizedDirection) != -1f
            ? Quaternion.FromToRotation(Vector3.right, normalizedDirection)
            : Quaternion.AngleAxis(180f, Vector3.forward);

        for (int i = 0; i < rotatedPoints.Length; i++)
            rotatedPoints[i] = rotation * rotatedPoints[i];

        const int stepCount = 32;
        Vector3 previous = origin + rotatedPoints[0];
        for (int i = 1; i <= stepCount; i++)
        {
            float t = i / (float)stepCount;
            Vector3 current = origin + EvaluateBezier(rotatedPoints, t);
            Handles.DrawLine(previous, current, 1.5f);
            previous = current;
        }
    }

    private static void DrawPolarPath(
        PolarMoveConfigSO config,
        Vector3 origin,
        Vector2 direction)
    {
        if (config == null)
            return;

        const int stepCount = 64;
        float directionAngle = Mathf.Atan2(direction.y, direction.x);
        Vector3 previous = origin + new Vector3(config.initialRadius, 0f, 0f);

        for (int i = 1; i <= stepCount; i++)
        {
            float time = DefaultPathDuration * i / stepCount;
            float angle = config.initialAngle
                        + time * config.angularSpeed
                        + 0.5f * time * time * config.angularAccel;
            float radius = config.initialRadius
                         + time * config.radialSpeed
                         + 0.5f * time * time * config.radialAccel;
            float radians = directionAngle + angle * Mathf.Deg2Rad;
            Vector3 current = origin + new Vector3(
                Mathf.Cos(radians) * radius,
                Mathf.Sin(radians) * radius,
                0f);
            Handles.DrawLine(previous, current, 1.5f);
            previous = current;
        }
    }

    private static Vector3 EvaluateBezier(Vector3[] points, float t)
    {
        Vector3[] buffer = (Vector3[])points.Clone();
        for (int level = 1; level < buffer.Length; level++)
        {
            for (int i = 0; i < buffer.Length - level; i++)
                buffer[i] = Vector3.Lerp(buffer[i], buffer[i + 1], t);
        }

        return buffer[0];
    }
}
