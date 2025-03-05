using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace ClickCounter
{
    public class CounterView : MonoBehaviour, IPoolable<string, IMemoryPool>
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
            set { _counterText.text = $"{_label}: {value}"; }
        }

        public void OnDespawned()
        {
            _pool.Despawn(this);
        }

        public void OnSpawned(string label, IMemoryPool pool)
        {
            _label = label;
            _pool = pool;
        }

        public class Factory : PlaceholderFactory<string, CounterView>
        {

        }
    }
}