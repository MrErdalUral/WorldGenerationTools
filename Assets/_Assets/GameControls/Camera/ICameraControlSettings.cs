using UnityEngine;

namespace GameControls.Settings
{
    public interface ICameraControlSettings
    {
        float YawSpeed { get; }
        float PitchSpeed { get; }
        float ZoomSpeed { get; }
        Vector2 PitchAngleLimits { get; }
        Vector2 ZoomLimits { get; }
    }
}