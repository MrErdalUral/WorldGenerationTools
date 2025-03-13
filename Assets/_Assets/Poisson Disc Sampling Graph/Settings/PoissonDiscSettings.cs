using System;
using R3;
using UnityEngine;


namespace FourWinged.PoissonGraph.Settings
{
    [CreateAssetMenu(fileName = "PoissonDiscSettings", menuName = "Poisson Disc/Settings")]
    public class PoissonDiscSettings : ScriptableObject
    {
        public Vector2 RegionSize = new Vector2(10, 10);
        public float MinY = 0f;
        public float MaxY = 5f;
        public float MinRadius = 0.5f;
        public float MaxRadius = 1.5f;
        public float MaxSlopeAngle = 30f;
        public int NumSamplesBeforeRejection = 30;
        public float VisualisationDelay = 0.1f;
        public int NoiseIterations = 5;
        
        
        public Subject<Unit> OnValuesChanged;

        void OnEnable()
        {
            OnValuesChanged = new Subject<Unit>();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                OnValuesChanged.OnNext(Unit.Default);
        }
    }
}