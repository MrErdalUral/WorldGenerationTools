namespace GameControls.PlayerInput
{
    public readonly struct InputSettings : IInputSettings
    {
        private readonly float _keyboardAxisMagnitude;

        public InputSettings(float keyboardAxisMagnitude)
        {
            _keyboardAxisMagnitude = keyboardAxisMagnitude;
        }

        public float KeyBoardAxisMagnitude => _keyboardAxisMagnitude;
    }
}