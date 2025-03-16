using System.Collections.Generic;
using UnityEngine;

namespace IslandGenerator
{
    public struct IslandDto
    {
        public List<Vector3> IslandVertices;
        public List<(int, int)> IslandPaths;
        public int[] IslandTriangles;

        public IslandDto(List<Vector3> vertices, List<(int, int)> paths, int[] triangles)
        {
            IslandVertices = vertices;
            IslandPaths = paths;
            IslandTriangles = triangles;
        }
    }
}