using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class WaterMap : Node2D
{
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
                Vector2 voronoiPoint = GetVoronoiPoint(chunkOrigin);
                WaterChunk chunk = new(chunkOrigin, _chunkSize, voronoiPoint);
                waterChunks.Add(chunkOrigin, chunk);
            }
        }
        foreach (WaterChunk chunk in waterChunks.Values)
        {
            Vector2[] voronoiPoint3x3 = Get3x3VoronoiPoints(chunk.origin);
            int[] chunkHeightData = new int[_chunkSize * _chunkSize];
            for(int x = 0; x < _chunkSize; x++)
            {
                for(int y = 0; y < _chunkSize;y++)
                {
                    float minDist = float.MaxValue;
                    float minDist2 = float.MaxValue;
                    Vector2 offset = new(x + 0.5f,y + 0.5f);
                    foreach (Vector2 voronoiPoint in voronoiPoint3x3)
                    {
                        float dist = (chunk.origin + offset).DistanceTo(voronoiPoint);
                        if(dist < minDist)
                        {
                            minDist2 = minDist;
                            minDist = dist;
                        }
                    }
                    if(minDist2 - minDist < 1.5f)
                    {
                        chunkHeightData[x + y * _chunkSize] = 0;
                    }
                    else
                    {
                        chunkHeightData[x + y * _chunkSize] = 1;
                    }
                    
                }
            }
            CreateChunkSprite(chunk.origin, chunkHeightData);
        }
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
            Position = origin + new Vector2(_chunkSize / 2, _chunkSize / 2),
            Texture = imageTex,
            Material = shaderMaterial
        };
        AddChild(sprite2D);
        chunkSprites.Add(origin, sprite2D);
    }

    private Vector2[] Get3x3VoronoiPoints(Vector2I chunkOrigin)
    {
        Vector2I[] origins = Get3x3Chunks(chunkOrigin);
        Vector2[] ret = new Vector2[9];
        for(int x = 0; x < 9; x++)
        {
            ret[x] = GetVoronoiPoint(origins[x]);
        }
        return ret;
    }

    private Vector2I[] Get3x3Chunks(Vector2I chunkOrigin)
    {
        Vector2I[] ret = new Vector2I[9];
        for(int x = -1; x < 2; x++)
        {
            for(int y = -1; y < 2; y++)
            {
                ret[x + 1 + 3 * (y + 1)] = new Vector2I(x * _chunkSize, y * _chunkSize) + chunkOrigin;
            }
        }
        return ret;
    }

    private Vector2I GetChunkOrigin(Vector2 pos)
    {
        return new(Mathf.FloorToInt(pos.X / _chunkSize) * _chunkSize, Mathf.FloorToInt(pos.Y / _chunkSize) * _chunkSize);
    }

    private Vector2 GetVoronoiPoint(Vector2I chunkOrigin)
    {
        if(waterChunks.ContainsKey(chunkOrigin))
        {
            return waterChunks[chunkOrigin].voronoiPoint;
        }
        else
        {
            return rng.GetRandVec2(chunkOrigin, _chunkSize / 2, _chunkSize / 2) + new Vector2(_chunkSize, _chunkSize) / 4 + chunkOrigin;
        }
    }

}
