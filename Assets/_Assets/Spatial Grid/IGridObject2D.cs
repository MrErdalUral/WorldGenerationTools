using UnityEngine;

namespace FourWinged.Grids
{
    public interface IGridObject2D
    {
        Vector2 Position2D { get; }
        Rect Rect { get; }
        float Radius { get; }
    }
}