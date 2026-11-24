using System.Globalization;

namespace UnPocoMas.Core.Randomness;

public static class Fnv1a32
{
    private const uint Offset = 0x811C9DC5;
    private const uint Prime = 0x01000193;

    public static uint Hash(string value)
    {
        uint h = Offset;

        foreach (char c in value)
        {
            h ^= c;

            unchecked
            {
                h *= Prime;
            }
        }

        return h;
    }

    public static uint DailySeed(DateOnly utcDate)
    {
        string text =
            $"UPM|v1|{utcDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

        return Hash(text);
    }
}
