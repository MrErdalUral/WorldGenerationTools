using Cysharp.Threading.Tasks;
using IslandGenerator.Installers;
using R3;
using UnityEngine;

namespace IslandGenerator
{
    public interface IIslandGenerator
    {
        UniTask<IslandDto> GenerateIsland(IslandGenerationSettings settings);
        Subject<IslandDto> OnIslandGenerated { get; }
    }
}