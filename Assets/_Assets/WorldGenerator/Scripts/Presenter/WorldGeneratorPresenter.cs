using System;
using System.Collections.Generic;
using FourWinged.Grids;
using FourWinged.PoissonGraph;
using FourWinged.WorldGenerator.Installers;
using FourWinged.WorldGenerator.View;
using R3;
using RandomNoise;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;
using Zenject;
using Mesh = UnityEngine.Mesh;

namespace FourWinged.WorldGenerator.Presenter
{
    public class WorldGeneratorPresenter : IInitializable, IDisposable
    {
        private readonly WorldGeneratorView _worldGeneratorView;
        private readonly IPoissonDiscSampler _poissonDiscSampler;
        private readonly INoise2D _noise2D;
        private readonly WorldGenerationSettings _worldGenerationSettings;
        private readonly Mesh _mesh;
        private readonly Polygon _polygon;
        private readonly ConstraintOptions _options;
        private readonly List<Vector3> _vertices;
        private readonly List<Vector2> _uvs;
        private IMesh _delaunayMesh;

        private NodeGraph<IGridObject2D> _nodeGraph => _poissonDiscSampler.NodeGraph;

        private DisposableBag _disposableBag;

        public WorldGeneratorPresenter(WorldGeneratorView worldGeneratorView, IPoissonDiscSampler poissonDiscSampler, INoise2D noise2D, WorldGenerationSettings worldGenerationSettings)
        {
            _worldGeneratorView = worldGeneratorView;
            _poissonDiscSampler = poissonDiscSampler;
            _noise2D = noise2D;
            _worldGenerationSettings = worldGenerationSettings;
            _vertices = new List<Vector3>();
            _uvs = new List<Vector2>();
            _mesh = new Mesh();
            _polygon = new Polygon();
            _options = new ConstraintOptions() { ConformingDelaunay = true };
        }

        public void Initialize()
        {
            _poissonDiscSampler.OnComplete.Subscribe(_ => CreateMesh()).AddTo(ref _disposableBag);
            CreateMesh();
        }

        private Vector3 Get3DPositionWithHeight(float x, float z)
        {
            var noiseValue = _noise2D.GetValue(x, z);
            var y = Mathf.Lerp(_worldGenerationSettings.MinimumHeight, _worldGenerationSettings.MaximumHeight, noiseValue);
            return new Vector3(x, y, z);
        }

        private void ApplyToUnityMesh(IMesh mesh)
        {
            _vertices.Clear();
            _uvs.Clear();

            foreach (var vertex in mesh.Vertices)
            {
                _vertices.Add(new Vector3((float)vertex.X, 0, (float)vertex.Y));
                _uvs.Add(new Vector2((float)vertex.X / _worldGenerationSettings.WorldSize.x, (float)vertex.Y / _worldGenerationSettings.WorldSize.y));
            }

            var v = _vertices[0];
            v.y = (_worldGenerationSettings.MinimumHeight + _worldGenerationSettings.MaximumHeight) * 0.5f;
            _vertices[0] = v;
            foreach (var nodeGraphEdge in _poissonDiscSampler.NodeGraph.Edges)
            {
                var v1Index = nodeGraphEdge.Item1;
                var v2Index = nodeGraphEdge.Item2;
                var v1 = _vertices[v1Index];
                var v2 = _vertices[v2Index];
                var d = (_poissonDiscSampler.NodeGraph.Nodes[v1Index].Position2D -
                         _poissonDiscSampler.NodeGraph.Nodes[v2Index].Position2D).magnitude;
                var p = _noise2D.GetValue(v2.x, v2.z);
                var angle = Mathf.Deg2Rad * Mathf.Lerp(-_worldGenerationSettings.MaxSlope, _worldGenerationSettings.MaxSlope, p);
                v2.y = v1.y + Mathf.Tan(angle) * d;
                var sigma = 1;
                var exponent = -new Vector2(v2.x / _worldGenerationSettings.WorldSize.x, v2.z / _worldGenerationSettings.WorldSize.y).sqrMagnitude / (2 * sigma * sigma);
                v2.y *= Mathf.Exp(exponent);
                v2.y = Mathf.Clamp(v2.y, _worldGenerationSettings.MinimumHeight, _worldGenerationSettings.MaximumHeight);
                _vertices[v2Index] = v2;
            }

            var tris = new int[mesh.Triangles.Count * 3];
            int i = 0;
            foreach (var meshTriangle in mesh.Triangles)
            {
                tris[i] = meshTriangle.GetVertexID(0);
                tris[i + 1] = meshTriangle.GetVertexID(2);
                tris[i + 2] = meshTriangle.GetVertexID(1);
                i += 3;
            }
            _mesh.SetVertices(_vertices);
            _mesh.SetUVs(0, _uvs);
            _mesh.SetTriangles(tris, 0);
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
            _worldGeneratorView.WorldMesh = _mesh;
        }

        private void CreateMesh()
        {
            if (_nodeGraph.Nodes.Count < 3) return;
            foreach (var node in _nodeGraph.Nodes)
            {
                _polygon.Add(new Vertex(node.Position2D.x, node.Position2D.y));
            }
            _delaunayMesh = _polygon.Triangulate(_options);
            ApplyToUnityMesh(_delaunayMesh);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
