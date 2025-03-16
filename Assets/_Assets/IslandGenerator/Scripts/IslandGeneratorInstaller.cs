using Grids;
using Grids.SpatialGrid;
using PoissonDiscSampling;
using IslandGenerator.Presenter;
using IslandGenerator.Settings;
using IslandGenerator.View;
using RandomNoise;
using UnityEngine;
using Zenject;

namespace IslandGenerator.Installers
{
    public class IslandGeneratorInstaller : MonoInstaller
    {
        [SerializeField] private IslandView _islandViewPrefab;
        [SerializeField] private Perlin2DSettings _perlinNoiseSettings;
        [SerializeField] private IslandGenerationSettings _islandGenerationSettings;
        public override void InstallBindings()
        {
            Random.InitState(0);

            Container.BindInterfacesAndSelfTo<GridObject2D.Factory>().AsSingle();
            Container.BindInterfacesAndSelfTo<SpatialGrid2D>()
                .AsTransient()
                .WithArguments(_islandGenerationSettings.PoissonDiscSettings.MaxRadius * 2, 16);

            Container.BindInterfacesAndSelfTo<PoissonDiscSampler>().AsSingle();

            Container.BindInterfacesAndSelfTo<Perlin2DSettings>().FromInstance(_perlinNoiseSettings).AsSingle();
            Container.BindInterfacesAndSelfTo<PerlinNoise2D>().AsSingle();

            Container.BindInterfacesAndSelfTo<IslandGenerator>().AsSingle();
            Container.BindFactory<IslandDto, IslandView, IslandView.Factory>()
                .FromPoolableMemoryPool<IslandDto, IslandView, IslandViewPool>(poolBinder => poolBinder
                    .FromComponentInNewPrefab(_islandViewPrefab));
            Container.BindInterfacesAndSelfTo<IslandGenerationSettings>().FromInstance(_islandGenerationSettings).AsSingle();
            Container.BindInterfacesAndSelfTo<IslandPresenter>().AsSingle();
        }
    }
    class IslandViewPool : MonoPoolableMemoryPool<IslandDto, IMemoryPool, IslandView>
    {
    }
}