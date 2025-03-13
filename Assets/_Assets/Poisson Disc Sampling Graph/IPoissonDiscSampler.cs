using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FourWinged.Grids;
using R3;

namespace FourWinged.PoissonGraph
{
    public interface IPoissonDiscSampler
    {
        UniTask SamplePointsAsync(); // Generates and returns the graph
        NodeGraph<IGridObject2D> NodeGraph { get; } 
        Subject<IGridObject2D> OnAddedNode { get; }
        Subject<NodeGraph<IGridObject2D>> OnComplete { get; }
    }
}