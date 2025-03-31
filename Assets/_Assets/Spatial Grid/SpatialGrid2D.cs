using System;
using System.Collections.Generic;
using UnityEngine;

namespace Grids.SpatialGrid
{
    public class SpatialGrid2D : IGrid2D
    {
        private readonly Dictionary<Vector2Int, HashSet<IGridObject2D>[,]> _grid = new Dictionary<Vector2Int, HashSet<IGridObject2D>[,]>();
        private readonly HashSet<IGridObject2D> _uniqueResults = new HashSet<IGridObject2D>();
        private readonly HashSet<IGridObject2D> _allObjects = new HashSet<IGridObject2D>();
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
        /// Resizes the grid cells with a new cell size, reassigning all objects.
        /// </summary>
        public void ResizeGridCells(float cellSize)
        {
            _cellSize = cellSize;
            _cellRadius = 0.5f * _cellSize * Mathf.Sqrt(2f);
            ClearCells();

            foreach (var obj in _allObjects)
            {
                AddToGrid(obj);
            }
        }

        /// <summary>
        /// Adds an object to all grid cells it occupies.
        /// </summary>
        public void AddToGrid(IGridObject2D obj)
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
        /// Removes an object from all grid cells it occupies.
        /// </summary>
        public void RemoveFromGrid(IGridObject2D obj)
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
        /// Updates the grid cells for an object that has moved.
        /// </summary>
        public void MoveObject(IGridObject2D obj, Vector2 displacement)
        {
            Vector2Int prevMin = GetCellPosition(obj.Rect.min - displacement);
            Vector2Int prevMax = GetCellPosition(obj.Rect.max - displacement);
            Vector2Int newMin = GetCellPosition(obj.Rect.min);
            Vector2Int newMax = GetCellPosition(obj.Rect.max);

            // If cell bounds have not changed, no update is needed.
            if (prevMin == newMin && prevMax == newMax)
                return;

            // Remove from cells no longer occupied.
            for (int x = prevMin.x; x <= prevMax.x; x++)
            {
                for (int y = prevMin.y; y <= prevMax.y; y++)
                {
                    if (x >= newMin.x && x <= newMax.x &&
                        y >= newMin.y && y <= newMax.y)
                    {
                        continue;
                    }
                    RemoveFromCell(obj, x, y);
                }
            }

            // Add to cells newly occupied.
            for (int x = newMin.x; x <= newMax.x; x++)
            {
                for (int y = newMin.y; y <= newMax.y; y++)
                {
                    if (x >= prevMin.x && x <= prevMax.x &&
                        y >= prevMin.y && y <= prevMax.y)
                    {
                        continue;
                    }
                    AddToCell(obj, x, y);
                }
            }
        }

        /// <summary>
        /// Clears the grid and all internal caches.
        /// </summary>
        public void Clear()
        {
            ClearCells();
            _uniqueResults.Clear();
            _uniqueResults.TrimExcess();
            _allObjects.Clear();
            _grid.Clear();
        }

        /// <summary>
        /// Checks whether the specified circular area is empty.
        /// </summary>
        public bool CheckRadiusEmpty(float radius, Vector2 center)
        {
            Vector2Int minCell = GetCellPosition(center - Vector2.one * radius);
            Vector2Int maxCell = GetCellPosition(center + Vector2.one * radius);
            Vector2Int minChunk = GetChunkPosition(minCell.x, minCell.y);
            Vector2Int maxChunk = GetChunkPosition(maxCell.x, maxCell.y);

            for (int chunkX = minChunk.x; chunkX <= maxChunk.x; chunkX++)
            {
                int chunkMinX = chunkX * _chunkSize;
                var (localMinX, localMaxX) = GetLocalBounds(chunkMinX, minCell.x, maxCell.x);

                for (int chunkY = minChunk.y; chunkY <= maxChunk.y; chunkY++)
                {
                    int chunkMinY = chunkY * _chunkSize;
                    var (localMinY, localMaxY) = GetLocalBounds(chunkMinY, minCell.y, maxCell.y);
                    var chunkPos = new Vector2Int(chunkX, chunkY);
                    if (!_grid.TryGetValue(chunkPos, out var chunk))
                        continue;

                    for (int lx = localMinX; lx <= localMaxX; lx++)
                    {
                        int cellX = chunkMinX + lx;
                        for (int ly = localMinY; ly <= localMaxY; ly++)
                        {
                            int cellY = chunkMinY + ly;
                            var cell = chunk[lx, ly];
                            if (cell == null || cell.Count < 1)
                                continue;

                            Vector2 cellCenter = GetCellCenter(cellX, cellY);
                            float distance = (cellCenter - center).magnitude;

                            if (distance <= (radius - _cellRadius))
                                return false;
                            else if (distance > (radius + _cellRadius))
                                continue;
                            else
                            {
                                foreach (var obj in cell)
                                {
                                    if (_uniqueResults.Contains(obj))
                                        continue;
                                    float distSq = (obj.Position2D - center).sqrMagnitude;
                                    float combined = obj.Radius + radius;
                                    if (distSq <= combined * combined)
                                        return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Adds all objects overlapping the specified circular area into the result list.
        /// </summary>
        public void RadiusQuery(float radius, Vector2 center, List<IGridObject2D> resultList)
        {
            Rect sphereBounds = new Rect(center - Vector2.one * radius, Vector2.one * (2f * radius));
            Vector2Int minCell = GetCellPosition(sphereBounds.min);
            Vector2Int maxCell = GetCellPosition(sphereBounds.max);
            Vector2Int minChunk = GetChunkPosition(minCell.x, minCell.y);
            Vector2Int maxChunk = GetChunkPosition(maxCell.x, maxCell.y);

            for (int chunkX = minChunk.x; chunkX <= maxChunk.x; chunkX++)
            {
                int chunkMinX = chunkX * _chunkSize;
                var (localMinX, localMaxX) = GetLocalBounds(chunkMinX, minCell.x, maxCell.x);

                for (int chunkY = minChunk.y; chunkY <= maxChunk.y; chunkY++)
                {
                    int chunkMinY = chunkY * _chunkSize;
                    var (localMinY, localMaxY) = GetLocalBounds(chunkMinY, minCell.y, maxCell.y);
                    var chunkPos = new Vector2Int(chunkX, chunkY);
                    if (!_grid.TryGetValue(chunkPos, out var chunk))
                        continue;

                    for (int lx = localMinX; lx <= localMaxX; lx++)
                    {
                        int cellX = chunkMinX + lx;
                        for (int ly = localMinY; ly <= localMaxY; ly++)
                        {
                            int cellY = chunkMinY + ly;
                            var cell = chunk[lx, ly];
                            if (cell == null || cell.Count < 1)
                                continue;

                            Vector2 cellCenter = GetCellCenter(cellX, cellY);
                            float distance = (cellCenter - center).magnitude;

                            if (distance <= (radius - _cellRadius))
                                _uniqueResults.UnionWith(cell);
                            else if (distance > (radius + _cellRadius))
                                continue;
                            else
                            {
                                foreach (var obj in cell)
                                {
                                    if (_uniqueResults.Contains(obj))
                                        continue;
                                    float distSq = (obj.Position2D - center).sqrMagnitude;
                                    float combined = obj.Radius + radius;
                                    if (distSq <= combined * combined)
                                        _uniqueResults.Add(obj);
                                }
                            }
                        }
                    }
                }
            }
            if (_uniqueResults.Count > 0)
            {
                resultList.AddRange(_uniqueResults);
                if (_uniqueResults.Count > 16)
                    _uniqueResults.TrimExcess();
            }
            _uniqueResults.Clear();
        }

        /// <summary>
        /// Adds all objects overlapping the specified box into the result list.
        /// </summary>
        public void BoxQuery(Rect box, List<IGridObject2D> resultList)
        {
            Vector2Int minCell = GetCellPosition(box.min);
            Vector2Int maxCell = GetCellPosition(box.max);
            Vector2Int minChunk = GetChunkPosition(minCell.x, minCell.y);
            Vector2Int maxChunk = GetChunkPosition(maxCell.x, maxCell.y);

            for (int chunkX = minChunk.x; chunkX <= maxChunk.x; chunkX++)
            {
                int chunkMinX = chunkX * _chunkSize;
                var (localMinX, localMaxX) = GetLocalBounds(chunkMinX, minCell.x, maxCell.x);

                for (int chunkY = minChunk.y; chunkY <= maxChunk.y; chunkY++)
                {
                    int chunkMinY = chunkY * _chunkSize;
                    var (localMinY, localMaxY) = GetLocalBounds(chunkMinY, minCell.y, maxCell.y);
                    var chunkPos = new Vector2Int(chunkX, chunkY);
                    if (!_grid.TryGetValue(chunkPos, out var chunk))
                        continue;

                    for (int lx = localMinX; lx <= localMaxX; lx++)
                    {
                        int cellX = chunkMinX + lx;
                        for (int ly = localMinY; ly <= localMaxY; ly++)
                        {
                            int cellY = chunkMinY + ly;
                            var cell = chunk[lx, ly];
                            if (cell == null)
                                continue;

                            Rect cellBounds = GetCellBounds(cellX, cellY);
                            if (box.Contains(cellBounds.min) && box.Contains(cellBounds.max))
                                _uniqueResults.UnionWith(cell);
                            else
                            {
                                foreach (var obj in cell)
                                {
                                    if (_uniqueResults.Contains(obj))
                                        continue;
                                    if (obj.Rect.Overlaps(box))
                                        _uniqueResults.Add(obj);
                                }
                            }
                        }
                    }
                }
            }
            if (_uniqueResults.Count > 0)
            {
                resultList.AddRange(_uniqueResults);
                if (_uniqueResults.Count > 16)
                    _uniqueResults.TrimExcess();
            }
            _uniqueResults.Clear();
        }

        #region Private Methods

        /// <summary>
        /// Returns the floored division result for negative numbers.
        /// </summary>
        private static int FloorDivInt(int value, int size) =>
            value >= 0 ? value / size : (value + 1) / size - 1;

        /// <summary>
        /// Converts a world position to a cell coordinate.
        /// </summary>
        private Vector2Int GetCellPosition(Vector3 pos)
        {
            int x = (int)(pos.x / _cellSize) - (pos.x < 0 ? 1 : 0);
            int y = (int)(pos.y / _cellSize) - (pos.y < 0 ? 1 : 0);
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// Converts cell coordinates to the corresponding chunk coordinate.
        /// </summary>
        private Vector2Int GetChunkPosition(int cellX, int cellY) =>
            new Vector2Int(FloorDivInt(cellX, _chunkSize), FloorDivInt(cellY, _chunkSize));

        /// <summary>
        /// Creates a new cell.
        /// </summary>
        private HashSet<IGridObject2D> CreateCell() => new HashSet<IGridObject2D>();

        /// <summary>
        /// Creates a new chunk.
        /// </summary>
        private HashSet<IGridObject2D>[,] CreateChunk() => new HashSet<IGridObject2D>[_chunkSize, _chunkSize];

        /// <summary>
        /// Adds an object to a specific cell.
        /// </summary>
        private void AddToCell(IGridObject2D obj, int cellX, int cellY)
        {
            Vector2Int chunkPos = GetChunkPosition(cellX, cellY);
            if (!_grid.TryGetValue(chunkPos, out var chunk))
            {
                chunk = CreateChunk();
                _grid.Add(chunkPos, chunk);
            }

            int localX = CalculateChunkLocal(cellX);
            int localY = CalculateChunkLocal(cellY);

            if (chunk[localX, localY] == null)
                chunk[localX, localY] = CreateCell();
            chunk[localX, localY].Add(obj);
        }

        /// <summary>
        /// Removes an object from a specific cell.
        /// </summary>
        private void RemoveFromCell(IGridObject2D obj, int cellX, int cellY)
        {
            Vector2Int chunkPos = GetChunkPosition(cellX, cellY);
            int localX = CalculateChunkLocal(cellX);
            int localY = CalculateChunkLocal(cellY);
            if (_grid.TryGetValue(chunkPos, out var chunk))
                chunk[localX, localY]?.Remove(obj);
        }

        /// <summary>
        /// Converts a global cell coordinate to a local chunk coordinate.
        /// </summary>
        private int CalculateChunkLocal(int value)
        {
            int local = value % _chunkSize;
            if (local < 0)
                local += _chunkSize;
            return local;
        }

        /// <summary>
        /// Returns the world-space bounds for a cell.
        /// </summary>
        private Rect GetCellBounds(int cellX, int cellY)
        {
            return new Rect
            {
                min = new Vector2(cellX * _cellSize, cellY * _cellSize),
                max = new Vector2((cellX + 1) * _cellSize, (cellY + 1) * _cellSize)
            };
        }

        /// <summary>
        /// Calculates the local bounds (min and max indices) for a chunk given global cell bounds.
        /// </summary>
        private (int localMin, int localMax) GetLocalBounds(int chunkStart, int globalMin, int globalMax)
        {
            int localMin = Mathf.Max(0, globalMin - chunkStart);
            int localMax = Mathf.Min(_chunkSize - 1, globalMax - chunkStart);
            return (localMin, localMax);
        }

        /// <summary>
        /// Clears all cells in the grid.
        /// </summary>
        private void ClearCells()
        {
            foreach (var kvp in _grid)
            {
                var chunk = kvp.Value;
                for (int x = 0; x < _chunkSize; x++)
                {
                    for (int y = 0; y < _chunkSize; y++)
                    {
                        chunk[x, y]?.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Returns the center of a cell in world space.
        /// </summary>
        private Vector2 GetCellCenter(int cellX, int cellY) =>
            new Vector2((cellX + 0.5f) * _cellSize, (cellY + 0.5f) * _cellSize);

        #endregion
    }
}
