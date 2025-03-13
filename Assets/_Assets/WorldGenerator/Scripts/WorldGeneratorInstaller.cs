using FourWinged.Grids;
using FourWinged.Grids.SpatialGrid;
using FourWinged.PoissonGraph;
using FourWinged.PoissonGraph.Settings;
using FourWinged.WorldGenerator.Presenter;
using FourWinged.WorldGenerator.View;
using RandomNoise;
using TMPro;
using TriangleNet.Meshing.Algorithm;
using UnityEngine;
using Zenject;

namespace FourWinged.WorldGenerator.Installers
{
    public class WorldGeneratorInstaller : MonoInstaller
    {
        [SerializeField] private WorldGeneratorView _worldGeneratorView;
        [SerializeField] private PoissonDiscSettings _poissonDiscSettings;
        [SerializeField] private Perlin2DSettings _perlinNoiseSettings;
        [SerializeField] private WorldGenerationSettings _worldGenerationSettings;
        public override void InstallBindings()
        {
            Random.InitState(0);
            Container.BindInstance(_worldGeneratorView).AsSingle();

            Container.BindInstance(_poissonDiscSettings).AsSingle();
            Container.BindInterfacesAndSelfTo<GridObject2D.Factory>().AsSingle();
            Container.BindInterfacesAndSelfTo<SpatialGrid2D<IGridObject2D>>()
                .AsTransient()
                .WithArguments(_poissonDiscSettings.MaxRadius * 2, 16);

            Container.BindInterfacesAndSelfTo<PoissonDiscGraphModel<IGridObject2D>>().AsSingle();

            Container.BindInstance(_perlinNoiseSettings).AsSingle();
            Container.BindInterfacesAndSelfTo<PerlinNoise2D>().AsSingle();

            Container.BindInstance(_worldGenerationSettings).AsSingle();
            Container.BindInterfacesAndSelfTo<WorldGeneratorPresenter>().AsSingle();
        }
    }
}