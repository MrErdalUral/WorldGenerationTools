namespace RandomNoise
{
    public interface INoise2D
    {
        float GetValue(float x, float y);
        void SetSeed(int seed);
    }
}