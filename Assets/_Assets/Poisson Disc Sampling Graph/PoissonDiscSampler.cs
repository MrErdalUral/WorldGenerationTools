using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Grids;
using Grids.SpatialGrid;
using PoissonDiscSampling.Settings;
using R3;
using RandomNoise;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace PoissonDiscSampling
{
    public class PoissonDiscSampler : IPoissonDiscSampler, IDisposable
    {
        private readonly IGridObject2DFactory _gridObjectFactory;
        private readonly INoise2D _noise2d;
        private readonly List<int> _activeSamples = new List<int>();
        private readonly IGrid2D _grid;
        private readonly NodeGraph<IGridObject2D> _nodeGraph = new NodeGraph<IGridObject2D>();

        public NodeGraph<IGridObject2D> NodeGraph => _nodeGraph;

        public PoissonDiscSampler(IGrid2D grid, IGridObject2DFactory gridObjectFactory, INoise2D noise2d)
        {
            _grid = grid;
            _gridObjectFactory = gridObjectFactory;
            _noise2d = noise2d;
        }

        public NodeGraph<IGridObject2D> SamplePointsAsync(IPoissonDiscSettings settings)
        {
            var startTime = DateTimeOffset.Now;
            ClearCollections();

            for (int i = 0; i < settings.NumberOfRoots; i++)
            {
                var position2D = Vector2.Scale(Random.insideUnitCircle, settings.RegionSize / 4);
                float initialRadius = Mathf.Lerp(settings.MinRadius, settings.MaxRadius, _noise2d.GetValue(position2D.x, position2D.y));
                while (!IsValid(settings, position2D, initialRadius))
                {
                    position2D = Vector2.Scale(Random.insideUnitCircle, settings.RegionSize / 4);
                    initialRadius = Mathf.Lerp(settings.MinRadius, settings.MaxRadius, _noise2d.GetValue(position2D.x, position2D.y));
                }
                IGridObject2D initialNode = _gridObjectFactory.Create(position2D, initialRadius);
                AddNode(initialNode, i, true);
            }

            while (_activeSamples.Count > 0)
            {
                var activeIndex = Random.Range(0, _activeSamples.Count);
                var nodeIndex = _activeSamples[activeIndex];
                var sampleNode = _nodeGraph.Nodes[nodeIndex];
                if (TryGenerateNewNodeAroundParent(settings, sampleNode, out var newNode))
                {
                    _nodeGraph.Edges.Add((nodeIndex, _nodeGraph.Nodes.Count));
                    AddNode(newNode, _nodeGraph.Nodes.Count);
                }
                else
                {
                    _activeSamples.RemoveAt(activeIndex);
                }
            }
            Debug.Log("Generated graph: " + _nodeGraph.Nodes.Count + " nodes");
            Debug.Log("Generation time: " + (DateTimeOffset.Now - startTime).TotalSeconds);

            return _nodeGraph;

        }

        private void AddNode(IGridObject2D newNode, int index, bool isRoot = false)
        {
            _nodeGraph.Nodes.Add(newNode);
            if (isRoot)
                _nodeGraph.Roots.Add(index);
            _activeSamples.Add(index);
            _grid.AddToGrid(newNode);

        }

        private bool TryGenerateNewNodeAroundParent(IPoissonDiscSettings settings, IGridObject2D parentNode,
            out IGridObject2D newNode)
        {
            newNode = null;
            for (int i = 0; i < settings.NumSamplesBeforeRejection; i++)
            {
                float angle = Random.value * 360 * Mathf.Deg2Rad;
                var angleDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float noise = _noise2d.GetValue(parentNode.Position2D.x, parentNode.Position2D.y);
                var parabolicNoise = 4 * noise * (1 - noise);
                float newRadius = Mathf.Lerp(settings.MinRadius, settings.MaxRadius, parabolicNoise);
                float distance = parentNode.Radius + newRadius;
                Vector2 newPosition = parentNode.Position2D + angleDirection * distance;
                var isValid = IsValid(settings, newPosition, newRadius);
                if (isValid)
                {
                    newNode = _gridObjectFactory.Create(newPosition, newRadius);
                    return true;
                }
            }
            return false;
        }

        private bool IsValid(IPoissonDiscSettings settings, Vector2 position, float radius)
        {
            if (position.x < -settings.RegionSize.x / 2 ||
                position.x > settings.RegionSize.x / 2 ||
                position.y < -settings.RegionSize.y / 2 ||
                position.y > settings.RegionSize.y / 2)
                return false;
            return _grid.CheckRadiusEmpty(radius, position);
        }


        public void Dispose()
        {
            ClearCollections();
        }

        private void ClearCollections()
        {
            _grid.Clear();
            _activeSamples.Clear();
            _nodeGraph.Dispose();
        }
    }
}