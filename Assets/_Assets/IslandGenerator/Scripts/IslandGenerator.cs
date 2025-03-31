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

        public IslandGenerator(IPoissonDiscSampler poissonDiscSampler, INoise2D noise2D)
        {
            _poissonDiscSampler = poissonDiscSampler;
            _noise2D = noise2D;
            _options = new ConstraintOptions { ConformingDelaunay = true };
        }

        /// <summary>
        /// Generates an island using the provided settings and an optional seed override.
        /// </summary>
        /// <param name="settings">Island generation settings.</param>
        /// <param name="overrideSeed">Optional seed to override the settings' seed.</param>
        /// <returns>A generated island represented by an IslandDto.</returns>
        public IslandDto GenerateIsland(IIslandGenerationSettings settings, int? overrideSeed = null)
        {
            _noise2D.SetSeed(overrideSeed ?? settings.Seed);
            var nodeGraph = _poissonDiscSampler.SamplePointsAsync(settings.PoissonDiscSettings);

            IMesh triangleMesh = CreateTriangleMesh(nodeGraph.Nodes);
            if (triangleMesh == null)
            {
                throw new System.InvalidOperationException("Insufficient nodes to generate a valid triangle mesh.");
            }

            List<Vector3> vertices = ConvertMeshVertices(triangleMesh.Vertices);
            GenerateHeights(nodeGraph, vertices, settings);
            int[] triangles = GenerateTriangles(triangleMesh.Triangles);

            return new IslandDto(vertices, nodeGraph.Edges, triangles);
        }

        /// <summary>
        /// Converts mesh vertices into a list of Unity Vector3 objects.
        /// </summary>
        private List<Vector3> ConvertMeshVertices(ICollection<Vertex> meshVertices)
        {
            var vertices = new List<Vector3>();
            foreach (var vertex in meshVertices)
            {
                vertices.Add(new Vector3((float)vertex.X, 0f, (float)vertex.Y));
            }
            return vertices;
        }

        /// <summary>
        /// Generates an array of triangle indices from the mesh triangles.
        /// </summary>
        private int[] GenerateTriangles(ICollection<TriangleNet.Topology.Triangle> meshTriangles)
        {
            int[] triangles = new int[meshTriangles.Count * 3];
            int i = 0;
            foreach (var triangle in meshTriangles)
            {
                triangles[i++] = triangle.GetVertexID(0);
                triangles[i++] = triangle.GetVertexID(2);
                triangles[i++] = triangle.GetVertexID(1);
            }
            return triangles;
        }

        /// <summary>
        /// Calculates the height of each vertex using noise values as a slope between nodes.
        /// </summary>
        /// <param name="nodeGraph">The node graph containing grid nodes.</param>
        /// <param name="vertices">The list of vertices to modify.</param>
        /// <param name="settings">Island generation settings.</param>
        private void GenerateHeights(INodeGraph<IGridObject2D> nodeGraph, List<Vector3> vertices,
            IIslandGenerationSettings settings)
        {
            float sigma = settings.HeightFalloffSigma;
            Vector2 worldSize = settings.WorldSize;

            // Set heights for root nodes.
            foreach (int rootIndex in nodeGraph.Roots)
            {
                Vector3 vertex = vertices[rootIndex];
                float falloff = Mathf.Exp(-new Vector2(vertex.x / worldSize.x, vertex.z / worldSize.y).sqrMagnitude / (2f * sigma * sigma));
                vertex.y = (settings.MinimumHeight + settings.MaximumHeight) * 0.5f * falloff;
                vertices[rootIndex] = vertex;
            }

            // Adjust heights along edges.
            foreach (var edge in nodeGraph.Edges)
            {
                int v1Index = edge.Item1;
                int v2Index = edge.Item2;
                Vector3 v1 = vertices[v1Index];
                Vector3 v2 = vertices[v2Index];

                Vector2 pos1 = _poissonDiscSampler.NodeGraph.Nodes[v1Index].Position2D;
                Vector2 pos2 = _poissonDiscSampler.NodeGraph.Nodes[v2Index].Position2D;
                float distance = (pos1 - pos2).magnitude;
                float noiseValue = _noise2D.GetValue(v2.x, v2.z);
                float angle = Mathf.Deg2Rad * Mathf.Lerp(-settings.MaxSlope, settings.MaxSlope, noiseValue);
                v2.y = v1.y + Mathf.Tan(angle) * distance;

                float exponent = -new Vector2(v2.x / worldSize.x, v2.z / worldSize.y).sqrMagnitude / (2f * sigma * sigma);
                v2.y *= Mathf.Exp(exponent);
                v2.y = Mathf.Clamp(v2.y, settings.MinimumHeight, settings.MaximumHeight);
                vertices[v2Index] = v2;
            }
        }

        /// <summary>
        /// Creates a triangle mesh from the provided nodes.
        /// </summary>
        private IMesh CreateTriangleMesh(List<IGridObject2D> nodes)
        {
            if (nodes.Count < 3)
            {
                return null;
            }

            var polygon = new Polygon();
            foreach (var node in nodes)
            {
                polygon.Add(new Vertex(node.Position2D.x, node.Position2D.y));
            }
            return polygon.Triangulate(_options);
        }
    }
}
