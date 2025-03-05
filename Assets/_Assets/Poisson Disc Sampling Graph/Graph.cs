using System;
using System.Collections.Generic;

namespace FourWinged.PoissonGraph
{
    public class Graph : IDisposable
    {
        private readonly List<GraphNode> _nodes;
        private readonly List <(int,int)> _edges;

        public List <GraphNode> Nodes => _nodes;
        public List <(int,int)> Edges => _edges;

        public Graph()
        {
            _nodes = new List<GraphNode>();
            _edges = new List<(int,int)>();
        }

        public void Dispose()
        {
            Nodes.Clear();
            Edges.Clear();
        }
    }
}