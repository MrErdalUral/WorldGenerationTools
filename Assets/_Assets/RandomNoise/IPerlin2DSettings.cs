using UnityEngine;

namespace RandomNoise
{
    public interface IPerlin2DSettings
    {
        int Seed { get; }
        float Scale { get; }
        int Octaves { get; }
        Vector2 Offset { get; }
    }
}