using System;
using System.Collections.Generic;
using Grids;
using Grids.SpatialGrid;
using PoissonDiscSampling.Settings;
using RandomNoise;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PoissonDiscSampling
{
    public class PoissonDiscSampler : IPoissonDiscSampler, IDisposable
    {
        private readonly IGridObject2DFactory _gridObjectFactory;
        private readonly INoise2D _noise2D;
        private readonly IGrid2D _grid;
        private readonly List<int> _activeSamples = new List<int>();
        private readonly INodeGraph<IGridObject2D> _nodeGraph = new NodeGraph<IGridObject2D>();

        public INodeGraph<IGridObject2D> NodeGraph => _nodeGraph;

        public PoissonDiscSampler(IGrid2D grid, IGridObject2DFactory gridObjectFactory, INoise2D noise2D)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _gridObjectFactory = gridObjectFactory ?? throw new ArgumentNullException(nameof(gridObjectFactory));
            _noise2D = noise2D ?? throw new ArgumentNullException(nameof(noise2D));
        }

        public INodeGraph<IGridObject2D> SamplePoints(IPoissonDiscSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var startTime = DateTimeOffset.Now;
            ClearCollections();

            CreateInitialNodes(settings);
            GenerateNodes(settings);

            Debug.Log($"Generated graph: {_nodeGraph.Nodes.Count} nodes");
            Debug.Log($"Generation time: {(DateTimeOffset.Now - startTime).TotalSeconds} seconds");

            return _nodeGraph;
        }

        private void CreateInitialNodes(IPoissonDiscSettings settings)
        {
            // Create a set of valid root nodes
            for (int i = 0; i < settings.NumberOfRoots; i++)
            {
                Vector2 position2D = GetRandomPositionWithinRegion(settings);
                float initialRadius = Mathf.Lerp(settings.MinRadius, settings.MaxRadius, _noise2D.GetValue(position2D.x, position2D.y));

                // Ensure the initial node's position is valid
                while (!IsValidPosition(settings, position2D, initialRadius))
                {
                    position2D = GetRandomPositionWithinRegion(settings);
                    initialRadius = Mathf.Lerp(settings.MinRadius, settings.MaxRadius, _noise2D.GetValue(position2D.x, position2D.y));
                }

                IGridObject2D initialNode = _gridObjectFactory.Create(position2D, initialRadius);
                AddNode(initialNode, _nodeGraph.Nodes.Count, isRoot: true);
            }
        }

        private void GenerateNodes(IPoissonDiscSettings settings)
        {
            // Continue sampling until there are no active nodes
            while (_activeSamples.Count > 0)
            {
                int randomActiveIndex = Random.Range(0, _activeSamples.Count);
                int parentIndex = _activeSamples[randomActiveIndex];
                IGridObject2D parentNode = _nodeGraph.Nodes[parentIndex];

                if (TryGenerateNodeAroundParent(settings, parentNode, out IGridObject2D newNode))
                {
                    _nodeGraph.Edges.Add((parentIndex, _nodeGraph.Nodes.Count));
                    AddNode(newNode, _nodeGraph.Nodes.Count);
                }
                else
                {
                    _activeSamples.RemoveAt(randomActiveIndex);
                }
            }
        }

        private Vector2 GetRandomPositionWithinRegion(IPoissonDiscSettings settings)
        {
            // For initial placement, use a fraction of the overall region size
            return Vector2.Scale(Random.insideUnitCircle, settings.RegionSize / 4);
        }

        private bool TryGenerateNodeAroundParent(IPoissonDiscSettings settings, IGridObject2D parentNode, out IGridObject2D newNode)
        {
            newNode = null;
            for (int i = 0; i < settings.NumSamplesBeforeRejection; i++)
            {
                float angle = Random.value * 360f * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                // Adjust new node radius using a parabolic transformation of noise
                float noise = _noise2D.GetValue(parentNode.Position2D.x, parentNode.Position2D.y);
                float adjustedNoise = 4 * noise * (1 - noise);
                float newRadius = Mathf.Lerp(settings.MinRadius, settings.MaxRadius, adjustedNoise);

                float distance = parentNode.Radius + newRadius;
                Vector2 candidatePosition = parentNode.Position2D + direction * distance;

                if (IsValidPosition(settings, candidatePosition, newRadius))
                {
                    newNode = _gridObjectFactory.Create(candidatePosition, newRadius);
                    return true;
                }
            }
            return false;
        }

        private bool IsValidPosition(IPoissonDiscSettings settings, Vector2 position, float radius)
        {
            // Check if the candidate position is within the allowed elliptical region bounds.
            float a = settings.RegionSize.x / 2f; // semi-major axis
            float b = settings.RegionSize.y / 2f; // semi-minor axis

            float normalizedX = position.x / a;
            float normalizedY = position.y / b;

            // If the point lies outside the ellipse, return false.
            if (normalizedX * normalizedX + normalizedY * normalizedY > 1)
            {
                return false;
            }

            return _grid.CheckRadiusEmpty(radius, position);
        }

        private void AddNode(IGridObject2D node, int index, bool isRoot = false)
        {
            _nodeGraph.Nodes.Add(node);
            if (isRoot)
            {
                _nodeGraph.Roots.Add(index);
            }
            _activeSamples.Add(index);
            _grid.AddToGrid(node);
        }

        private void ClearCollections()
        {
            _grid.Clear();
            _activeSamples.Clear();
            _nodeGraph.Clear();
        }

        public void Dispose()
        {
            ClearCollections();
            GC.SuppressFinalize(this);
        }
    }
}
