using System;
using FourWinged.Grids;
using UnityEngine;

namespace FourWinged.Grids
{
    public class GridObject : IGridObject
    {
        public class Factory : IGridObjectFactory
        {
            public IGridObject Create(Vector3 position, float radius)
            {
                return new GridObject(position, radius);
            }
        }
        private readonly Vector3 _position;
        private readonly Vector2 _position2D;
        private readonly float _radius;
        private readonly Rect _rect;
        private readonly Bounds _bounds;

        public GridObject(Vector3 position, float radius)
        {
            _position = position;
            _position2D = new Vector2(position.x, position.z);
            _radius = radius;
            _rect = new Rect(_position2D, new Vector2(2 * radius, 2 * radius));
            _bounds = new Bounds(_position, new Vector3(2 * radius, 2 * radius, 2 * radius));
        }

        public Vector2 Position2D => _position2D;
        public Rect Rect => _rect;
        public float Radius => _radius;
        public Vector3 Position => _position;
        public Bounds BoundingBox => _bounds;
    }
}