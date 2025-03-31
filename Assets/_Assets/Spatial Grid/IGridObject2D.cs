using UnityEngine;

namespace Grids
{
    public interface IGridObject2D : IPosition2D, IVolume2D, IRadius { }

    public interface IPosition2D
    {
        Vector2 Position2D { get; }
    }

    public interface IVolume2D
    {
        Rect Rect { get; }
    }

    public interface IRadius
    {
        float Radius { get; }
    }
}