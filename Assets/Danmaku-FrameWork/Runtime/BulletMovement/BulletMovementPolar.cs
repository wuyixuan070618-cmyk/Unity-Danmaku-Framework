using System;
using UnityEngine;

public class BulletMovementPolar : BulletMovementBase
{
    private PolarMoveConfigSO _config;
    private RuntimeState _state;

    protected override void Move(float deltaTime)
    {
        _state.Angle += _state.AngularSpeed * deltaTime;
        _state.AngularSpeed += _state.AngularAcceleration * deltaTime;
        _state.Radius += _state.RadialSpeed * deltaTime;
        _state.RadialSpeed += _state.RadialAcceleration * deltaTime;

        Vector3 offset = new Vector3(
            Mathf.Cos(_state.Angle),
            Mathf.Sin(_state.Angle),
            0f) * _state.Radius;

        transform.position = context.position + offset;
    }

    protected override void OnInitialize(BulletDefinitionSO definition, BulletSpawnContext spawnContext)
    {
        _config = definition?.movementConfig as PolarMoveConfigSO;
        if (_config == null)
        {
            throw new InvalidOperationException(definition.name);
        }

        _state.Angle = _config.initialAngle * Mathf.Deg2Rad;
        _state.Radius = _config.initialRadius;
        _state.AngularSpeed = _config.angularSpeed * Mathf.Deg2Rad;
        _state.RadialSpeed = _config.radialSpeed;
        _state.AngularAcceleration = _config.angularAccel * Mathf.Deg2Rad;
        _state.RadialAcceleration = _config.radialAccel;

        float directionAngleRadians = Mathf.Atan2(
            spawnContext.direction.y,
            spawnContext.direction.x);

        float initialAngleRadians = _config.initialAngle * Mathf.Deg2Rad + directionAngleRadians;
        Vector3 initialOffset = new Vector3(
            Mathf.Cos(initialAngleRadians),
            Mathf.Sin(initialAngleRadians),
            0f) * _config.initialRadius;

        transform.position = spawnContext.position + initialOffset;
        _state.Angle+=directionAngleRadians;
    }

    private struct RuntimeState
    {
        public float Angle;
        public float Radius;
        public float AngularSpeed;
        public float RadialSpeed;
        public float AngularAcceleration;
        public float RadialAcceleration;
    }
}
