using R3;
using UnityEngine;

namespace GameControls.PlayerInput
{
    public interface IPlayerInputView
    {
        ReactiveProperty<Vector3> CameraInput { get; }
        ReactiveProperty<Vector2> AxisInput { get; }
        Observable<Unit> GetOrCreateKeyDownObservable(KeyCode key);
    }
}