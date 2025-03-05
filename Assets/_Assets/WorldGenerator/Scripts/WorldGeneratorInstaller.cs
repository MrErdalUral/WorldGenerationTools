using FourWinged.PoissonGraph;
using FourWinged.PoissonGraph.Settings;
using FourWinged.SpatialGrid;
using UnityEngine;
using Zenject;

namespace FourWinged.WorldGenerator.Installers
{
    public class WorldGeneratorInstaller : MonoInstaller
    {
        [SerializeField] PoissonDiscSettings _poissonDiscSettings;
        public override void InstallBindings()
        {
            Container.BindInstance(_poissonDiscSettings).AsSingle();
            Container.BindInterfacesAndSelfTo<SpatialGrid<GraphNode>>().AsTransient().WithArguments(_poissonDiscSettings.MaxRadius*2, 16);
            Container.BindInterfacesAndSelfTo<PoissonDiscGraphModel>().AsSingle();
        }
    }
}