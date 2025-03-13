using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FourWinged.Grids;
using R3;

namespace FourWinged.PoissonGraph
{
    public interface IGraphModel<T>
    {
        UniTask GenerateGraphAsync(); // Generates and returns the graph
        NodeGraph<T> NodeGraph { get; } 
        Subject<T> OnAddedNode { get; }
        Subject<NodeGraph<T>> OnComplete { get; }
    }
}