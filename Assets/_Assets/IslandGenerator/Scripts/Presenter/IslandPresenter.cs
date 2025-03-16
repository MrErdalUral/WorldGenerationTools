using System;
using Cysharp.Threading.Tasks;
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

        private DisposableBag _disposableBag;
        private DisposableBag _islandsDisposableBag;

        public IslandPresenter(IslandView.Factory islandViewFactory, IIslandGenerationSettings islandGenerationSettings, IIslandGenerator islandGenerator)
        {
            _islandViewFactory = islandViewFactory;
            _islandGenerationSettings = islandGenerationSettings;
            _islandGenerator = islandGenerator;
        }

        public void Initialize()
        {
            _islandsDisposableBag.AddTo(ref _disposableBag);

            CreateIslandView(_islandGenerator.GenerateIsland(_islandGenerationSettings));

            Observable.EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Space))
                .Subscribe(_ =>
                {
                    ClearIslands();
                    CreateIslandView(_islandGenerator.GenerateIsland(_islandGenerationSettings, Random.Range(int.MinValue, int.MaxValue)));
                })
                .AddTo(ref _disposableBag);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }

        private void CreateIslandView(IslandDto islandDto)
        {
            var islandView = _islandViewFactory.Create(islandDto);
            islandView.AddTo(ref _islandsDisposableBag);
            islandView.transform.position = Vector3.zero;
        }

        private void ClearIslands()
        {
            _islandsDisposableBag.Clear();
        }
    }
}
