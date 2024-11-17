using Godot;
using System;

public partial class RNG : RefCounted
{
    private uint Seed;
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
    public RNG(Vector2I position)
    {
        Seed = (uint)(position.X + (position.Y * LargePrimeNum));
        Next = Seed;
    }

    public void SetSeed(int seed) { Seed = (uint)seed; Next = Seed; }
    public void SetSeedV(Vector2I seed) { SetSeed(seed.X + (seed.Y * LargePrimeNum)); }

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
    private uint Get2DNoise(int x, int y)
    {
        return Get1DNoise((uint)(x + (LargePrimeNum * y)));
    }

    public float GetRandRatio(Vector2I pos)
    {
        return Mathf.InverseLerp(int.MinValue, int.MaxValue, (int)Get2DNoise(pos.X, pos.Y));
    }

    public int GetRandRange(Vector2I pos, int from, int to)
    {
        if(from == to) { return from; }
        return (int)Mathf.Lerp(from, to, GetRandRatio(pos));
    }

    public Vector2I GetRand2DRange(Vector2I pos, int rows, int cols)
    {
        int Rand = GetRandRange(pos, 0, rows * cols);
        int x = Rand / rows;
        int y = Rand - (x * rows);
        return new Vector2I(x, y);
    }

    // NEXT Functions
    public void ResetNext() { Next = Seed; }

    public int NextRandRange(int from, int to)
    {
        if(from == to) { return from; }
        Next = Get1DNoise(Next);
        return (int)Mathf.Lerp(from, to, Mathf.InverseLerp(int.MinValue, int.MaxValue, (int)Next));
    }

    public Vector2I NextRand2DRange(int rows, int cols)
    {
        int Rand = NextRandRange(0, rows * cols);
        int x = Rand / rows;
        int y = Rand - (x * rows);
        return new Vector2I(x, y);
    }
}
