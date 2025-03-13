using UnityEngine;

namespace FourWinged.WorldGenerator.Installers
{
    [CreateAssetMenu(fileName = "WorldGenerationSettings", menuName = "World Generator/Settings")]

    public class WorldGenerationSettings : ScriptableObject
    {
        public Vector2 WorldSize;
        public float MinimumHeight;
        public float MaximumHeight;
    }
}