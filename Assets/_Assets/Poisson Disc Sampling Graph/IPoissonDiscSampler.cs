﻿using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Grids;
using PoissonDiscSampling.Settings;
using R3;

namespace PoissonDiscSampling
{
    public interface IPoissonDiscSampler
    {
        NodeGraph<IGridObject2D> SamplePointsAsync(IPoissonDiscSettings settings); // Generates and returns the graph
        NodeGraph<IGridObject2D> NodeGraph { get; }
    }
}