using Grids;
using PoissonDiscSampling;
using UnityEngine;
using Zenject;
using Mesh = UnityEngine.Mesh;

namespace IslandGenerator.View
{
    public class IslandView : MonoBehaviour, IIslandView
    {
        [SerializeField] private MeshFilter _meshFilter;

        private IMemoryPool _pool;
        private Mesh _mesh;

        [Inject] private readonly IPoissonDiscSampler _sampler;

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
        }


        public void Dispose()
        {
            _pool.Despawn(this);
        }

        public class Factory : PlaceholderFactory<IslandDto, IslandView>, IIslandViewFactory
        {

        }

        public Vector3 Position
        {
            set => transform.position = value;
        }
    }

    public interface IIslandViewFactory : IFactory<IslandDto, IslandView> { }
}