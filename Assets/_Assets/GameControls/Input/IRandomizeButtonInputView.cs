using R3;

namespace GameControls.PlayerInput
{
    public interface IRandomizeButtonInputView
    {
        Observable<Unit> RandomizeButtonClicked { get; }
    }
}