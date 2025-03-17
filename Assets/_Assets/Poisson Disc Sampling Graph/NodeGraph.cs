using System;
using System.Collections.Generic;

namespace PoissonDiscSampling
{
    public class NodeGraph<T> : IDisposable
    {
        private readonly List<T> _nodes;
        private readonly List <(int,int)> _edges;
        private readonly List<int> _roots;

        public List <T> Nodes => _nodes;
        public List <(int,int)> Edges => _edges;
        public List<int> Roots =>_roots;

        public NodeGraph()
        {
            _nodes = new List<T>();
            _edges = new List<(int,int)>();
            _roots = new List<int>();
        }

        public void Dispose()
        {
            Nodes.Clear();
            Edges.Clear();
            Roots.Clear();
        }
    }
}