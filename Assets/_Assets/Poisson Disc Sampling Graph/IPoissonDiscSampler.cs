using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Grids;
using PoissonDiscSampling.Settings;
using R3;

namespace PoissonDiscSampling
{
    public interface IPoissonDiscSampler
    {
        INodeGraph<IGridObject2D> SamplePointsAsync(IPoissonDiscSettings settings); // Generates and returns the graph
        INodeGraph<IGridObject2D> NodeGraph { get; }
    }
}