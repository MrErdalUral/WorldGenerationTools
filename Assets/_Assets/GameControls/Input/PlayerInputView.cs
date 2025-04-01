using System.Collections;
using System.Collections.Generic;
using GameControls.Settings;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameControls.PlayerInput
{
    public class PlayerInputView : MonoBehaviour, IPlayerInputView, IRandomizeButtonInputView
    {
        [Inject] private readonly IInputSettings _inputSettings;
        private readonly Dictionary<KeyCode, Observable<Unit>> _keyDownSubjects = new Dictionary<KeyCode, Observable<Unit>>();
        [SerializeField] private Button _randomizeButton;
        private Vector2 _mousePosition;

        public ReactiveProperty<Vector3> CameraInput { get; } = new ReactiveProperty<Vector3>();
        public ReactiveProperty<Vector2> AxisInput { get; } = new ReactiveProperty<Vector2>();
        public Observable<Unit> RandomizeButtonClicked => _randomizeButton.OnClickAsObservable();

        // Update is called once per frame
        void Update()
        {
            var input = new Vector3();

            if (Input.GetMouseButtonDown(0))
                _mousePosition = Input.mousePosition;

            if (Input.GetMouseButton(0))
                input = (Input.mousePosition - (Vector3)_mousePosition) * Time.deltaTime;

            input += Input.mouseScrollDelta.y * Time.deltaTime * Vector3.forward;

            CameraInput.Value = input;
            var axisInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            AxisInput.Value = axisInput.normalized * _inputSettings.KeyBoardAxisMagnitude;
            if (axisInput.sqrMagnitude != 0)
                AxisInput.ForceNotify();
            _mousePosition = Input.mousePosition;
        }

        public Observable<Unit> GetOrCreateKeyDownObservable(KeyCode key)
        {
            if (_keyDownSubjects.TryGetValue(key, out var subject)) return subject;

            subject = Observable.EveryUpdate().Where(_ => Input.GetKeyDown(key));
            _keyDownSubjects.Add(key, subject);
            return subject;
        }

    }
}
