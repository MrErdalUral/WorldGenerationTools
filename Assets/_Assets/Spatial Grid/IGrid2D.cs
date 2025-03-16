using System.Collections.Generic;
using UnityEngine;

namespace Grids.SpatialGrid
{
    public interface IGrid2D
    {
        void ResizeGridCells(float cellSize);
        void AddToGrid(IGridObject2D obj);
        void RemoveFromGrid(IGridObject2D obj);
        void MoveObject(IGridObject2D obj, Vector2 displacement);
        void Clear();
        bool CheckRadiusEmpty(float radius, Vector2 center);
        void RadiusQuery(float radius, Vector2 center, List<IGridObject2D> resultList);
        void BoxQuery(Rect box, List<IGridObject2D> resultList);
    }
}