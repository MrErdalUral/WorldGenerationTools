using R3;

namespace ClickCounter
{
    public interface ICounterModel
    {
        void AddToCount(string key, int clicks);
        ReadOnlyReactiveProperty<int> GetCounter(string key);
    }
}