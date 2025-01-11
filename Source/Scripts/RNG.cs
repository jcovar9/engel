using Godot;

public partial class RNG
{
    private readonly uint Seed;
    private uint Next;
    private const uint BitNoise1 = 0x68E31DA4;
    private const uint BitNoise2 = 0xB5297A4D;
    private const uint BitNoise3 = 0x1B56C4E9;
    private const int LargePrimeNum = 198491317;

    public RNG(uint seed)
    {
        Seed = seed;
        Next = seed;
    }

    private uint Get1DNoise(uint position)
    {
        uint mangled = position;
        mangled *= BitNoise1;
        mangled += Seed;
        mangled ^= mangled >> 8;
        mangled += BitNoise2;
        mangled ^= mangled << 8;
        mangled *= BitNoise3;
        mangled ^= mangled >> 8;
        return mangled;
    }
    private uint Get2DNoise(Vector2I vec)
    {
        return Get1DNoise((uint)(vec.X + (LargePrimeNum * vec.Y)));
    }

    private float Vec2IToRandRatio(Vector2I pos)
    {
        return Mathf.InverseLerp(uint.MinValue, uint.MaxValue, Get2DNoise(pos));
    }

    public int GetRandRange(Vector2I pos, int from, int to)
    {
        if (from == to) { return from; }
        return (int)Mathf.Lerp(from, to, Vec2IToRandRatio(pos));
    }

    public Vector2I GetRandVec2I(Vector2I pos, int width, int height)
    {
        int rand = GetRandRange(pos, 0, width * height);
        int randY = rand / width;
        int randX = rand - randY * width;
        return new Vector2I(randX, randY);
    }

    // NEXT FUNCTIONS

    public void ResetNext() { Next = Seed; }

    private uint GetNext1DNoise(uint position)
    {
        uint mangled = position;
        mangled *= BitNoise1;
        mangled += Next;
        mangled ^= mangled >> 8;
        mangled += BitNoise2;
        mangled ^= mangled << 8;
        mangled *= BitNoise3;
        mangled ^= mangled >> 8;
        Next = mangled;
        return mangled;
    }

    private uint GetNext2DNoise(Vector2I vec)
    {
        return GetNext1DNoise((uint)(vec.X + (LargePrimeNum * vec.Y)));
    }

    private float Vec2IToNextRandRatio(Vector2I pos)
    {
        return Mathf.InverseLerp(uint.MinValue, uint.MaxValue, GetNext2DNoise(pos));
    }

    public int NextRandRange(Vector2I pos, int from, int to)
    {
        if (from == to) { return from; }
        return (int)Mathf.Lerp(from, to, Vec2IToNextRandRatio(pos));
    }

    public Vector2I NextRandVec2I(Vector2I pos, int width, int height)
    {
        int rand = NextRandRange(pos, 0, width * height);
        int x = rand / width;
        int y = rand - (x * width);
        return new Vector2I(x, y);
    }
}
