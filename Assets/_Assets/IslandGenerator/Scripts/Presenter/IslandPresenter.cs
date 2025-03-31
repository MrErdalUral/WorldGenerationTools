using System;
using Cysharp.Threading.Tasks;
using GameControls.PlayerInput;
using IslandGenerator.Installers;
using IslandGenerator.Settings;
using IslandGenerator.View;
using R3;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace IslandGenerator.Presenter
{
    public class IslandPresenter : IInitializable, IDisposable
    {
        private readonly IslandView.Factory _islandViewFactory;
        private readonly IIslandGenerationSettings _islandGenerationSettings;
        private readonly IIslandGenerator _islandGenerator;
        private readonly IPlayerInputView _playerInputView;
        private readonly IRandomizeButtonInputView _randomizeButtonInputView;

        private DisposableBag _disposableBag;
        private DisposableBag _islandsDisposableBag;

        public IslandPresenter(IslandView.Factory islandViewFactory,
            IIslandGenerationSettings islandGenerationSettings,
            IIslandGenerator islandGenerator, 
            IPlayerInputView playerInputView, 
            IRandomizeButtonInputView randomizeButtonInputView)
        {
            _islandViewFactory = islandViewFactory;
            _islandGenerationSettings = islandGenerationSettings;
            _islandGenerator = islandGenerator;
            _playerInputView = playerInputView;
            _randomizeButtonInputView = randomizeButtonInputView;
        }

        public void Initialize()
        {
            _islandsDisposableBag.AddTo(ref _disposableBag);

            CreateIslandView(_islandGenerator.GenerateIsland(_islandGenerationSettings));

            _playerInputView.GetOrCreateKeyDownObservable(KeyCode.Space)
                .Merge(_randomizeButtonInputView.RandomizeButtonClicked)
                .Subscribe(_ => CreateIslandView(_islandGenerator.GenerateIsland(_islandGenerationSettings, Random.Range(int.MinValue, int.MaxValue))))
                .AddTo(ref _disposableBag);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }

        private void CreateIslandView(IslandDto islandDto)
        {
            _islandsDisposableBag.Clear();//Clear old island view(s)

            var islandView = _islandViewFactory.Create(islandDto);
            islandView.AddTo(ref _islandsDisposableBag);
            islandView.transform.position = Vector3.zero;
        }
    }
}
