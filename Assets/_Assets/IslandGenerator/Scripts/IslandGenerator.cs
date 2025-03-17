using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Grids;
using IslandGenerator.Settings;
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
        private readonly IPoissonDiscSampler _poissonDiscSampler;
        private readonly INoise2D _noise2D;
        private readonly ConstraintOptions _options;
        private Polygon _polygon;

        public IslandGenerator(IPoissonDiscSampler poissonDiscSampler, INoise2D noise2D)
        {
            _poissonDiscSampler = poissonDiscSampler;
            _noise2D = noise2D;
            _options = new ConstraintOptions() { ConformingDelaunay = true };
        }

        public IslandDto GenerateIsland(IIslandGenerationSettings settings, int? overrideSeed = null)
        {
            _noise2D.SetSeed(overrideSeed ?? settings.Seed);
            var nodeGraph = _poissonDiscSampler.SamplePointsAsync(settings.PoissonDiscSettings);

            var iMesh = CreateTriangleMesh(nodeGraph.Nodes);
            var vertices = new List<Vector3>();
            foreach (var vertex in iMesh.Vertices)
            {
                vertices.Add(new Vector3((float)vertex.X, 0, (float)vertex.Y));
            }
            GenerateHeights(nodeGraph, vertices, settings);

            var tris = new int[iMesh.Triangles.Count * 3];
            int i = 0;
            foreach (var meshTriangle in iMesh.Triangles)
            {
                tris[i] = meshTriangle.GetVertexID(0);
                tris[i + 1] = meshTriangle.GetVertexID(2);
                tris[i + 2] = meshTriangle.GetVertexID(1);
                i += 3;
            }

            var island = new IslandDto(vertices, nodeGraph.Edges, tris);
            return island;
        }

        /// <summary>
        /// Calculates height by using noise values as slope between nodes.
        /// </summary>
        /// <param name="nodeGraph"></param>
        /// <param name="vertices"></param>
        /// <param name="settings"></param>
        private void GenerateHeights(NodeGraph<IGridObject2D> nodeGraph, List<Vector3> vertices,
            IIslandGenerationSettings settings)
        {
            var sigma = settings.HeightFalloffSigma;
            foreach (var rootIndex in nodeGraph.Roots)
            {
                var v = vertices[rootIndex];
                v.y = (settings.MinimumHeight + settings.MaximumHeight) * 0.5f *
                      Mathf.Exp(-new Vector2(v.x / settings.WorldSize.x, v.z / settings.WorldSize.y).sqrMagnitude / (2f * sigma * sigma));
                vertices[rootIndex] = v;
            }
            foreach (var nodeGraphEdge in nodeGraph.Edges)
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
                var exponent = -new Vector2(v2.x / settings.WorldSize.x, v2.z / settings.WorldSize.y).sqrMagnitude / (2 * sigma * sigma);
                v2.y *= Mathf.Exp(exponent);
                v2.y = Mathf.Clamp(v2.y, settings.MinimumHeight, settings.MaximumHeight);
                vertices[v2Index] = v2;
            }
        }

        private IMesh CreateTriangleMesh(List<IGridObject2D> nodes)
        {
            if (nodes.Count < 3) return null;
            _polygon = new Polygon();
            foreach (var node in nodes)
            {
                _polygon.Add(new Vertex(node.Position2D.x, node.Position2D.y));
            }
            return _polygon.Triangulate(_options);
        }


    }
}
