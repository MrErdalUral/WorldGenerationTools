using System.Collections.Generic;
using UnityEngine;

namespace FourWinged.Grids.SpatialGrid
{
    public interface IGrid
    {
        void ResizeGridCells(float cellSize);
        void AddToGrid(IGridObject obj);
        void RemoveFromGrid(IGridObject obj);
        void MoveObject(IGridObject obj, Vector3 displacement);
        void Clear();
        bool CheckRadiusEmpty(float radius, Vector3 center);
        void RadiusQuery(float radius, Vector3 center, List<IGridObject> resultList);
        void BoxQuery(Bounds box, List<IGridObject> resultList);
    }
}