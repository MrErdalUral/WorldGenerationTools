using System;
using System.Collections.Generic;

namespace PoissonDiscSampling
{
    public class NodeGraph<T> : INodeGraph<T>
    {
        private readonly List<T> _nodes = new List<T>();
        private readonly List <(int,int)> _edges = new List<(int,int)>();
        private readonly List<int> _roots = new List<int>();

        public List <T> Nodes => _nodes;
        public List <(int,int)> Edges => _edges;
        public List<int> Roots =>_roots;

        public void Clear()
        {
            Nodes.Clear();
            Edges.Clear();
            Roots.Clear();
        }
    }
}