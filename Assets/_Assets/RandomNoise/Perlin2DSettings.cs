using UnityEngine;

namespace RandomNoise
{
    [CreateAssetMenu(fileName = "Perlin2DSettings", menuName = "Perlin2D/Settings")]
    public class Perlin2DSettings : ScriptableObject, IPerlin2DSettings
    {
        [SerializeField] private int _seed;
        [SerializeField] private float _scale;
        [SerializeField] private int _octaves;
        [SerializeField] private Vector2 _offset;

        public int Seed => _seed;
        public float Scale => _scale;
        public int Octaves => _octaves;
        public Vector2 Offset => _offset;
    }
}