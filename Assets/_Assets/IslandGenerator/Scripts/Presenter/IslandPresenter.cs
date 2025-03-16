using System;
using Cysharp.Threading.Tasks;
using IslandGenerator.Installers;
using IslandGenerator.View;
using R3;
using UnityEngine;
using Zenject;

namespace IslandGenerator.Presenter
{
    public class IslandPresenter : IInitializable, IDisposable
    {
        private readonly IslandView.Factory _islandViewFactory;
        private readonly IslandGenerationSettings _islandGenerationSettings;
        private readonly IIslandGenerator _islandGenerator;

        private DisposableBag _disposableBag;

        public IslandPresenter(IslandView.Factory islandViewFactory, IslandGenerationSettings islandGenerationSettings, IIslandGenerator islandGenerator)
        {
            _islandViewFactory = islandViewFactory;
            _islandGenerationSettings = islandGenerationSettings;
            _islandGenerator = islandGenerator;
        }

        public void Initialize()
        {
            _islandGenerator
                .OnIslandGenerated
                .Take(1)
                .Subscribe(CreateIslandView)
                .AddTo(ref _disposableBag);

            _islandGenerator.GenerateIsland(_islandGenerationSettings).Forget();
        }

        private void CreateIslandView(IslandDto islandDto)
        {
            var islandView = _islandViewFactory.Create(islandDto);
            islandView.AddTo(ref _disposableBag);
            islandView.transform.position = Vector3.zero;
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
