using System;
using System.Collections.Generic;
using UnityEngine;

namespace FourWinged.Grids.SpatialGrid
{
    public class SpatialGrid : IGrid
    {
        private readonly Dictionary<Vector3Int, HashSet<IGridObject>[,,]> _grid = new Dictionary<Vector3Int, HashSet<IGridObject>[,,]>();
        private readonly HashSet<IGridObject> _uniqueResults = new HashSet<IGridObject>();
        private readonly HashSet<IGridObject> _allObjects = new HashSet<IGridObject>();
        private readonly int _chunkSize;
        private float _cellSize;
        private float _cellRadius;


        public SpatialGrid(float cellSize, int chunkSize = 32)
        {
            _cellSize = cellSize;
            _chunkSize = chunkSize;
            _cellRadius = 0.5f * _cellSize * Mathf.Sqrt(3f);
        }
        /// <summary>
        /// Restructure cells with the new cellSize
        /// </summary>
        /// <param name="cellSize"></param>
        public void ResizeGridCells(float cellSize)
        {
            _cellSize = cellSize;
            _cellRadius = 0.5f * _cellSize * Mathf.Sqrt(3f);

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
        public void AddToGrid(IGridObject obj)
        {
            Vector3Int minCellPos = GetCellPosition(obj.BoundingBox.min);
            Vector3Int maxCellPos = GetCellPosition(obj.BoundingBox.max);
            for (int x = minCellPos.x; x <= maxCellPos.x; x++)
            {
                for (int y = minCellPos.y; y <= maxCellPos.y; y++)
                {
                    for (int z = minCellPos.z; z <= maxCellPos.z; z++)
                    {
                        AddToCell(obj, x, y, z);
                    }
                }
            }
            _allObjects.Add(obj);
        }

        /// <summary>
        /// Remove object from grid cells.
        /// </summary>
        /// <param name="obj"></param>
        public void RemoveFromGrid(IGridObject obj)
        {
            Vector3Int minCellPos = GetCellPosition(obj.BoundingBox.min);
            Vector3Int maxCellPos = GetCellPosition(obj.BoundingBox.max);

            for (int x = minCellPos.x; x <= maxCellPos.x; x++)
            {
                for (int y = minCellPos.y; y <= maxCellPos.y; y++)
                {
                    for (int z = minCellPos.z; z <= maxCellPos.z; z++)
                    {
                        RemoveFromCell(obj, x, y, z);
                    }
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
        public void MoveObject(IGridObject obj, Vector3 displacement)
        {
            Vector3Int prevMinCellPos = GetCellPosition(obj.BoundingBox.min - displacement);
            Vector3Int newMinCellPos = GetCellPosition(obj.BoundingBox.min);
            Vector3Int prevMaxCellPos = GetCellPosition(obj.BoundingBox.max - displacement);
            Vector3Int newMaxCellPos = GetCellPosition(obj.BoundingBox.max);

            if (prevMinCellPos == newMinCellPos && prevMaxCellPos == newMaxCellPos)
                return;

            // Remove from cells no longer in cell range
            for (int x = prevMinCellPos.x; x <= prevMaxCellPos.x; x++)
            {
                for (int y = prevMinCellPos.y; y <= prevMaxCellPos.x; y++)
                {
                    for (int z = prevMinCellPos.z; z <= prevMaxCellPos.z; z++)
                    {
                        if (x >= newMinCellPos.x && x <= newMaxCellPos.x &&
                            y >= newMinCellPos.y && y <= newMaxCellPos.y &&
                            z >= newMinCellPos.z && z <= newMaxCellPos.z)
                        {
                            continue;
                        }
                        RemoveFromCell(obj, x, y, z);
                    }
                }
            }

            // Add to new cells that was not previously in cell range
            for (int x = newMinCellPos.x; x <= newMaxCellPos.x; x++)
            {
                for (int y = newMinCellPos.y; y <= newMaxCellPos.y; y++)
                {
                    for (int z = newMinCellPos.z; z <= newMaxCellPos.z; z++)
                    {
                        if (x >= prevMinCellPos.x && x <= prevMaxCellPos.x &&
                            y >= prevMinCellPos.y && y <= prevMaxCellPos.y &&
                            z >= prevMinCellPos.z && z <= prevMaxCellPos.z)
                        {
                            continue;
                        }
                        AddToCell(obj, x, y, z);
                    }
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

        public bool CheckRadiusEmpty(float radius, Vector3 center)
        {
            Vector3Int minCellPos = GetCellPosition(center - Vector3.one * radius);
            Vector3Int maxCellPos = GetCellPosition(center + Vector3.one * radius);

            // Then figure out which chunks that cell range spans.
            Vector3Int minChunkPos = GetChunkPosition(minCellPos.x, minCellPos.y, minCellPos.z);
            Vector3Int maxChunkPos = GetChunkPosition(maxCellPos.x, maxCellPos.y, maxCellPos.z);

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

                    for (int chunkZ = minChunkPos.z; chunkZ <= maxChunkPos.z; chunkZ++)
                    {
                        var chunkPos = new Vector3Int(chunkX, chunkY, chunkZ);
                        if (!_grid.TryGetValue(chunkPos, out var chunk))
                            continue;

                        int chunkMinZ = chunkZ * _chunkSize;
                        int chunkMaxZ = chunkMinZ + (_chunkSize - 1);

                        int localMinZ = 0;
                        if (chunkMinZ < minCellPos.z)
                            localMinZ = CalculateChunkLocal(Math.Max(chunkMinZ, minCellPos.z));
                        int localMaxZ = _chunkSize - 1;
                        if (chunkMaxZ > maxCellPos.z)
                            localMaxZ = CalculateChunkLocal(Math.Min(chunkMaxZ, maxCellPos.z));

                        // Iterate the cells in this chunk that lie within our bounding box range
                        for (int lx = localMinX; lx <= localMaxX; lx++)
                        {
                            int cellX = chunkMinX + lx;
                            for (int ly = localMinY; ly <= localMaxY; ly++)
                            {
                                int cellY = chunkMinY + ly;
                                for (int lz = localMinZ; lz <= localMaxZ; lz++)
                                {
                                    var cell = chunk[lx, ly, lz];
                                    if (cell == null)
                                        continue;

                                    int cellZ = chunkMinZ + lz;

                                    //----------------------------------------------------
                                    // 1) Compute the center of this cell
                                    //----------------------------------------------------
                                    Vector3 cellCenter = GetCellCenter(cellX, cellY, cellZ);

                                    //----------------------------------------------------
                                    // 2) Distance-based culling logic:
                                    //
                                    //    d = distance(queryCenter, cellCenter)
                                    //    If (d + halfDiag < radius) => entire cell inside
                                    //    If (d > radius + halfDiag) => entire cell outside
                                    //    else => partial => check per-object
                                    //----------------------------------------------------
                                    float distToCenter = (cellCenter - center).magnitude;

                                    if (distToCenter < (radius - _cellRadius))
                                    {
                                        if (cell.Count > 1)
                                            return false;
                                    }
                                    else if (distToCenter > (radius + _cellRadius))
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
                                            float distSq = (obj.Position - center).sqrMagnitude;
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
                }
            }

            return true;
        }

        public void RadiusQuery(float radius, Vector3 center, List<IGridObject> resultList)
        {
            Bounds sphereBounds = new Bounds(center, Vector3.one * (2f * radius));
            Vector3Int minCellPos = GetCellPosition(sphereBounds.min);
            Vector3Int maxCellPos = GetCellPosition(sphereBounds.max);

            // Then figure out which chunks that cell range spans.
            Vector3Int minChunkPos = GetChunkPosition(minCellPos.x, minCellPos.y, minCellPos.z);
            Vector3Int maxChunkPos = GetChunkPosition(maxCellPos.x, maxCellPos.y, maxCellPos.z);

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

                    for (int chunkZ = minChunkPos.z; chunkZ <= maxChunkPos.z; chunkZ++)
                    {
                        var chunkPos = new Vector3Int(chunkX, chunkY, chunkZ);
                        if (!_grid.TryGetValue(chunkPos, out var chunk))
                            continue;

                        int chunkMinZ = chunkZ * _chunkSize;
                        int chunkMaxZ = chunkMinZ + (_chunkSize - 1);

                        int localMinZ = 0;
                        if (chunkMinZ < minCellPos.z)
                            localMinZ = CalculateChunkLocal(Math.Max(chunkMinZ, minCellPos.z));
                        int localMaxZ = _chunkSize - 1;
                        if (chunkMaxZ > maxCellPos.z)
                            localMaxZ = CalculateChunkLocal(Math.Min(chunkMaxZ, maxCellPos.z));

                        // Iterate the cells in this chunk that lie within our bounding box range
                        for (int lx = localMinX; lx <= localMaxX; lx++)
                        {
                            int cellX = chunkMinX + lx;
                            for (int ly = localMinY; ly <= localMaxY; ly++)
                            {
                                int cellY = chunkMinY + ly;
                                for (int lz = localMinZ; lz <= localMaxZ; lz++)
                                {
                                    var cell = chunk[lx, ly, lz];
                                    if (cell == null)
                                        continue;

                                    int cellZ = chunkMinZ + lz;

                                    //----------------------------------------------------
                                    // 1) Compute the center of this cell
                                    //----------------------------------------------------
                                    Vector3 cellCenter = GetCellCenter(cellX, cellY, cellZ);

                                    //----------------------------------------------------
                                    // 2) Distance-based culling logic:
                                    //
                                    //    d = distance(queryCenter, cellCenter)
                                    //    If (d + halfDiag < radius) => entire cell inside
                                    //    If (d > radius + halfDiag) => entire cell outside
                                    //    else => partial => check per-object
                                    //----------------------------------------------------
                                    float distToCenter = (cellCenter - center).magnitude;

                                    if (distToCenter < (radius - _cellRadius))
                                    {
                                        _uniqueResults.UnionWith(cell);
                                    }
                                    else if (distToCenter > (radius + _cellRadius))
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
                                            float distSq = (obj.Position - center).sqrMagnitude;
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
        public void BoxQuery(Bounds box, List<IGridObject> resultList)
        {
            Vector3Int minCellPos = GetCellPosition(box.min);
            Vector3Int maxCellPos = GetCellPosition(box.max);

            // Determine which chunks we need to cover
            Vector3Int minChunkPos = GetChunkPosition(minCellPos.x, minCellPos.y, minCellPos.z);
            Vector3Int maxChunkPos = GetChunkPosition(maxCellPos.x, maxCellPos.y, maxCellPos.z);

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

                    for (int chunkZ = minChunkPos.z; chunkZ <= maxChunkPos.z; chunkZ++)
                    {
                        var chunkPos = new Vector3Int(chunkX, chunkY, chunkZ);
                        if (!_grid.TryGetValue(chunkPos, out var chunk))
                            continue;

                        // Same logic for Z
                        int chunkMinZ = chunkZ * _chunkSize;
                        int chunkMaxZ = chunkMinZ + (_chunkSize - 1);

                        int localMinZ = 0;
                        if (chunkMinZ < minCellPos.z)
                            localMinZ = CalculateChunkLocal(Math.Max(chunkMinZ, minCellPos.z));

                        int localMaxZ = _chunkSize - 1;
                        if (chunkMaxZ > maxCellPos.z)
                            localMaxZ = CalculateChunkLocal(Math.Min(chunkMaxZ, maxCellPos.z));

                        for (int localX = localMinX; localX <= localMaxX; localX++)
                        {
                            int cellX = chunkMinX + localX;
                            for (int localY = localMinX; localY <= localMaxY; localY++)
                            {
                                int cellY = chunkMinY + localY;
                                for (int localZ = localMinZ; localZ <= localMaxZ; localZ++)
                                {
                                    var cell = chunk[localX, localMaxY, localZ];
                                    if (cell == null)
                                        continue;

                                    int cellZ = chunkMinZ + localZ;
                                    var bounds = GetCellBounds(cellX, cellY, cellZ);
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
                                            if (obj.BoundingBox.Intersects(box))
                                                _uniqueResults.Add(obj);
                                        }
                                    }
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

        private Vector3Int GetChunkPosition(int cellX, int cellY, int cellZ)
        {
            return new Vector3Int(FloorDivInt(cellX, _chunkSize),
                FloorDivInt(cellY, _chunkSize),
                FloorDivInt(cellZ, _chunkSize));
        }

        //FloorDiv logic not needed here since we are working with float values
        private Vector3Int GetCellPosition(Vector3 pos)
        {
            return new Vector3Int((int)(pos.x / _cellSize) - (pos.x < 0 ? 1 : 0),
                (int)(pos.y / _cellSize) - (pos.y < 0 ? 1 : 0),
                (int)(pos.z / _cellSize) - (pos.z < 0 ? 1 : 0));
        }

        private HashSet<IGridObject> CreateCell()
        {
            return new HashSet<IGridObject>();
        }

        private HashSet<IGridObject>[,,] CreateChunk()
        {
            return new HashSet<IGridObject>[_chunkSize, _chunkSize, _chunkSize];
        }

        private void AddToCell(IGridObject obj, int cellX, int cellY, int cellZ)
        {
            var chunkPos = GetChunkPosition(cellX, cellY, cellZ);
            if (!_grid.TryGetValue(chunkPos, out var chunk))
            {
                chunk = CreateChunk();
                _grid.Add(chunkPos, chunk);
            }

            var localX = CalculateChunkLocal(cellX);
            var localY = CalculateChunkLocal(cellY);
            var localZ = CalculateChunkLocal(cellZ);

            var cell = chunk[localX, localY, localZ];
            if (cell == null)
            {
                cell = CreateCell();
                chunk[localX, localY, localZ] = cell;
            }
            cell.Add(obj);
        }

        private void RemoveFromCell(IGridObject obj, int cellX, int cellY, int cellZ)
        {
            var chunkPos = GetChunkPosition(cellX, cellY, cellZ);

            var localX = CalculateChunkLocal(cellX);
            var localY = CalculateChunkLocal(cellY);
            var localZ = CalculateChunkLocal(cellZ);

            if (!_grid.TryGetValue(chunkPos, out var chunk)) return;
            var cell = chunk[localX, localY, localZ];
            cell.Remove(obj);
        }

        private int CalculateChunkLocal(int value)
        {
            var local = (value % _chunkSize);
            if (local < 0)
                local += _chunkSize;
            return local;
        }

        private Bounds GetCellBounds(int cellX, int cellY, int cellZ)
        {
            var bounds = new Bounds
            {
                min = new Vector3(cellX * _cellSize, cellY * _cellSize, cellZ * _cellSize),
                max = new Vector3((cellX + 1) * _cellSize, (cellY + 1) * _cellSize, (cellZ + 1) * _cellSize)
            };
            return bounds;
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
                        for (var z = 0; z < _chunkSize; z++)
                        {
                            var cell = chunk[x, y, z];
                            if (cell == null) continue;
                            cell.Clear();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Returns the center of a cell at (cellX, cellY, cellZ) in world space.
        /// </summary>
        private Vector3 GetCellCenter(int cellX, int cellY, int cellZ)
        {
            // For a cell from [cellX * _cellSize .. (cellX+1) * _cellSize],
            // the center is (cellX + 0.5) * _cellSize. Same for Y and Z.
            return new Vector3(
                (cellX + 0.5f) * _cellSize,
                (cellY + 0.5f) * _cellSize,
                (cellZ + 0.5f) * _cellSize
            );
        }
        #endregion
    }
}