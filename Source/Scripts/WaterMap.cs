using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class WaterMap : Node2D
{
    private int _halfMapSize = 1;
    [Export(PropertyHint.Range, "1,16,1")] public int HalfMapSize { get => _halfMapSize; set { _halfMapSize = value; FieldSet(); } }
    private uint _seed = 0;
    [Export] public uint Seed { get => _seed; set { _seed = value; FieldSet(); } }
    private int _chunkSize = 64;
    [Export] public int ChunkSize { get => _chunkSize; set { _chunkSize = value; FieldSet(); } }
    [Export] public Shader shader;
    private RNG rng;
    private Dictionary<Vector2I, WaterChunk> waterChunks;
    private Dictionary<Vector2I, Sprite2D> chunkSprites = new();
    private Dictionary<Tuple<Vector2,Vector2>, Dictionary<Vector2I,HashSet<Vector2I>>> rivers;
    
    private void FieldSet()
    {
        NotifyPropertyListChanged();
        Init();
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Init();
	}

    private void Init()
    {
        waterChunks = new();
        rivers = new();
        foreach (Sprite2D sprite in chunkSprites.Values)
        {
            sprite.QueueFree();
        }
        chunkSprites.Clear();
        rng = new(_seed);
        for(int x = -_halfMapSize; x <= _halfMapSize; x++)
        {
            for(int y = -_halfMapSize; y <= _halfMapSize; y++)
            {
                Vector2I chunkOrigin = new(x * _chunkSize, y * _chunkSize);
                WaterChunk chunk = new(chunkOrigin, _chunkSize, rng);
                waterChunks.Add(chunkOrigin, chunk);
            }
        }
        for(int x = -(_halfMapSize - 1); x <= _halfMapSize - 1; x++)
        {
            for(int y = -(_halfMapSize - 1); y <= _halfMapSize - 1; y++)
            {
                Vector2I currChunkOrigin = new(x * _chunkSize, y * _chunkSize);
                List<Tuple<Vector2, float>> lakeSeeds = waterChunks[currChunkOrigin].lakeSeeds;
                for(int i = 0; i < lakeSeeds.Count - 1; i++)
                {
                    AddRiver(lakeSeeds[i].Item1, lakeSeeds[i+1].Item1);
                }
                AddRiver(lakeSeeds[lakeSeeds.Count-1].Item1, lakeSeeds[0].Item1);
            }
        }
        foreach (WaterChunk chunk in waterChunks.Values)
        {
            Sprite2D chunkSprite = chunk.GetChunkSprite(shader);
            AddChild(chunkSprite);
            chunkSprites.Add(chunk.origin, chunkSprite);
        }
    }

    private void AddRiver(Vector2 lakeSeed1, Vector2 lakeSeed2)
    {
        Tuple<Vector2, Vector2> riverID;
        if(lakeSeed1.Length() < lakeSeed2.Length())
        {
            riverID = new(lakeSeed1, lakeSeed2);
        }
        else
        {
            riverID = new(lakeSeed2, lakeSeed1);
        }
        if(!rivers.ContainsKey(riverID))
        {
            rivers[riverID] = GetRiver(riverID.Item1, riverID.Item2);
            foreach (KeyValuePair<Vector2I, HashSet<Vector2I>> chunkRiverTiles in rivers[riverID])
            {
                if(waterChunks.ContainsKey(chunkRiverTiles.Key))
                {
                    foreach (Vector2I riverTile in chunkRiverTiles.Value)
                    {
                        Vector2I localRiverTile = riverTile - chunkRiverTiles.Key;
                        waterChunks[chunkRiverTiles.Key].heightData[localRiverTile.X + localRiverTile.Y * _chunkSize] = 2;
                    }
                }
            }
        }
    }

    public Dictionary<Vector2I, HashSet<Vector2I>> GetRiver(Vector2 start, Vector2 end)
    {
        Vector2 dir = (end - start).Normalized();
        Vector2 unitStepSize = new(Mathf.Sqrt(1.0f + (dir.Y / dir.X) * (dir.Y / dir.X)), Mathf.Sqrt(1.0f + (dir.X / dir.Y) * (dir.X / dir.Y)));
        Vector2I riverTile = new(Mathf.FloorToInt(start.X), Mathf.FloorToInt(start.Y));
        Dictionary<Vector2I, HashSet<Vector2I>> chunkRiverTiles = new();
        Vector2 lengthIn1D;
        Vector2I step;
        if(dir.X < 0)
        {
            step.X = -1;
            lengthIn1D.X = (start.X - riverTile.X) * unitStepSize.X;
        }
        else
        {
            step.X = 1;
            lengthIn1D.X = (riverTile.X + 1 - start.X) * unitStepSize.X;
        }
        if(dir.Y < 0)
        {
            step.Y = -1;
            lengthIn1D.Y = (start.Y - riverTile.Y) * unitStepSize.Y;
        }
        else
        {
            step.Y = 1;
            lengthIn1D.Y = (riverTile.Y + 1 - start.Y) * unitStepSize.Y;
        }
        Vector2I endTile = new(Mathf.FloorToInt(end.X), Mathf.FloorToInt(end.Y));
        float length = (end - start).Length();
        while(riverTile != endTile && (riverTile - start).Length() < length)
        {
            if(lengthIn1D.X < lengthIn1D.Y)
            {
                riverTile.X += step.X;
                lengthIn1D.X += unitStepSize.X;
            }
            else
            {
                riverTile.Y += step.Y;
                lengthIn1D.Y += unitStepSize.Y;
            }
            Vector2I currOrigin = GetChunkOrigin(riverTile);
            if(riverTile != endTile)
            {
                if(!chunkRiverTiles.ContainsKey(currOrigin))
                {
                    chunkRiverTiles.Add(currOrigin, new());
                }
                chunkRiverTiles[currOrigin].Add(riverTile);
            }
        }
        return chunkRiverTiles;
    }

        // foreach (WaterChunk chunk in waterChunks.Values)
        // {
        //     List<Vector2I> voronoiPoints = GetVoronoiNeighbors(chunk.origin);
        //     voronoiPoints.Add(GetVoronoiPoint(chunk.origin));
        //     for(int x = 0; x < _chunkSize; x++)
        //     {
        //         for(int y = 0; y < _chunkSize;y++)
        //         {
        //             float minDistanceVoronoi = float.MaxValue;
        //             float minDistanceVoronoi2 = float.MaxValue;
        //             float minDistanceVoronoi3 = float.MaxValue;
                    
        //             Vector2I currPoint = new Vector2I(x,y) + chunk.origin;
        //             foreach (Vector2I voronoiPoint in voronoiPoints)
        //             {
        //                 float distance = currPoint.DistanceTo(voronoiPoint);
        //                 if(distance < minDistanceVoronoi)
        //                 {
        //                     minDistanceVoronoi3 = minDistanceVoronoi2;
        //                     minDistanceVoronoi2 = minDistanceVoronoi;
        //                     minDistanceVoronoi = distance;
        //                 }
        //                 else if(distance < minDistanceVoronoi2)
        //                 {
        //                     minDistanceVoronoi3 = minDistanceVoronoi2;
        //                     minDistanceVoronoi2 = distance;
        //                 }
        //                 else if(distance < minDistanceVoronoi3)
        //                 {
        //                     minDistanceVoronoi3 = distance;
        //                 }
        //             }
        //             if(minDistanceVoronoi3 - minDistanceVoronoi < 1.0f)
        //             {
        //                 chunk.heightData[x + y * _chunkSize] = 0;
        //             }
        //             else if(minDistanceVoronoi2 - minDistanceVoronoi < 1.0f)
        //             {
        //                 chunk.heightData[x + y * _chunkSize] = 1;
        //             }
        //             else if(minDistanceVoronoi < 1.0f)
        //             {
        //                 chunk.heightData[x + y * _chunkSize] = 2;
        //             }
        //             else
        //             {
        //                 chunk.heightData[x + y * _chunkSize] = 3;
        //             }
                    
        //         }
        //     }
        // }

        // WaterChunk zeroChunk = waterChunks[new(0,0)];
        // foreach (Vector2I lakeSeed in GetValidLakeSeeds(zeroChunk.origin))
        // {
        //     Vector2I lakeSeedOrigin = GetChunkOrigin(lakeSeed);
        //     Vector2I localLakeSeed = lakeSeed - lakeSeedOrigin;
        //     if(waterChunks.ContainsKey(lakeSeedOrigin))
        //     {
        //         waterChunks[lakeSeedOrigin].heightData[localLakeSeed.X + localLakeSeed.Y * _chunkSize] = 4;
        //     }
        // }

    private Vector2I GetChunkOrigin(Vector2 pos)
    {
        return new(Mathf.FloorToInt(pos.X / _chunkSize) * _chunkSize, Mathf.FloorToInt(pos.Y / _chunkSize) * _chunkSize);
    }


}
