using R3;

namespace KeyCounter
{
    public interface ICounterModel
    {
        void AddToCount(string key, int clicks);
        ReadOnlyReactiveProperty<int> GetCounter(string key);
        void ResetCounters();
    }
}