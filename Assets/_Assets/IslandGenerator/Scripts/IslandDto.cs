using System.Collections.Generic;
using UnityEngine;

namespace IslandGenerator
{
    public readonly struct IslandDto
    {
        private readonly List<Vector3> _islandVertices;
        private readonly List<(int, int)> _islandPaths;
        private readonly int[] _islandTriangles;

        public List<Vector3> IslandVertices => _islandVertices;
        public List<(int, int)> IslandPaths => _islandPaths;
        public int[] IslandTriangles => _islandTriangles;

        public IslandDto(List<Vector3> vertices, List<(int, int)> paths, int[] triangles)
        {
            _islandVertices = vertices;
            _islandPaths = paths;
            _islandTriangles = triangles;
        }
    }
}