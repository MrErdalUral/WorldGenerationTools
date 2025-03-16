using System.Collections.Generic;
using UnityEngine;

namespace RandomNoise
{
    public class PerlinNoise2D : INoise2D
    {
        private readonly Dictionary<Vector2Int, float> _cache;
        private readonly IPerlin2DSettings _settings;
        private readonly Vector2[] _octaveOffsets;
        public PerlinNoise2D(IPerlin2DSettings settings)
        {
            _settings = settings;
            _cache = new Dictionary<Vector2Int, float>();
            _octaveOffsets = new Vector2[_settings.Octaves];
            SetSeed(settings.Seed);
        }

        public float GetValue(float x, float y)
        {
            if (_settings.Octaves < 1) return 0.5f;
            float maxValue = 0;
            float totalValue = 0;
            for (int o = 0; o < _settings.Octaves; o++)
            {
                var octaveMagnitude = Mathf.Pow(2, o);
                maxValue += octaveMagnitude;
                totalValue += Mathf.PerlinNoise((x + _settings.Offset.x + _octaveOffsets[o].x) * _settings.Scale,
                                  (y + _settings.Offset.y + _octaveOffsets[o].y) * _settings.Scale)
                              * octaveMagnitude;
            }
            totalValue /= maxValue;
            return totalValue;
        }

        public void SetSeed(int seed)
        {
            var random = new System.Random(seed);
            for (int o = 0; o < _octaveOffsets.Length; o++)
            {
                _octaveOffsets[o] = new Vector2(random.Next(int.MinValue, int.MaxValue) / (float)int.MaxValue, random.Next(int.MinValue, int.MaxValue) / (float)int.MaxValue) * 10000f;
            }
        }
    }
}