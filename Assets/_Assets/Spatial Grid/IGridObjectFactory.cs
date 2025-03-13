using UnityEngine;

namespace FourWinged.Grids
{
    public interface IGridObjectFactory
    {
        IGridObject Create(Vector3 position, float radius);
    }
}