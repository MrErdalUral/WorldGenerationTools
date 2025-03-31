using R3;
using UnityEngine;

namespace GameControls.Camera
{
    public interface IGameCameraModel
    {
        ReadOnlyReactiveProperty<Vector3> Position { get; }
        ReadOnlyReactiveProperty<Vector3> Target { get; }
        void SetTarget(Vector3 target);
        void MoveCamera(Vector3 movementDelta);
    }
}