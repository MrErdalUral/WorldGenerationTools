using Cysharp.Threading.Tasks;
using IslandGenerator.Installers;
using IslandGenerator.Settings;
using R3;
using UnityEngine;

namespace IslandGenerator
{
    public interface IIslandGenerator
    {
        IslandDto GenerateIsland(IIslandGenerationSettings settings, int? overrideSeed = null);
    }
}