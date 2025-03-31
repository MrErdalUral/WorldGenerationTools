using GameControls.Settings;
using R3;
using UnityEngine;
using Zenject;

namespace GameControls.Camera
{
    public class OrbitalGameCameraModel : IGameCameraModel, IInitializable, ITickable
    {
        private readonly ICameraControlSettings _settings;

        private readonly ReactiveProperty<Vector3> _position = new ReactiveProperty<Vector3>();
        private readonly ReactiveProperty<Vector3> _target = new ReactiveProperty<Vector3>();
        private Vector3 _direction;
        private float _yaw;
        private float _pitch;
        private float _zoom;
        private Vector3 _targetPosition;

        public OrbitalGameCameraModel(ICameraControlSettings settings)
        {
            _settings = settings;
        }

        public ReadOnlyReactiveProperty<Vector3> Position => _position;
        public ReadOnlyReactiveProperty<Vector3> Target => _target;

        public void SetTarget(Vector3 target)
        {
            _target.Value = target;
        }

        public void MoveCamera(Vector3 movementDelta)
        {
            _yaw += movementDelta.x * _settings.YawSpeed;
            _pitch = Mathf.Clamp(_pitch + movementDelta.y * _settings.PitchSpeed, _settings.PitchAngleLimits.x, _settings.PitchAngleLimits.y);
            _zoom = Mathf.Clamp(_zoom + movementDelta.z * _settings.ZoomSpeed, _settings.ZoomLimits.x, _settings.ZoomLimits.y);
            UpdateDirection();
            UpdatePosition();
        }

        public void Initialize()
        {
            _pitch = _settings.PitchAngleLimits.x;
            _zoom = _settings.ZoomLimits.x;
            _target.Value = Vector3.up;
            UpdateDirection();
            UpdatePosition();
        }

        /// <summary>
        /// Calculates the normalized direction vector based on yaw and pitch angles.
        /// </summary>
        private void UpdateDirection()
        {
            float yawRad = _yaw * Mathf.Deg2Rad;
            float pitchRad = _pitch * Mathf.Deg2Rad;

            float cosPitch = Mathf.Cos(pitchRad);
            float x = cosPitch * Mathf.Sin(yawRad);
            float y = Mathf.Sin(pitchRad);
            float z = cosPitch * Mathf.Cos(yawRad);

            _direction = new Vector3(x, y, z).normalized;
        }

        /// <summary>
        /// Positions the camera based on the focus center, computed direction, and distance.
        /// </summary>
        private void UpdatePosition()
        {
            _targetPosition = _target.Value + _direction * _zoom;
        }

        public void Tick()
        {
            _position.Value = Vector3.Lerp(_position.Value, _targetPosition, Time.deltaTime * 10);
        }
    }
}