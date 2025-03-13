using FourWinged.Grids;
using UnityEngine;

namespace FourWinged.Grids
{
    public readonly struct GridObject2D : IGridObject2D
    {
        public class Factory : IGridObject2DFactory
        {
            public IGridObject2D Create(Vector2 position2D, float radius)
            {
                return new GridObject2D(position2D, radius);
            }

            public IGridObject2D Create(Vector3 position, float radius)
            {
                return new GridObject2D(new Vector2(position.x, position.z), radius);
            }
        }
        private readonly Vector2 _position2D;
        private readonly float _radius;
        private readonly Rect _rect;

        public GridObject2D(Vector2 position2D, float radius)
        {
            _position2D = position2D;
            _radius = radius;
            _rect = new Rect(_position2D, new Vector2(2 * radius, 2 * radius));
        }

        public Vector2 Position2D => _position2D;
        public Rect Rect => _rect;
        public float Radius => _radius;
    }
}