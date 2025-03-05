using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FourWinged.PoissonGraph.Settings;
using FourWinged.SpatialGrid;
using R3;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace FourWinged.PoissonGraph
{
    public class PoissonDiscGraphModel : IGraphModel, IDisposable, IInitializable
    {
        private readonly PoissonDiscSettings _settings;
        private readonly List<GraphNode> _activeSamples = new List<GraphNode>();
        private readonly Graph _graph = new Graph();
        private readonly SpatialGrid<GraphNode> _spatialGrid;
        private readonly Subject<Graph> _onGraphUpdated = new Subject<Graph>();

        private DisposableBag _disposableBag;

        public Graph Graph => _graph;
        public Subject<Graph> OnGraphUpdated => _onGraphUpdated;

        public PoissonDiscGraphModel(PoissonDiscSettings settings, SpatialGrid<GraphNode> spatialGrid)
        {
            _settings = settings;
            _spatialGrid = spatialGrid;
        }

        public void Initialize()
        {
            _settings
                .OnValuesChanged
                .Subscribe(_ => GenerateGraph().Forget())
                .AddTo(ref _disposableBag);
            GenerateGraph().Forget();
        }

        public async UniTask GenerateGraph()
        {
            var startTime = DateTimeOffset.Now;

            ClearCollections();

            float initialRadius = Mathf.PerlinNoise(_settings.PerlinOffset.x * _settings.PerlinScale,
                _settings.PerlinOffset.y * _settings.PerlinScale);
            GraphNode initialNode = new GraphNode(0, Vector3.zero, initialRadius);
            AddNode(initialNode);
            while (_activeSamples.Count > 0)
            {
                int activeIndex = Random.Range(0, _activeSamples.Count);
                GraphNode sampleNode = _activeSamples[activeIndex];
                bool found = false;
                float angle = Random.value * 360 * Mathf.Deg2Rad;
                for (int i = 0; i < _settings.NumSamplesBeforeRejection; i++)
                {
                    GraphNode newNode = GenerateNodeAroundAngle(sampleNode, angle);
                    if (!IsValid(newNode))
                    {
                        angle = Random.value * 360 * Mathf.Deg2Rad;
                        continue;
                    }

                    _graph.Edges.Add((sampleNode.Index, newNode.Index));
                    AddNode(newNode);
                    found = true;
                    if (_settings.VisualisationDelay > 0)
                    {
                        _onGraphUpdated.OnNext(_graph);
                        await UniTask.WaitForSeconds(_settings.VisualisationDelay);
                    }
                    break;
                }

                if (!found)
                    _activeSamples.RemoveAt(activeIndex);
            }
            _onGraphUpdated.OnNext(_graph);

            Debug.Log("Generated graph: " + _graph.Nodes.Count + " nodes");
            Debug.Log("Generation time: " + (DateTimeOffset.Now - startTime).TotalSeconds);
        }

        private void AddNode(GraphNode newNode)
        {
            _graph.Nodes.Add(newNode);
            _activeSamples.Add(newNode);
            _spatialGrid.AddToGrid(newNode);
        }

        private GraphNode GenerateNodeAroundAngle(GraphNode node, float angle)
        {
            float newRadius = Random.Range(_settings.MinRadius, _settings.MaxRadius);
            float distance = node.Radius + newRadius;
            Vector3 newPosition = node.Position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;
            float perlinValue = 0;
            for (int i = 0; i < _settings.PerlinIterations; i++)
            {
                perlinValue = Mathf.PerlinNoise((newPosition.x + _settings.PerlinOffset.x) * _settings.PerlinScale,
                    (newPosition.z + _settings.PerlinOffset.y) * _settings.PerlinScale);
                var r = (1 - 2 * perlinValue) * (1 - 2 * perlinValue);
                newRadius = Mathf.Lerp(_settings.MaxRadius, _settings.MinRadius, r);
                distance = node.Radius + newRadius;
                newPosition = node.Position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;
            }

            // Apply max slope constraint for Y position
            float maxYChange = Mathf.Tan(_settings.MaxSlopeAngle * Mathf.Deg2Rad) * distance;
            float newY = node.Position.y + Mathf.Lerp(-maxYChange, maxYChange, perlinValue);
            newY = Mathf.Clamp(newY, _settings.MinY, _settings.MaxY);
            newPosition.y = newY;


            return new GraphNode(_graph.Nodes.Count, newPosition, newRadius);
        }

        private bool IsValid(GraphNode node)
        {
            if (node.Position.x < -_settings.RegionSize.x / 2 ||
                node.Position.x > _settings.RegionSize.x / 2 ||
                node.Position.z < -_settings.RegionSize.y / 2 ||
                node.Position.z > _settings.RegionSize.y / 2)
                return false;
            return _spatialGrid.CheckRadiusEmpty(node.BoundingRadius, node.BoundingPosition);
        }


        public void Dispose()
        {
            _disposableBag.Clear();
            ClearCollections();
        }

        private void ClearCollections()
        {
            _spatialGrid.Clear();
            _activeSamples.Clear();
            _graph.Dispose();
        }
    }
}