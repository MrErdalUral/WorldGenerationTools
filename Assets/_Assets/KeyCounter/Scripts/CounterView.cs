using System;
using UnityEngine;
using TMPro;
using Zenject;

namespace KeyCounter
{
    public class CounterView : MonoBehaviour, IPoolable<string, IMemoryPool>, IDisposable
    {
        [SerializeField] private TMP_Text _counterText;
        private string _label;
        IMemoryPool _pool;

        public string Label
        {
            set => _label = value;
        }

        public int Count
        {
            set
            {
                {
                    transform.SetAsFirstSibling();
                    _counterText.text = $"{_label}: {value}";
                }
            }
        }

        public void OnDespawned()
        {
            _pool = null;
        }

        public void OnSpawned(string label, IMemoryPool pool)
        {
            _label = label;
            _pool = pool;
            transform.SetAsFirstSibling();
        }

        public class Factory : PlaceholderFactory<string, CounterView>
        {

        }

        public void Dispose()
        {
            _pool.Despawn(this);
        }
    }
}