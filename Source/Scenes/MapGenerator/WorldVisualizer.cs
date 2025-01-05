using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class WorldVisualizer : Node2D
{
    [Export] public Options options;

    //private float[,] chunkVertexHeights = new float[100,100];
    //private HashSet<Vector2I> oceanTiles = new();
    private Sprite2D voronoiGridSprite;

    public void Init()
    {
        DrawView();
    }

    // private void SetOceanTiles()
    // {
    //     oceanTiles.Clear();
    //     HashSet<Vector2I> seenTiles = new();
    //     Queue<Vector2I> tileQueue = new();
    //     Vector2I currOceanTile = new(0, 0);
    //     tileQueue.Enqueue(currOceanTile);
    //     seenTiles.Add(currOceanTile);
    //     while (0 < tileQueue.Count)
    //     {

    //     }
    // }

    // private float GetHeight(float x, float y)
    // {
    //     float height = (options.chunkVertexNoise.GetNoise2D(x, y) + 1.0f) * 0.5f;
    //     float xDistanceMultiplier = 1f - Mathf.Pow(x * 2 / options.MapSize, 2f);
    //     float yDistanceMultiplier = 1f - Mathf.Pow(y * 2 / options.MapSize, 2f);
    //     return height * xDistanceMultiplier * yDistanceMultiplier;
    // }

    private void DrawView()
    {
        if (voronoiGridSprite != null)
        {
            voronoiGridSprite.QueueFree();
        }
        voronoiGridSprite = new()
        {
            Texture = new VoronoiGrid(options).GetImgTex(),
            TextureFilter = TextureFilterEnum.Nearest,
        };
        AddChild(voronoiGridSprite);
    }

    // private byte[] GetImgData()
    // {
    //     int chunksPerSide = options.MapSize / options.ChunkSize;
    //     byte[] data = new byte[4 * chunksPerSide * chunksPerSide];

    //     for (int pixelX = 0; pixelX < chunksPerSide; pixelX++)
    //     {
    //         for (int pixelY = 0; pixelY < chunksPerSide; pixelY++)
    //         {
    //             int imgDataIndex = (pixelX + pixelY * chunksPerSide) * 4;
    //             int xPos = (pixelX - chunksPerSide / 2) * options.ChunkSize + options.ChunkSize / 2;
    //             int yPos = (pixelY - chunksPerSide / 2) * options.ChunkSize + options.ChunkSize / 2;
    //             float height = GetHeight(xPos, yPos);
    //             byte colorVal = (byte)(int)(255 * height);
    //             if (height < 0.2f)
    //             {
    //                 data[imgDataIndex + 2] = (byte)(colorVal + 192);
    //             }
    //             else if (height < 0.21f)
    //             {
    //                 data[imgDataIndex] = (byte)(colorVal + 128);
    //                 data[imgDataIndex + 1] = (byte)(colorVal + 128);
    //             }
    //             else if (height < 0.5f)
    //             {
    //                 data[imgDataIndex + 1] = colorVal;
    //             }
    //             else if (height < 0.7f)
    //             {
    //                 data[imgDataIndex] = (byte)(colorVal / 2);
    //                 data[imgDataIndex + 1] = (byte)(colorVal / 2);
    //                 data[imgDataIndex + 2] = (byte)(colorVal / 2);
    //             }
    //             else
    //             {
    //                 data[imgDataIndex] = colorVal;
    //                 data[imgDataIndex + 1] = colorVal;
    //                 data[imgDataIndex + 2] = colorVal;
    //             }
    //             data[imgDataIndex + 3] = 255;
    //         }
    //     }
    //     return data;
    // }

}


/* REQUIREMENTS:
-rivers and lakes that make sense
    -vertexes for lakes to center on and rivers to connect vertexes
-edges of chunks are continuous
-noise throughout the terrain
*/