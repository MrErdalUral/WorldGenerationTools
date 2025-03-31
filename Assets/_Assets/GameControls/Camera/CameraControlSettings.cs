using UnityEngine;

namespace GameControls.Settings
{
    [CreateAssetMenu(fileName = "CameraControlSettings", menuName = "Camera Control/Settings")]
    public class CameraControlSettings : ScriptableObject, ICameraControlSettings
    {
        [SerializeField] private float _yawSpeed;
        [SerializeField] private float _pitchSpeed;
        [SerializeField] private float _zoomSpeed;
        [SerializeField] private Vector2 _pitchAngleLimits;
        [SerializeField] private Vector2 _zoomLimits;

        public float YawSpeed => _yawSpeed;
        public float PitchSpeed => _pitchSpeed;
        public float ZoomSpeed => _zoomSpeed;
        public Vector2 PitchAngleLimits => _pitchAngleLimits;
        public Vector2 ZoomLimits => _zoomLimits;
    }
}