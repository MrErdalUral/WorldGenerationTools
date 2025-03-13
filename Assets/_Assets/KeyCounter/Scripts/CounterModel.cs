using R3;
using System.Collections.Generic;

namespace KeyCounter
{
    public class CounterModel : ICounterModel
    {
        private readonly Dictionary<string, ReactiveProperty<int>> _counters = new Dictionary<string, ReactiveProperty<int>>();

        public void AddToCount(string key, int clicks)
        {
            if (!_counters.TryGetValue(key, out var clickCounter)) return;
            clickCounter.Value += clicks;
        }
        public ReadOnlyReactiveProperty<int> GetCounter(string key)
        {
            if (!_counters.ContainsKey(key))
                _counters[key] = new ReactiveProperty<int>();
            return _counters[key];
        }

        public void ResetCounters()
        {
            _counters.Clear();
        }
    }
}