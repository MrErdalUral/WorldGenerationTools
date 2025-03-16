using PoissonDiscSampling.Settings;
using UnityEngine;

namespace IslandGenerator.Settings
{
    [CreateAssetMenu(fileName = "IslandGenerationSettings", menuName = "World Generator/Settings")]

    public class IslandGenerationSettings : ScriptableObject, IIslandGenerationSettings
    {
        [SerializeField] private int _seed;
        [SerializeField] private Material _islandMaterial;
        [SerializeField] private Vector2 _worldSize = new Vector2(100, 100);
        [Tooltip("Better to keep this value 0 for now because of the triangle.net setup")]
        [SerializeField]
        private float _minimumHeight = 0;
        [SerializeField]
        private float _maximumHeight = 30;
        [SerializeField] private float _maxSlope = 10;
        [SerializeField] private PoissonDiscSettings _poissonSettings;

        public int Seed => _seed;
        public Material IslandMaterial => _islandMaterial;
        public Vector2 WorldSize => _worldSize;
        public float MinimumHeight => _minimumHeight;
        public float MaximumHeight => _maximumHeight;
        public float MaxSlope => _maxSlope;
        public IPoissonDiscSettings PoissonDiscSettings => _poissonSettings;
    }
}