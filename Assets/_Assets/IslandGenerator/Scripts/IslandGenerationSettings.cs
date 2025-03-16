using PoissonDiscSampling.Settings;
using UnityEngine;

namespace IslandGenerator.Installers
{
    [CreateAssetMenu(fileName = "IslandGenerationSettings", menuName = "World Generator/Settings")]

    public class IslandGenerationSettings : ScriptableObject
    {
        public Vector2 WorldSize = new Vector2(100, 100);
        public float MinimumHeight = 10;
        public float MaximumHeight = -10;
        public float MaxSlope = 10;
        public PoissonDiscSettings PoissonSettings;
    }
}