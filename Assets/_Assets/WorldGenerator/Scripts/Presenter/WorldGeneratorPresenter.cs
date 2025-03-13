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
        private readonly IGraphModel<IGridObject2D> _graphModel;
        private readonly INoise2D _noise2D;
        private readonly WorldGenerationSettings _worldGenerationSettings;
        private readonly Mesh _mesh;
        private readonly Polygon _polygon;
        private readonly ConstraintOptions _options;
        private readonly List<Vector3> _vertices;
        private IMesh _delaunayMesh;

        private NodeGraph<IGridObject2D> _nodeGraph => _graphModel.NodeGraph;

        private DisposableBag _disposableBag;

        public WorldGeneratorPresenter(WorldGeneratorView worldGeneratorView, IGraphModel<IGridObject2D> graphModel, INoise2D noise2D, WorldGenerationSettings worldGenerationSettings)
        {
            _worldGeneratorView = worldGeneratorView;
            _graphModel = graphModel;
            _noise2D = noise2D;
            _worldGenerationSettings = worldGenerationSettings;
            _vertices = new List<Vector3>();
            _mesh = new Mesh();
            _polygon = new Polygon();
            _options = new ConstraintOptions() { ConformingDelaunay = true };
        }

        public void Initialize()
        {
            _graphModel.OnComplete.Subscribe(_ => CreateMesh()).AddTo(ref _disposableBag);
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
            foreach (var vertex in mesh.Vertices)
            {
                _vertices.Add(Get3DPositionWithHeight((float)vertex.X, (float)vertex.Y));
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
