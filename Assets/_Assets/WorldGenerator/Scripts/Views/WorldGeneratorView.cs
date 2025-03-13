using FourWinged.PoissonGraph;
using FourWinged.PoissonGraph.Settings;
using RandomNoise;
using UnityEngine;
using Zenject;
using Mesh = UnityEngine.Mesh;

namespace FourWinged.WorldGenerator.View
{
    public class WorldGeneratorView : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private bool _drawNodeRadiuses;
        [Inject] private IPoissonDiscSampler _discSampler;
        [Inject] private IPoissonDiscSettings _settings;

        public Mesh WorldMesh
        {
            set => _meshFilter.mesh = value;
        }

        void OnDrawGizmos()
        {
            if (_drawNodeRadiuses && _discSampler != null && _meshFilter.mesh != null)
            {
                for (var i = 0; i < _discSampler.NodeGraph.Nodes.Count; i++)
                {
                    var node = _discSampler.NodeGraph.Nodes[i];
                    Gizmos.color = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(_settings.MinRadius, _settings.MaxRadius, node.Radius));
                    Gizmos.DrawWireSphere(_meshFilter.mesh.vertices[i], node.Radius);
                }
            }
        }
    }
}