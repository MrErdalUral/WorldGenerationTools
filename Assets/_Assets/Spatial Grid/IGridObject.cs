using System;
using UnityEngine;

namespace Grids
{
    //Extends IGridObject2D so they can be registered to a 2D grid and their positions be compared in x,z dimensions
    public interface IGridObject : IPosition, IVolume, IRadius
    {

    }

    public interface IPosition 
    {
        Vector3 Position { get; }
    }

    public interface IVolume 
    {
        Bounds BoundingBox { get; }
    }
}
