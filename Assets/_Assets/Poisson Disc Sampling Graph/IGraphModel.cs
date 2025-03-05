using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace FourWinged.PoissonGraph
{
    public interface IGraphModel
    {
        UniTask GenerateGraph(); // Generates and returns the graph
        
        Graph Graph { get; } 
        Subject<Graph> OnGraphUpdated { get; }
    }
}