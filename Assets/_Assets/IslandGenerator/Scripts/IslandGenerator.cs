using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Grids;
using IslandGenerator.Installers;
using PoissonDiscSampling;
using R3;
using RandomNoise;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;

namespace IslandGenerator
{
    public class IslandGenerator : IIslandGenerator
    {
        public readonly IPoissonDiscSampler _poissonDiscSampler;
        public readonly INoise2D _noise2D;
        private readonly Polygon _polygon;
        private readonly ConstraintOptions _options;
        private IMesh _delaunayMesh;
        private Subject<IslandDto> _island = new Subject<IslandDto>();

        public Subject<IslandDto> OnIslandGenerated => _island;


        public IslandGenerator(IPoissonDiscSampler poissonDiscSampler, INoise2D noise2D)
        {
            _poissonDiscSampler = poissonDiscSampler;
            _noise2D = noise2D;
            _polygon = new Polygon();
            _options = new ConstraintOptions() { ConformingDelaunay = true };

        }

        public async UniTask<IslandDto> GenerateIsland(IslandGenerationSettings settings)
        {
            var nodeGraph = await _poissonDiscSampler.SamplePointsAsync(settings.PoissonSettings);

            CreateTriangleMesh(nodeGraph.Nodes);
            var vertices = new List<Vector3>();
            foreach (var vertex in _delaunayMesh.Vertices)
            {
                vertices.Add(new Vector3((float)vertex.X, 0, (float)vertex.Y));
            }
            GenerateHeights(vertices, settings);

            var tris = new int[_delaunayMesh.Triangles.Count * 3];
            int i = 0;
            foreach (var meshTriangle in _delaunayMesh.Triangles)
            {
                tris[i] = meshTriangle.GetVertexID(0);
                tris[i + 1] = meshTriangle.GetVertexID(2);
                tris[i + 2] = meshTriangle.GetVertexID(1);
                i += 3;
            }

            var island = new IslandDto(vertices, nodeGraph.Edges, tris);
            OnIslandGenerated.OnNext(island);
            return island;
        }


        private void GenerateHeights(List<Vector3> vertices, IslandGenerationSettings settings)
        {
            var v = vertices[0];
            v.y = (settings.MinimumHeight + settings.MaximumHeight) * 0.5f;
            vertices[0] = v;
            foreach (var nodeGraphEdge in _poissonDiscSampler.NodeGraph.Edges)
            {
                var v1Index = nodeGraphEdge.Item1;
                var v2Index = nodeGraphEdge.Item2;
                var v1 = vertices[v1Index];
                var v2 = vertices[v2Index];
                var d = (_poissonDiscSampler.NodeGraph.Nodes[v1Index].Position2D -
                         _poissonDiscSampler.NodeGraph.Nodes[v2Index].Position2D).magnitude;
                var p = _noise2D.GetValue(v2.x, v2.z);
                var angle = Mathf.Deg2Rad * Mathf.Lerp(-settings.MaxSlope, settings.MaxSlope, p);
                v2.y = v1.y + Mathf.Tan(angle) * d;
                var sigma = 1;
                var exponent = -new Vector2(v2.x / settings.WorldSize.x, v2.z / settings.WorldSize.y).sqrMagnitude / (2 * sigma * sigma);
                v2.y *= Mathf.Exp(exponent);
                v2.y = Mathf.Clamp(v2.y, settings.MinimumHeight, settings.MaximumHeight);
                vertices[v2Index] = v2;
            }
        }

        private void CreateTriangleMesh(List<IGridObject2D> nodes)
        {
            if (nodes.Count < 3) return;
            foreach (var node in nodes)
            {
                _polygon.Add(new Vertex(node.Position2D.x, node.Position2D.y));
            }
            _delaunayMesh = _polygon.Triangulate(_options);
        }


    }
}
