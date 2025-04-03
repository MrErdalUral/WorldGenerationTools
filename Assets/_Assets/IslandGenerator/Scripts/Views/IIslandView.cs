using System;
using UnityEngine;
using Zenject;

namespace IslandGenerator.View
{
    public interface IIslandView : IPoolable<IslandDto, IMemoryPool>, IDisposable
    {
        Vector3 Position { set; }
    }
}