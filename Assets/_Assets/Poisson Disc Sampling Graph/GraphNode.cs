using System;
using UnityEngine;

namespace FourWinged.PoissonGraph
{
    public struct GraphNode : IGridObject
    {
        private readonly Vector3 _position;
        private readonly Vector3 _position2D;
        private readonly float _radius;
        private readonly int _index;
        private readonly Bounds _bounds;

        public int Index => _index;
        public Vector3 Position => _position;
        public Vector3 BoundingPosition => _position2D;
        public Bounds BoundingBox => _bounds;
        public float BoundingRadius => _radius;
        public float Radius => _radius;

        public GraphNode(int index, Vector3 position, float radius)
        {
            _index = index;
            _position = position;
            _position2D = new Vector3(position.x, 0, position.z);
            _radius = radius;
            _bounds = new Bounds(_position2D, new Vector3(2 * radius, 2 * radius, 2 * radius));
        }
    }
}