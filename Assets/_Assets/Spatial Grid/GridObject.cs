using System;
using Grids;
using UnityEngine;

namespace Grids
{
    public class GridObject : IGridObject, IRadius
    {
        public class Factory : IGridObjectFactory
        {
            public IGridObject Create(Vector3 position, float radius)
            {
                return new GridObject(position, radius);
            }
        }
        private readonly Vector3 _position;
        private readonly float _radius;
        private readonly Bounds _bounds;

        public GridObject(Vector3 position, float radius)
        {
            _position = position;
            _radius = radius;
            _bounds = new Bounds(_position, new Vector3(2 * radius, 2 * radius, 2 * radius));
        }

        public float Radius => _radius;
        public Vector3 Position => _position;
        public Bounds BoundingBox => _bounds;
    }
}