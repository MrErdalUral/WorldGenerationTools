using System;
using UnityEngine;

namespace Grids
{
    //Extends IGridObject2D so they can be registered to a 2D grid and their positions be compared in x,z dimensions
    public interface IGridObject : IGridObject2D 
    {
        Vector3 Position { get; }
        Bounds BoundingBox { get; }
    }
}
