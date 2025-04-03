using System;
using R3;
using UnityEngine;


namespace PoissonDiscSampling.Settings
{
    [CreateAssetMenu(fileName = "PoissonDiscSettings", menuName = "Poisson Disc/Settings")]
    public class PoissonDiscSettings : ScriptableObject, IPoissonDiscSettings
    {
        [SerializeField]private Vector2 _regionSize = new Vector2(10, 10);
        [SerializeField]private float _minRadius = 0.5f;
        [SerializeField]private float _maxRadius = 1.5f;
        [SerializeField]private int _numSamplesBeforeRejection = 30;
        [SerializeField] [Range(1, 5)] private int _numberOfRoots;

        public Vector2 RegionSize => _regionSize;
        public float MinRadius => _minRadius;
        public float MaxRadius => _maxRadius;
        public int NumSamplesBeforeRejection => _numSamplesBeforeRejection;
        public int NumberOfRoots => _numberOfRoots;
    }
}