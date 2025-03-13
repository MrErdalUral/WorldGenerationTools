using UnityEngine;

namespace FourWinged.PoissonGraph.Settings
{
    public interface IPoissonDiscSettings
    {
        Vector2 RegionSize {get;}
        float MinRadius { get;}
        float MaxRadius { get;}
        int NumSamplesBeforeRejection { get;}
        int DensitySamples { get; }
        float VisualizationDelay { get; }
    }
}