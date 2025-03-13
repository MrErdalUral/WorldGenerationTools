using System;
using System.Collections.Generic;
using UnityEngine;

namespace FourWinged.Grids.SpatialGrid
{
    public class SpatialGrid2D<T> where T : IGridObject2D
    {
        private readonly Dictionary<Vector2Int, HashSet<T>[,]> _grid = new Dictionary<Vector2Int, HashSet<T>[,]>();
        private readonly HashSet<T> _uniqueResults = new HashSet<T>();
        private readonly HashSet<T> _allObjects = new HashSet<T>();
        private readonly int _chunkSize;
        private float _cellSize;
        private float _cellRadius;


        public SpatialGrid2D(float cellSize, int chunkSize = 32)
        {
            _cellSize = cellSize;
            _chunkSize = chunkSize;
            _cellRadius = 0.5f * _cellSize * Mathf.Sqrt(2f);

        }
        /// <summary>
        /// Restructure cells with the new cellSize
        /// </summary>
        /// <param name="cellSize"></param>
        public void ResizeGridCells(float cellSize)
        {
            _cellSize = cellSize;
            _cellRadius = 0.5f * _cellSize * Mathf.Sqrt(2f);

            //We only need to clear content of the cells. Min X,Y cell coordinates and existing columns & cells can be reused
            ClearCells();

            foreach (var obj in _allObjects)
            {
                AddToGrid(obj);
            }
        }

        /// <summary>
        /// Adds object to the grid cells
        /// </summary>
        /// <param name="obj"></param>
        public void AddToGrid(T obj)
        {
            Vector2Int minCellPos = GetCellPosition(obj.Rect.min);
            Vector2Int maxCellPos = GetCellPosition(obj.Rect.max);
            for (int x = minCellPos.x; x <= maxCellPos.x; x++)
            {
                for (int y = minCellPos.y; y <= maxCellPos.y; y++)
                {
                    AddToCell(obj, x, y);
                }
            }
            _allObjects.Add(obj);
        }

        /// <summary>
        /// Remove object from grid cells.
        /// </summary>
        /// <param name="obj"></param>
        public void RemoveFromGrid(T obj)
        {
            Vector2Int minCellPos = GetCellPosition(obj.Rect.min);
            Vector2Int maxCellPos = GetCellPosition(obj.Rect.max);

            for (int x = minCellPos.x; x <= maxCellPos.x; x++)
            {
                for (int y = minCellPos.y; y <= maxCellPos.y; y++)
                {
                    RemoveFromCell(obj, x, y);
                }
            }
            _allObjects.Remove(obj);
        }

        /// <summary>
        /// Checks the old and the new bounding volume of the object.
        /// Removes the object from old cells and add them to the new ones
        /// Does not update shared cells between old and new
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="displacement"></param>
        public void Move(T obj, Vector2 displacement)
        {
            Vector2Int prevMinCellPos = GetCellPosition(obj.Rect.min - displacement);
            Vector2Int newMinCellPos = GetCellPosition(obj.Rect.min);
            Vector2Int prevMaxCellPos = GetCellPosition(obj.Rect.max - displacement);
            Vector2Int newMaxCellPos = GetCellPosition(obj.Rect.max);

            if (prevMinCellPos == newMinCellPos && prevMaxCellPos == newMaxCellPos)
                return;

            // Remove from cells no longer in cell range
            for (int x = prevMinCellPos.x; x <= prevMaxCellPos.x; x++)
            {
                for (int y = prevMinCellPos.y; y <= prevMaxCellPos.x; y++)
                {
                    {
                        if (x >= newMinCellPos.x && x <= newMaxCellPos.x &&
                            y >= newMinCellPos.y && y <= newMaxCellPos.y)
                        {
                            continue;
                        }
                        RemoveFromCell(obj, x, y);
                    }
                }
            }

            // Add to new cells that was not previously in cell range
            for (int x = newMinCellPos.x; x <= newMaxCellPos.x; x++)
            {
                for (int y = newMinCellPos.y; y <= newMaxCellPos.y; y++)
                {

                    if (x >= prevMinCellPos.x && x <= prevMaxCellPos.x &&
                        y >= prevMinCellPos.y && y <= prevMaxCellPos.y)
                    {
                        continue;
                    }
                    AddToCell(obj, x, y);
                }
            }
        }

        /// <summary>
        /// Clean up allocations and prepare the grid for reuse
        /// </summary>
        public void Clear()
        {
            ClearCells();
            _uniqueResults.Clear();
            _uniqueResults.TrimExcess();
            _allObjects.Clear();
            _grid.Clear();
        }

        public bool CheckRadiusEmpty(float radius, Vector2 center)
        {
            Rect sphereBounds = new Rect(center - Vector2.one * radius, Vector2.one * (2f * radius));
            Vector2Int minCellPos = GetCellPosition(sphereBounds.min);
            Vector2Int maxCellPos = GetCellPosition(sphereBounds.max);

            // Then figure out which chunks that cell range spans.
            Vector2Int minChunkPos = GetChunkPosition(minCellPos.x, minCellPos.y);
            Vector2Int maxChunkPos = GetChunkPosition(maxCellPos.x, maxCellPos.y);

            // Iterate over each chunk in the relevant range
            for (int chunkX = minChunkPos.x; chunkX <= maxChunkPos.x; chunkX++)
            {
                int chunkMinX = chunkX * _chunkSize;
                int chunkMaxX = chunkMinX + (_chunkSize - 1);

                // Clamp this chunk’s local iteration range to [minCellPos.x .. maxCellPos.x]
                int localMinX = 0;
                if (chunkMinX < minCellPos.x)
                    localMinX = CalculateChunkLocal(Math.Max(chunkMinX, minCellPos.x));
                int localMaxX = _chunkSize - 1;
                if (chunkMaxX > maxCellPos.x)
                    localMaxX = CalculateChunkLocal(Math.Min(chunkMaxX, maxCellPos.x));

                for (int chunkY = minChunkPos.y; chunkY <= maxChunkPos.y; chunkY++)
                {
                    int chunkMinY = chunkY * _chunkSize;
                    int chunkMaxY = chunkMinY + (_chunkSize - 1);

                    int localMinY = 0;
                    if (chunkMinY < minCellPos.y)
                        localMinY = CalculateChunkLocal(Math.Max(chunkMinY, minCellPos.y));
                    int localMaxY = _chunkSize - 1;
                    if (chunkMaxY > maxCellPos.y)
                        localMaxY = CalculateChunkLocal(Math.Min(chunkMaxY, maxCellPos.y));
                    var chunkPos = new Vector2Int(chunkX, chunkY);
                    if (!_grid.TryGetValue(chunkPos, out var chunk))
                        continue;


                    // Iterate the cells in this chunk that lie within our bounding box range
                    for (int lx = localMinX; lx <= localMaxX; lx++)
                    {
                        int cellX = chunkMinX + lx;
                        for (int ly = localMinY; ly <= localMaxY; ly++)
                        {
                            int cellY = chunkMinY + ly;
                            var cell = chunk[lx, ly];
                            if (cell == null)
                                continue;

                            //----------------------------------------------------
                            // 1) Compute the center of this cell
                            //----------------------------------------------------
                            Vector2 cellCenter = GetCellCenter(cellX, cellY);

                            //----------------------------------------------------
                            // 2) Distance-based culling logic:
                            //
                            //    d = distance(queryCenter, cellCenter)
                            //    If (d + halfDiag < radius) => entire cell inside
                            //    If (d > radius + halfDiag) => entire cell outside
                            //    else => partial => check per-object
                            //----------------------------------------------------
                            float sqrDistToCenter = (cellCenter - center).sqrMagnitude;

                            if (sqrDistToCenter < (radius - _cellRadius) * (radius - _cellRadius))
                            {
                                if (cell.Count > 1)
                                    return false;
                            }
                            else if (sqrDistToCenter > (radius + _cellRadius) * (radius + _cellRadius))
                            {
                                // skip
                            }
                            else
                            {
                                // Partial overlap => do per-object checks
                                foreach (var obj in cell)
                                {
                                    // Avoid duplicates in final result
                                    if (_uniqueResults.Contains(obj))
                                        continue;

                                    // Use the object’s bounding sphere or a simpler center+radius check:
                                    float distSq = (obj.Position2D - center).sqrMagnitude;
                                    float combinedRadius = obj.Radius + radius;
                                    if (distSq <= combinedRadius * combinedRadius)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        public void RadiusQuery(float radius, Vector2 center, List<T> resultList)
        {
            Rect sphereBounds = new Rect(center - Vector2.one * radius, Vector2.one * (2f * radius));
            Vector2Int minCellPos = GetCellPosition(sphereBounds.min);
            Vector2Int maxCellPos = GetCellPosition(sphereBounds.max);

            // Then figure out which chunks that cell range spans.
            Vector2Int minChunkPos = GetChunkPosition(minCellPos.x, minCellPos.y);
            Vector2Int maxChunkPos = GetChunkPosition(maxCellPos.x, maxCellPos.y);

            // Iterate over each chunk in the relevant range
            for (int chunkX = minChunkPos.x; chunkX <= maxChunkPos.x; chunkX++)
            {
                int chunkMinX = chunkX * _chunkSize;
                int chunkMaxX = chunkMinX + (_chunkSize - 1);

                // Clamp this chunk’s local iteration range to [minCellPos.x .. maxCellPos.x]
                int localMinX = 0;
                if (chunkMinX < minCellPos.x)
                    localMinX = CalculateChunkLocal(Math.Max(chunkMinX, minCellPos.x));
                int localMaxX = _chunkSize - 1;
                if (chunkMaxX > maxCellPos.x)
                    localMaxX = CalculateChunkLocal(Math.Min(chunkMaxX, maxCellPos.x));

                for (int chunkY = minChunkPos.y; chunkY <= maxChunkPos.y; chunkY++)
                {
                    int chunkMinY = chunkY * _chunkSize;
                    int chunkMaxY = chunkMinY + (_chunkSize - 1);

                    int localMinY = 0;
                    if (chunkMinY < minCellPos.y)
                        localMinY = CalculateChunkLocal(Math.Max(chunkMinY, minCellPos.y));
                    int localMaxY = _chunkSize - 1;
                    if (chunkMaxY > maxCellPos.y)
                        localMaxY = CalculateChunkLocal(Math.Min(chunkMaxY, maxCellPos.y));
                    var chunkPos = new Vector2Int(chunkX, chunkY);
                    if (!_grid.TryGetValue(chunkPos, out var chunk))
                        continue;


                    // Iterate the cells in this chunk that lie within our bounding box range
                    for (int lx = localMinX; lx <= localMaxX; lx++)
                    {
                        int cellX = chunkMinX + lx;
                        for (int ly = localMinY; ly <= localMaxY; ly++)
                        {
                            int cellY = chunkMinY + ly;
                            var cell = chunk[lx, ly];
                            if (cell == null)
                                continue;

                            //----------------------------------------------------
                            // 1) Compute the center of this cell
                            //----------------------------------------------------
                            Vector2 cellCenter = GetCellCenter(cellX, cellY);

                            //----------------------------------------------------
                            // 2) Distance-based culling logic:
                            //
                            //    d = distance(queryCenter, cellCenter)
                            //    If (d + halfDiag < radius) => entire cell inside
                            //    If (d > radius + halfDiag) => entire cell outside
                            //    else => partial => check per-object
                            //----------------------------------------------------
                            float sqrDistToCenter = (cellCenter - center).sqrMagnitude;

                            if (sqrDistToCenter < (radius - _cellRadius) * (radius - _cellRadius))
                            {
                                _uniqueResults.UnionWith(cell);
                            }
                            else if (sqrDistToCenter > (radius + _cellRadius) * (radius + _cellRadius))
                            {
                                // skip
                            }
                            else
                            {
                                // Partial overlap => do per-object checks
                                foreach (var obj in cell)
                                {
                                    // Avoid duplicates in final result
                                    if (_uniqueResults.Contains(obj))
                                        continue;

                                    // Use the object’s bounding sphere or a simpler center+radius check:
                                    float distSq = (obj.Position2D - center).sqrMagnitude;
                                    float combinedRadius = obj.Radius + radius;
                                    if (distSq <= combinedRadius * combinedRadius)
                                    {
                                        _uniqueResults.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Now copy results to the caller’s list
            if (_uniqueResults.Count > 0)
            {
                resultList.AddRange(_uniqueResults);
                if (_uniqueResults.Count > 16)
                    _uniqueResults.TrimExcess();
            }
            _uniqueResults.Clear();
        }



        /// <summary>
        /// fills resultList with unique results from the grid.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="resultList"></param>
        public void BoxQuery(Rect box, List<T> resultList)
        {
            Vector2Int minCellPos = GetCellPosition(box.min);
            Vector2Int maxCellPos = GetCellPosition(box.max);

            // Determine which chunks we need to cover
            Vector2Int minChunkPos = GetChunkPosition(minCellPos.x, minCellPos.y);
            Vector2Int maxChunkPos = GetChunkPosition(maxCellPos.x, maxCellPos.y);

            for (int chunkX = minChunkPos.x; chunkX <= maxChunkPos.x; chunkX++)
            {
                // Global cell range covered by this chunk
                int chunkMinX = chunkX * _chunkSize;
                int chunkMaxX = chunkMinX + (_chunkSize - 1);

                // Clamp to our query’s [minCellPos.X..maxCellPos.X]
                int localMinX = 0;
                if (chunkMinX < minCellPos.x)
                    localMinX = CalculateChunkLocal(Math.Max(chunkMinX, minCellPos.x));

                int localMaxX = _chunkSize - 1;
                if (chunkMaxX > maxCellPos.x)
                    localMaxX = CalculateChunkLocal(Math.Min(chunkMaxX, maxCellPos.x));
                for (int chunkY = minChunkPos.y; chunkY <= maxChunkPos.y; chunkY++)
                {
                    // Global cell range covered by this chunk
                    int chunkMinY = chunkY * _chunkSize;
                    int chunkMaxY = chunkMinY + (_chunkSize - 1);

                    // Clamp to our query’s [minCellPos.X..maxCellPos.X]
                    int localMinY = 0;
                    if (chunkMinY < minCellPos.y)
                        localMinY = CalculateChunkLocal(Math.Max(chunkMinY, minCellPos.y));

                    int localMaxY = _chunkSize - 1;
                    if (chunkMaxY > maxCellPos.y)
                        localMaxY = CalculateChunkLocal(Math.Min(chunkMaxY, maxCellPos.y));
                    var chunkPos = new Vector2Int(chunkX, chunkY);
                    if (!_grid.TryGetValue(chunkPos, out var chunk))
                        continue;


                    for (int localX = localMinX; localX <= localMaxX; localX++)
                    {
                        int cellX = chunkMinX + localX;
                        for (int localY = localMinX; localY <= localMaxY; localY++)
                        {
                            int cellY = chunkMinY + localY;
                            var cell = chunk[localX, localMaxY];
                            if (cell == null)
                                continue;

                            var bounds = GetCellBounds(cellX, cellY);
                            // If the query box fully contains this cell's bounding box,
                            // union the entire cell at once.
                            if (box.Contains(bounds.min) && box.Contains(bounds.max))
                            {
                                _uniqueResults.UnionWith(cell);
                            }
                            else
                            {
                                // Otherwise check individual bounding boxes
                                foreach (var obj in cell)
                                {
                                    if (_uniqueResults.Contains(obj)) continue;
                                    if (obj.Rect.Overlaps(box))
                                        _uniqueResults.Add(obj);
                                }
                            }
                        }
                    }
                }
            }

            var count = _uniqueResults.Count;
            if (count > 0)
            {
                resultList.AddRange(_uniqueResults);
                //If the m_uniqueResults capacity is too big from a query we want to trim it for future query performance
                if (count > 16)
                    _uniqueResults.TrimExcess();
            }
            _uniqueResults.Clear();

        }

        #region Private Methods

        private static int FloorDivInt(int value, int size)
        {
            // If value >= 0, integer division is fine.
            // If negative, adjust so we truly floor (e.g., -3/2 => -2 in math floor).
            if (value >= 0)
                return value / size;
            else
                return (value + 1) / size - 1;
        }

        private Vector2Int GetChunkPosition(int cellX, int cellY)
        {
            return new Vector2Int(FloorDivInt(cellX, _chunkSize),
                FloorDivInt(cellY, _chunkSize));
        }

        //FloorDiv logic not needed here since we are working with float values
        private Vector2Int GetCellPosition(Vector3 pos)
        {
            return new Vector2Int((int)(pos.x / _cellSize) - (pos.x < 0 ? 1 : 0),
                (int)(pos.y / _cellSize) - (pos.y < 0 ? 1 : 0));
        }

        private HashSet<T> CreateCell()
        {
            return new HashSet<T>();
        }

        private HashSet<T>[,] CreateChunk()
        {
            return new HashSet<T>[_chunkSize, _chunkSize];
        }

        private void AddToCell(T obj, int cellX, int cellY)
        {
            var chunkPos = GetChunkPosition(cellX, cellY);
            if (!_grid.TryGetValue(chunkPos, out var chunk))
            {
                chunk = CreateChunk();
                _grid.Add(chunkPos, chunk);
            }

            var localX = CalculateChunkLocal(cellX);
            var localY = CalculateChunkLocal(cellY);

            var cell = chunk[localX, localY];
            if (cell == null)
            {
                cell = CreateCell();
                chunk[localX, localY] = cell;
            }
            cell.Add(obj);
        }

        private void RemoveFromCell(T obj, int cellX, int cellY)
        {
            var chunkPos = GetChunkPosition(cellX, cellY);

            var localX = CalculateChunkLocal(cellX);
            var localY = CalculateChunkLocal(cellY);

            if (!_grid.TryGetValue(chunkPos, out var chunk)) return;
            var cell = chunk[localX, localY];
            cell.Remove(obj);
        }

        private int CalculateChunkLocal(int value)
        {
            var local = (value % _chunkSize);
            if (local < 0)
                local += _chunkSize;
            return local;
        }

        private Rect GetCellBounds(int cellX, int cellY)
        {
            var rect = new Rect
            {
                min = new Vector2(cellX * _cellSize, cellY * _cellSize),
                max = new Vector2((cellX + 1) * _cellSize, (cellY + 1) * _cellSize)
            };
            return rect;
        }

        /// <summary>
        /// Clears all the cells in the grid
        /// </summary>
        private void ClearCells()
        {
            foreach (var pair in _grid)
            {
                var chunk = pair.Value;
                for (int x = 0; x < _chunkSize; x++)
                {
                    for (var y = 0; y < _chunkSize; y++)
                    {
                        var cell = chunk[x, y];
                        if (cell == null) continue;
                        cell.Clear();
                    }
                }
            }
        }
        /// <summary>
        /// Returns the center of a cell at (cellX, cellY, cellZ) in world space.
        /// </summary>
        private Vector2 GetCellCenter(int cellX, int cellY)
        {
            // For a cell from [cellX * _cellSize .. (cellX+1) * _cellSize],
            // the center is (cellX + 0.5) * _cellSize. Same for Y and Z.
            return new Vector2(
                (cellX + 0.5f) * _cellSize,
                (cellY + 0.5f) * _cellSize
            );
        }
        #endregion
    }
}