using UnityEngine;

namespace Grids
{
    public interface IGridObjectFactory
    {
        IGridObject Create(Vector3 position, float radius);
    }
}