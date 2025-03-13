using System;
using UnityEngine;

namespace FourWinged.Grids
{
    public interface IGridObject : IGridObject2D
    {
        Vector3 Position { get; }
        Bounds BoundingBox { get; }
    }

    public interface IGridObject2D
    {
        Vector2 Position2D { get; }
        Rect Rect { get; }
        float Radius { get; }
    }

    public interface IGridObject2DFactory
    {
        IGridObject2D Create(Vector2 position2D, float radius);
        IGridObject2D Create(Vector3 position, float radius);
    }
}
