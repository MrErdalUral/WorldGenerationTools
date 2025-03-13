using UnityEngine;
using Mesh = UnityEngine.Mesh;

namespace FourWinged.WorldGenerator.View
{
    public class WorldGeneratorView : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;

        public Mesh WorldMesh
        {
            set => _meshFilter.mesh = value;
        }
    }
}