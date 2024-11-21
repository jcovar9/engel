using Godot;
using System;

public partial class RNG
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

    private float Vec2IToRandRatio(Vector2I pos)
    {
        return Mathf.InverseLerp(uint.MinValue, uint.MaxValue, Get2DNoise(pos.X, pos.Y));
    }

    // public float Vec2ToRandRatio(Vector2 pos)
    // {
    //     return Mathf.InverseLerp(uint.MinValue, uint.MaxValue, Get2DNoise((int)pos.X, (int)pos.Y));
    // }

    public int GetRandRange(Vector2I pos, int from, int to)
    {
        if(from == to) { return from; }
        return (int)Mathf.Lerp(from, to, Vec2IToRandRatio(pos));
    }

    public Vector2I GetRandVec2I(Vector2I pos, int width, int height)
    {
        int rand = GetRandRange(pos, 0, width * height);
        int randY = rand / width;
        int randX = rand - randY * width;
        return new Vector2I(randX, randY);
    }

    // public Vector2 GetRandVec2(Vector2 pos, float width, float height)
    // {
    //     Next = Get1DNoise(Next + (uint)(pos.X * pos.Y * LargePrimeNum));
    //     float randX = Mathf.Lerp(0.0f, width, Mathf.InverseLerp(uint.MinValue, uint.MaxValue, Next));
    //     Next = Get1DNoise(Next);
    //     float randY = Mathf.Lerp(0.0f, height, Mathf.InverseLerp(uint.MinValue, uint.MaxValue, Next));
    //     ResetNext();
    //     return new Vector2(randX, randY);
    // }

    // NEXT Functions
    public void ResetNext() { Next = Seed; }

    public int NextRandRange(int from, int to)
    {
        if(from == to) { return from; }
        Next = Get1DNoise(Next);
        return (int)Mathf.Lerp(from, to, Mathf.InverseLerp(uint.MinValue, uint.MaxValue, Next));
    }

    public Vector2I NextRand2DRange(int rows, int cols)
    {
        int Rand = NextRandRange(0, rows * cols);
        int x = Rand / rows;
        int y = Rand - (x * rows);
        return new Vector2I(x, y);
    }
}
