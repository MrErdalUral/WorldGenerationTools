using UnityEngine;

namespace FourWinged.Grids
{
    public interface IGridObject2DFactory
    {
        IGridObject2D Create(Vector2 position2D, float radius);
        IGridObject2D Create(Vector3 position, float radius);
    }
}