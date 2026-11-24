namespace UnPocoMas.Core.Randomness;

public sealed class Mulberry32
{
    private uint _state;

    public Mulberry32(uint seed)
    {
        _state = seed;
    }

    public double NextDouble()
    {
        unchecked
        {
            _state += 0x6D2B79F5;

            uint t = _state;

            t = (uint)(((t ^ (t >> 15)) * (1u | t)));

            t ^= t + (uint)(((t ^ (t >> 7)) * (61u | t)));

            uint result = t ^ (t >> 14);

            return result / 4294967296d;
        }
    }
}
