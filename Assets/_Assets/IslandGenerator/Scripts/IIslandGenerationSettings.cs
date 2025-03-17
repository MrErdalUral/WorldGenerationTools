using PoissonDiscSampling.Settings;
using UnityEngine;

namespace IslandGenerator.Settings
{
    public interface IIslandGenerationSettings
    {
        int Seed { get; }
        Material IslandMaterial { get; }
        Vector2 WorldSize { get; }
        float MinimumHeight { get; }
        float MaximumHeight { get; }
        float MaxSlope { get; }
        IPoissonDiscSettings PoissonDiscSettings { get; }
        float HeightFalloffSigma { get; }
    }
}