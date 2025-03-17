using System;
using Grids;
using PoissonDiscSampling;
using UnityEngine;
using Zenject;
using Mesh = UnityEngine.Mesh;

namespace IslandGenerator.View
{
    public class IslandView : MonoBehaviour, IPoolable<IslandDto, IMemoryPool>, IDisposable
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private bool _showGizmos;

        private IMemoryPool _pool;
        private Mesh _mesh;

        private NodeGraph<IGridObject2D> _nodeGraph;
        [Inject]private readonly IPoissonDiscSampler _sampler;

        public void OnDespawned()
        {
            _pool = null;
        }

        public void OnSpawned(IslandDto islandDto, IMemoryPool pool)
        {
            _pool = pool;
            _mesh = new Mesh();
            _mesh.SetVertices(islandDto.IslandVertices);
            _mesh.SetTriangles(islandDto.IslandTriangles, 0);
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
            _meshFilter.mesh = _mesh;
            _nodeGraph = _sampler.NodeGraph;
        }

        void OnDrawGizmos()
        {
            if (!_showGizmos) return;
            foreach (var edge in _nodeGraph.Edges)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(_mesh.vertices[edge.Item1], _mesh.vertices[edge.Item2]);
            }

            foreach (var rootIndex in _nodeGraph.Roots)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(_mesh.vertices[rootIndex],2);
            }
        }

        public void Dispose()
        {
            _pool.Despawn(this);
        }

        public class Factory : PlaceholderFactory<IslandDto, IslandView>
        {

        }
    }
}