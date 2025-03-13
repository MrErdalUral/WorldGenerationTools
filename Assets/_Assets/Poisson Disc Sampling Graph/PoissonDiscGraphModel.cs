using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FourWinged.Grids;
using FourWinged.Grids.SpatialGrid;
using FourWinged.PoissonGraph.Settings;
using R3;
using RandomNoise;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace FourWinged.PoissonGraph
{
    public class PoissonDiscGraphModel<T> : IGraphModel<T>, IDisposable, IInitializable where T : IGridObject2D
    {
        private readonly IGridObjectFactory<T> _gridObjectFactory;
        private readonly INoise2D _noise2d;
        private readonly PoissonDiscSettings _settings;
        private readonly List<int> _activeSamples = new List<int>();
        private readonly NodeGraph<T> _nodeGraph = new NodeGraph<T>();
        private readonly SpatialGrid2D<T> _grid;
        private readonly Subject<T> _onAddedNode = new Subject<T>();
        private readonly Subject<NodeGraph<T>> _onComplete = new Subject<NodeGraph<T>>();

        private DisposableBag _disposableBag;

        public NodeGraph<T> NodeGraph => _nodeGraph;
        public SpatialGrid2D<T> Grid => _grid;
        public Subject<T> OnAddedNode => _onAddedNode;
        public Subject<NodeGraph<T>> OnComplete => _onComplete;

        public PoissonDiscGraphModel(PoissonDiscSettings settings, SpatialGrid2D<T> grid, IGridObjectFactory<T> gridObjectFactory, INoise2D noise2d)
        {
            _settings = settings;
            _grid = grid;
            _gridObjectFactory = gridObjectFactory;
            _noise2d = noise2d;
        }

        public void Initialize()
        {
            GenerateGraphAsync().Forget();
        }

        public async UniTask GenerateGraphAsync()
        {
            var startTime = DateTimeOffset.Now;
            ClearCollections();
            float initialRadius = Mathf.Lerp(_settings.MinRadius, _settings.MaxRadius, _noise2d.GetValue(0, 0));
            T initialNode = _gridObjectFactory.Create(Vector2.zero, initialRadius);
            AddNode(initialNode, 0);
            if (_settings.VisualisationDelay >= 0)
            {
                _onAddedNode.OnNext(initialNode);
                await UniTask.WaitForSeconds(_settings.VisualisationDelay);
            }

            while (_activeSamples.Count > 0)
            {
                var activeIndex = Random.Range(0, _activeSamples.Count);
                var nodeIndex = _activeSamples[activeIndex];
                var sampleNode = _nodeGraph.Nodes[nodeIndex];
                if (TryGenerateNodeAroundAngle(sampleNode, out var newNode))
                {
                    _nodeGraph.Edges.Add((nodeIndex, _nodeGraph.Nodes.Count));
                    AddNode(newNode, _nodeGraph.Nodes.Count);
                    if (_settings.VisualisationDelay >= 0)
                    {
                        await UniTask.WaitForSeconds(_settings.VisualisationDelay);
                    }
                }
                else
                {
                    _activeSamples.RemoveAt(activeIndex);
                }
            }
            _onComplete.OnNext(_nodeGraph);
            Debug.Log("Generated graph: " + _nodeGraph.Nodes.Count + " nodes");
            Debug.Log("Generation time: " + (DateTimeOffset.Now - startTime).TotalSeconds);
        }

        private void AddNode(T newNode, int index)
        {
            _nodeGraph.Nodes.Add(newNode);
            _activeSamples.Add(index);
            _grid.AddToGrid(newNode);
            _onAddedNode.OnNext(newNode);
        }
        
        private bool TryGenerateNodeAroundAngle(T parentNode, out T newNode)
        {
            newNode = default;
            for (int i = 0; i < _settings.NumSamplesBeforeRejection; i++)
            {
                float angle = Random.value * 360 * Mathf.Deg2Rad;
                var angleDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float newRadius = _settings.MaxRadius;
                float distance = parentNode.Radius + newRadius;
                Vector2 newPosition = parentNode.Position2D + angleDirection * distance;
                for (int n = 0; n < _settings.NoiseIterations; n++)
                {
                    newRadius = Mathf.Lerp(_settings.MinRadius, _settings.MaxRadius, _noise2d.GetValue(newPosition.x, newPosition.y));
                    distance = parentNode.Radius + newRadius;
                    newPosition = parentNode.Position2D + angleDirection * distance;
                }
                var isValid = IsValid(newPosition, newRadius);
                if (isValid)
                {
                    newNode = _gridObjectFactory.Create(newPosition, newRadius);
                    return true;
                }
            }
            return false;
        }

        private bool IsValid(Vector2 position, float radius)
        {
            if (position.x < -_settings.RegionSize.x / 2 ||
                position.x > _settings.RegionSize.x / 2 ||
                position.y < -_settings.RegionSize.y / 2 ||
                position.y > _settings.RegionSize.y / 2)
                return false;
            return _grid.CheckRadiusEmpty(radius, position);
        }


        public void Dispose()
        {
            _disposableBag.Dispose();
            ClearCollections();
        }

        private void ClearCollections()
        {
            _activeSamples.Clear();
            _nodeGraph.Dispose();
        }
    }
}