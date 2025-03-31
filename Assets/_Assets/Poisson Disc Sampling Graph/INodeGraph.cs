using System.Collections.Generic;

namespace PoissonDiscSampling
{
    public interface INodeGraph<T>
    {
        void Clear();
        List<T> Nodes { get; }
        List<(int, int)> Edges { get; }
        List<int> Roots { get; }
    }
}