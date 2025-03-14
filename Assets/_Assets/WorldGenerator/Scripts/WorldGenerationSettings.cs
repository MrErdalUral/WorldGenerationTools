using UnityEngine;

namespace FourWinged.WorldGenerator.Installers
{
    [CreateAssetMenu(fileName = "WorldGenerationSettings", menuName = "World Generator/Settings")]

    public class WorldGenerationSettings : ScriptableObject
    {
        public Vector2 WorldSize = new Vector2(100, 100);
        public float MinimumHeight = 10;
        public float MaximumHeight = -10;
        public float MaxSlope = 10;
    }
}