using UnityEngine;
using System.Collections.Generic;
using Zenject;
using System.Linq;
using FourWinged.PoissonGraph;
using FourWinged.PoissonGraph.Settings;
using R3;
using Mesh = UnityEngine.Mesh;

namespace FourWinged.WorldGenerator.Views
{
    public class WorldGeneratorView : MonoBehaviour
    {
        [Inject] private IGraphModel _graphModel;
        [Inject] private PoissonDiscSettings _settings;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private bool _showEdges = false;
        [SerializeField] private bool _showNodes = false;
        [SerializeField] private bool _showPerlinNoise = false;
        private Mesh _mesh;
        private Graph _graph => _graphModel.Graph;

        void Start()
        {
           
        }


        void OnDrawGizmos()
        {
            if (_graphModel == null) return;
            if (_graph == null) return;
            if (_graph.Nodes == null) return;
            if (_graph.Edges == null) return;
            if (_settings == null) return;

            if (_showPerlinNoise)
            {
                var halfSize = _settings.RegionSize / 2;
                for (int x = 0; x < _settings.RegionSize.x; x++)
                {
                    for (int y = 0; y < _settings.RegionSize.y; y++)
                    {
                        Gizmos.color = Color.Lerp(Color.black, Color.white,
                            Mathf.PerlinNoise((x - halfSize.x + _settings.PerlinOffset.x) * _settings.PerlinScale,
                                (y - halfSize.y + _settings.PerlinOffset.y) * _settings.PerlinScale));
                        Gizmos.DrawCube(new Vector3(x, -3, y) - new Vector3(halfSize.x, 0, halfSize.y), Vector3.one);
                    }
                }
            }

            if (_showNodes)
                foreach (GraphNode node in _graph.Nodes)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(node.Position + transform.position, node.Radius);
                }

            if (_showEdges)
                foreach (var connection in _graph.Edges)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(_graph.Nodes[connection.Item1].Position + transform.position,
                        _graph.Nodes[connection.Item2].Position + transform.position);
                }
        }
    }
}