using GameControls.PlayerInput;
using R3;
using UnityEngine;
using Zenject;

namespace GameControls.Camera
{
    public class GameCameraPresenter : IInitializable
    {
        private readonly IPlayerInputView _playerInputView;
        private readonly IGameCameraView _gameCameraView;
        private readonly IGameCameraModel _gameCameraModel;
        private DisposableBag _disposableBag;

        public GameCameraPresenter(IPlayerInputView playerInputView, IGameCameraView gameCameraView, IGameCameraModel gameCameraModel)
        {
            _playerInputView = playerInputView;
            _gameCameraView = gameCameraView;
            _gameCameraModel = gameCameraModel;
        }

        public void Initialize()
        {
            _gameCameraModel.Position.Subscribe(position => _gameCameraView.Position = position).AddTo(ref _disposableBag);
            _gameCameraModel.Target.Subscribe(target => _gameCameraView.Target = target).AddTo(ref _disposableBag);
            _playerInputView.CameraInput.Subscribe(_gameCameraModel.MoveCamera).AddTo(ref _disposableBag);
            _playerInputView.AxisInput.Subscribe(input => _gameCameraModel.MoveCamera(input) ).AddTo(ref _disposableBag);
        }
    }
}