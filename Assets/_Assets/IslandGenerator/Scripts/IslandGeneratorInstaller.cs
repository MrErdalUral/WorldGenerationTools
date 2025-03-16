using Grids;
using Grids.SpatialGrid;
using PoissonDiscSampling;
using PoissonDiscSampling.Settings;
using IslandGenerator.Presenter;
using IslandGenerator.View;
using RandomNoise;
using TMPro;
using TriangleNet.Meshing.Algorithm;
using UnityEngine;
using Zenject;

namespace IslandGenerator.Installers
{
    public class IslandGeneratorInstaller : MonoInstaller
    {
        [SerializeField] private IslandView _islandViewPrefab;
        [SerializeField] private PoissonDiscSettings _poissonDiscSettings;
        [SerializeField] private Perlin2DSettings _perlinNoiseSettings;
        [SerializeField] private IslandGenerationSettings _islandGenerationSettings;
        public override void InstallBindings()
        {
            Random.InitState(0);

            Container.BindInterfacesAndSelfTo<PoissonDiscSettings>().FromInstance(_poissonDiscSettings).AsSingle();
            Container.BindInterfacesAndSelfTo<GridObject2D.Factory>().AsSingle();
            Container.BindInterfacesAndSelfTo<SpatialGrid2D>()
                .AsTransient()
                .WithArguments(_poissonDiscSettings.MaxRadius * 2, 16);

            Container.BindInterfacesAndSelfTo<PoissonDiscSampler>().AsSingle();

            Container.BindInstance(_perlinNoiseSettings).AsSingle();
            Container.BindInterfacesAndSelfTo<PerlinNoise2D>().AsSingle();

            Container.BindInterfacesAndSelfTo<IslandGenerator>().AsSingle();
            Container.BindFactory<IslandDto, IslandView, IslandView.Factory>()
                .FromPoolableMemoryPool<IslandDto, IslandView, IslandViewPool>(poolBinder => poolBinder
                    .FromComponentInNewPrefab(_islandViewPrefab));
            Container.BindInstance(_islandGenerationSettings).AsSingle();
            Container.BindInterfacesAndSelfTo<IslandPresenter>().AsSingle();
        }
    }
    class IslandViewPool : MonoPoolableMemoryPool<IslandDto, IMemoryPool, IslandView>
    {
    }
}