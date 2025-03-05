using R3;
using System.Collections.Generic;

namespace ClickCounter
{
    public class CounterModel : ICounterModel
    {
        private Dictionary<string, ReactiveProperty<int>> _counters;

        public CounterModel()
        {
            _counters = new Dictionary<string, ReactiveProperty<int>>();
        }

        public void AddToCount(string key, int clicks)
        {
            if (!_counters.ContainsKey(key)) return;
            var clickCounter = _counters[key];
            clickCounter.Value += clicks;
        }
        public ReadOnlyReactiveProperty<int> GetCounter(string key)
        {
            if (!_counters.ContainsKey(key))
                _counters[key] = new ReactiveProperty<int>();
            return _counters[key];
        }
    }
}