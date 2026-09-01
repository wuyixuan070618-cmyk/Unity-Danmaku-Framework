using UnityEditor;
using UnityEngine;

internal static class BulletMovementPreviewUtility
{
    private const int MinimumSineSamples = 64;
    private const int SineSamplesPerCycle = 24;
    private const int MaximumSineSamples = 2048;
    private const float DefaultPreviewDuration = 3f;

    public static void DrawSinePath(
        BulletDefinitionSO definition,
        Vector3 origin,
        Vector2 direction)
    {
        if (definition == null ||
            definition.movementConfig == null ||
            definition.MovementType != BulletMovementType.Sine ||
            direction.sqrMagnitude < 1e-6f)
        {
            return;
        }

        SineMoveConfigSO config = definition.movementConfig as SineMoveConfigSO;
        if (config == null) return;

        Vector2 forwardDirection = direction.normalized;
        Vector2 perpendicularDirection = new Vector2(
            -forwardDirection.y,
            forwardDirection.x);
        float previewDuration = config.duration > 0f
            ? config.duration
            : DefaultPreviewDuration;
        int sampleCount = CalculateSineSamples(config, previewDuration);

        float initialWaveOffset = config.amplitude * Mathf.Sin(config.phase);
        Vector3 previous = origin
                         + (Vector3)(perpendicularDirection * initialWaveOffset);

        Color oldColor = Handles.color;
        Handles.color = Color.white;

        for (int i = 1; i <= sampleCount; i++)
        {
            float time = previewDuration * i / sampleCount;
            float forwardDistance = config.speed * time
                                  + 0.5f * config.accel * time * time;
            float waveOffset = config.amplitude * Mathf.Sin(
                2f * Mathf.PI * config.frequency * forwardDistance + config.phase);
            Vector3 current = origin
                            + (Vector3)(forwardDirection * forwardDistance)
                            + (Vector3)(perpendicularDirection * waveOffset);

            Handles.DrawLine(previous, current, 1.5f);
            previous = current;
        }

        Handles.color = oldColor;
    }

    private static int CalculateSineSamples(SineMoveConfigSO config, float duration)
    {
        float endDistance = config.speed * duration
                          + 0.5f * config.accel * duration * duration;
        float traveledDistance = Mathf.Abs(endDistance);

        if (Mathf.Abs(config.accel) > 1e-6f)
        {
            float reversalTime = -config.speed / config.accel;
            if (reversalTime > 0f && reversalTime < duration)
            {
                float reversalDistance = config.speed * reversalTime
                                       + 0.5f * config.accel * reversalTime * reversalTime;
                traveledDistance = Mathf.Abs(reversalDistance)
                                 + Mathf.Abs(endDistance - reversalDistance);
            }
        }

        float cycleCount = Mathf.Abs(config.frequency) * traveledDistance;
        return Mathf.Clamp(
            Mathf.CeilToInt(cycleCount * SineSamplesPerCycle),
            MinimumSineSamples,
            MaximumSineSamples);
    }
}
