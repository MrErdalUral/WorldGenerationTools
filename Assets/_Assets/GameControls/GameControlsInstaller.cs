using System.Collections;
using System.Collections.Generic;
using GameControls.Camera;
using GameControls.PlayerInput;
using GameControls.Settings;
using UnityEngine;
using Zenject;

namespace GameControls.Installers
{
    public class GameControlsInstaller : MonoInstaller
    {
        [SerializeField] private PlayerInputView _playerInputView;
        [SerializeField] private GameCameraView _gameCameraView;
        [SerializeField] private CameraControlSettings _cameraControlSettings;
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlayerInputView>().FromInstance(_playerInputView).AsSingle();
            Container.BindInterfacesAndSelfTo<CameraControlSettings>().FromInstance(_cameraControlSettings).AsSingle();
            Container.BindInterfacesAndSelfTo<OrbitalGameCameraModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameCameraView>().FromInstance(_gameCameraView).AsSingle();
            Container.BindInterfacesAndSelfTo<GameCameraPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<InputSettings>().FromInstance(new InputSettings(0.5f)).AsSingle();
        }
    }
}

