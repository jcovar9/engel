using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class WaterMap : Node2D
{
    // private float _waterCutoff = 2;
    // [Export(PropertyHint.Range, "0.5,5.0,0.25")] public float WaterCutoff { get => _waterCutoff; set { _waterCutoff = value; FieldSet(); } }
    private int _width = 2;
    [Export(PropertyHint.Range, "2,16,2")] public int Width { get => _width; set { _width = value; FieldSet(); } }
    private int _height = 2;
    [Export(PropertyHint.Range, "2,16,2")] public int Height { get => _height; set { _height = value; FieldSet(); } }
    private uint _seed = 0;
    [Export] public uint Seed { get => _seed; set { _seed = value; FieldSet(); } }
    private int _chunkSize = 64;
    [Export] public int ChunkSize { get => _chunkSize; set { _chunkSize = value; FieldSet(); } }
    [Export] public Shader shader;
    private RNG rng;
    private Dictionary<Vector2I, WaterChunk> waterChunks;
    private Dictionary<Vector2I, Sprite2D> chunkSprites = new();
    
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
        foreach (Sprite2D sprite in chunkSprites.Values)
        {
            sprite.QueueFree();
        }
        chunkSprites.Clear();
        rng = new(_seed);
        for(int x = -_width / 2; x < _width / 2; x++)
        {
            for(int y = -_height / 2; y < _height / 2; y++)
            {
                Vector2I chunkOrigin = new(x * _chunkSize, y * _chunkSize);
                Vector2I voronoiPoint = GetVoronoiPoint(chunkOrigin);
                WaterChunk chunk = new(chunkOrigin, _chunkSize, voronoiPoint);
                waterChunks.Add(chunkOrigin, chunk);
            }
        }
        foreach (WaterChunk chunk in waterChunks.Values)
        {
            Vector2I[] voronoiPoints = Get3x3VoronoiPoints(chunk.origin);
            for(int x = 0; x < _chunkSize; x++)
            {
                for(int y = 0; y < _chunkSize;y++)
                {
                    float minDistanceVoronoi = float.MaxValue;
                    float minDistanceVoronoi2 = float.MaxValue;
                    float minDistanceVoronoi3 = float.MaxValue;
                    
                    Vector2I currPoint = new Vector2I(x,y) + chunk.origin;
                    foreach (Vector2I voronoiPoint in voronoiPoints)
                    {
                        float distance = currPoint.DistanceTo(voronoiPoint);
                        if(distance < minDistanceVoronoi)
                        {
                            minDistanceVoronoi3 = minDistanceVoronoi2;
                            minDistanceVoronoi2 = minDistanceVoronoi;
                            minDistanceVoronoi = distance;
                        }
                    }
                    if(minDistanceVoronoi3 - minDistanceVoronoi < 2.8f)
                    {
                        chunk.heightData[x + y * _chunkSize] = 0;
                    }
                    else if(minDistanceVoronoi2 - minDistanceVoronoi < 2.8f)
                    {
                        chunk.heightData[x + y * _chunkSize] = 1;
                    }
                    else if(minDistanceVoronoi < 1.0f)
                    {
                        chunk.heightData[x + y * _chunkSize] = 2;
                    }
                    else
                    {
                        chunk.heightData[x + y * _chunkSize] = 3;
                    }
                    
                }
            }
        }

        WaterChunk zeroChunk = waterChunks[new(0,0)];
        Vector2I[] voronoiPoint3x3 = Get3x3VoronoiPoints(zeroChunk.origin);
        SetLakeSeed(voronoiPoint3x3[4], voronoiPoint3x3[0], voronoiPoint3x3[1]);
        SetLakeSeed(voronoiPoint3x3[4], voronoiPoint3x3[0], voronoiPoint3x3[3]);
        SetLakeSeed(voronoiPoint3x3[4], voronoiPoint3x3[1], voronoiPoint3x3[3]);

        SetLakeSeed(voronoiPoint3x3[4], voronoiPoint3x3[1], voronoiPoint3x3[2]);
        SetLakeSeed(voronoiPoint3x3[4], voronoiPoint3x3[2], voronoiPoint3x3[5]);
        SetLakeSeed(voronoiPoint3x3[4], voronoiPoint3x3[1], voronoiPoint3x3[5]);
        foreach (WaterChunk waterChunk in waterChunks.Values)
        {
            CreateChunkSprite(waterChunk.origin, waterChunk.heightData);
        }
    }

    private List<Vector2I> GetValidLakeSeeds(Vector2I chunkOrigin)
    {
        Vector2I centerVoronoiPoint = GetVoronoiPoint(chunkOrigin);
        List<Vector2I> voronoiNeighbors = GetVoronoiNeighbors(chunkOrigin);
        List<Vector2I> validLakeSeeds = new();
        foreach (Vector2I voronoiPoint in voronoiNeighbors)
        {
            
        }

    }

    private static Vector2 GetCircumcenter(Vector2I voronoiA, Vector2I voronoiB, Vector2I voronoiC)
    {
        float voronoiASq = voronoiA.X * voronoiA.X + voronoiA.Y * voronoiA.Y;
        float voronoiBSq = voronoiB.X * voronoiB.X + voronoiB.Y * voronoiB.Y;
        float voronoiCSq = voronoiC.X * voronoiC.X + voronoiC.Y * voronoiC.Y;
        float BYminusCY = voronoiB.Y - voronoiC.Y;
        float CYminusAY = voronoiC.Y - voronoiA.Y;
        float AYminusBY = voronoiA.Y - voronoiB.Y;
        float denominator = 2.0f * (voronoiA.X * BYminusCY + voronoiB.X * CYminusAY + voronoiC.X * AYminusBY);
        float x = (voronoiASq * BYminusCY + voronoiBSq * CYminusAY + voronoiCSq * AYminusBY) / denominator;
        float y = (voronoiASq * (voronoiC.X - voronoiB.X) + voronoiBSq * (voronoiA.X - voronoiC.X) + voronoiCSq * (voronoiB.X - voronoiA.X)) / denominator;
        return new(x,y);
    }

    private void CreateChunkSprite(Vector2I origin, int[] chunkHeightData)
    {
        Image image = Image.CreateEmpty(_chunkSize, _chunkSize, false, Image.Format.Rgba8);
        ImageTexture imageTex = ImageTexture.CreateFromImage(image);
        ShaderMaterial shaderMaterial = new()
        {
            Shader = shader
        };
        shaderMaterial.SetShaderParameter("chunkHeightData", chunkHeightData);
        shaderMaterial.SetShaderParameter("chunkSize", _chunkSize);
        Sprite2D sprite2D = new()
        {
            Position = origin + new Vector2I(_chunkSize / 2, _chunkSize / 2),
            Texture = imageTex,
            Material = shaderMaterial
        };
        AddChild(sprite2D);
        chunkSprites.Add(origin, sprite2D);
    }

    private List<Vector2I> GetVoronoiNeighbors(Vector2I chunkOrigin)
    {
        List<Vector2I> origins = GetChunkNeighbors(chunkOrigin);
        for(int x = 0; x < 8; x++)
        {
            origins[x] = GetVoronoiPoint(origins[x]);
        }
        return origins;
    }

    private List<Vector2I> GetChunkNeighbors(Vector2I chunkOrigin)
    {
        return new(){
            new Vector2I(-1, -1) * _chunkSize + chunkOrigin,
            new Vector2I( 0, -1) * _chunkSize + chunkOrigin,
            new Vector2I( 1, -1) * _chunkSize + chunkOrigin,
            new Vector2I(-1,  0) * _chunkSize + chunkOrigin,
            new Vector2I( 1,  0) * _chunkSize + chunkOrigin,
            new Vector2I(-1,  1) * _chunkSize + chunkOrigin,
            new Vector2I( 0,  1) * _chunkSize + chunkOrigin,
            new Vector2I( 1,  1) * _chunkSize + chunkOrigin,
        };
    }

    private Vector2I GetChunkOrigin(Vector2 pos)
    {
        return new(Mathf.FloorToInt(pos.X / _chunkSize) * _chunkSize, Mathf.FloorToInt(pos.Y / _chunkSize) * _chunkSize);
    }

    private Vector2I GetVoronoiPoint(Vector2I chunkOrigin)
    {
        if(waterChunks.ContainsKey(chunkOrigin))
        {
            return waterChunks[chunkOrigin].voronoiPoint;
        }
        else
        {
            return rng.GetRandVec2I(chunkOrigin, _chunkSize / 2, _chunkSize / 2) + new Vector2I(_chunkSize, _chunkSize) / 4 + chunkOrigin;
        }
    }

}
