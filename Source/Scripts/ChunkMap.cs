using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class ChunkMap : Node2D
{
    private uint _seed = 0;
    [Export] public uint Seed { get => _seed; set { _seed = value; FieldSet(); } }
    private int _mapSize = 1;
    [Export(PropertyHint.Range, "0,16,1")] public int MapSize { get => _mapSize; set { _mapSize = value; FieldSet(); } }
    private int _chunkSize = 64;
    [Export] public int ChunkSize { get => _chunkSize; set { _chunkSize = value; FieldSet(); } }
    private int _viewSize = 100;
    [Export] public int ViewSize { get => _viewSize; set { _viewSize = value; FieldSet(); } }
    [Export] public Shader shader;
    private RNG rng;
    private Dictionary<Vector2I, VoronoiChunk> chunks;
    private Sprite2D spriteView;
    private int[] heightData;

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
        if(IsInsideTree())
        {
            chunks = new();
            rng = new(_seed);
            for(int x = -_mapSize; x <= _mapSize; x++)
            {
                for(int y = -_mapSize; y <= _mapSize; y++)
                {
                    Vector2I chunkOrigin = new(x * _chunkSize, y * _chunkSize);
                    VoronoiChunk chunk = new(chunkOrigin, _chunkSize, rng);
                    chunks.Add(chunkOrigin, chunk);
                }
            }
            heightData = new int[_viewSize * _viewSize];
            foreach (VoronoiChunk chunk in chunks.Values)
            {
                foreach (Vector2I edgeTile in chunk.GetBorderTiles())
                {
                    SetChunkTile(edgeTile, heightData, 1);
                }
                Vector2I voronoiOrigin = new(Mathf.FloorToInt(chunk.voronoiOrigin.X), Mathf.FloorToInt(chunk.voronoiOrigin.Y));
                SetChunkTile(voronoiOrigin, heightData, 2);
                foreach (Tuple<Vector2, float> chunkVertex in chunk.chunkVertexes)
                {
                    Vector2I vertexTile = new(Mathf.FloorToInt(chunkVertex.Item1.X), Mathf.FloorToInt(chunkVertex.Item1.Y));
                    SetChunkTile(vertexTile, heightData, 3);
                }
            }
            DrawView();
        }
    }

    private void DrawView()
    {
        if(spriteView != null)
        {
            spriteView.QueueFree();
        }
        Image image = Image.CreateEmpty(_viewSize, _viewSize, false, Image.Format.Rgba8);
        ImageTexture imageTex = ImageTexture.CreateFromImage(image);
        ShaderMaterial shaderMaterial = new()
        {
            Shader = shader
        };
        shaderMaterial.SetShaderParameter("chunkHeightData", heightData);
        shaderMaterial.SetShaderParameter("viewSize", _viewSize);
        shaderMaterial.SetShaderParameter("chunkSize", _chunkSize);
        spriteView = new()
        {
            Texture = imageTex,
            Material = shaderMaterial
        };
        AddChild(spriteView);
    }

    private void SetChunkTile(Vector2I chunkTile, int[] heightData, int value)
    {
        Vector2I offsetChunkTile = chunkTile + new Vector2I(_viewSize - _chunkSize, _viewSize - _chunkSize) / 2;
        if(0 <= offsetChunkTile.X && offsetChunkTile.X < _viewSize &&
        0 <= offsetChunkTile.Y && offsetChunkTile.Y < _viewSize)
        {
            heightData[offsetChunkTile.X + offsetChunkTile.Y * _viewSize] = value;
        }
    }

    private Vector2I GetChunkOrigin(Vector2 pos)
    {
        return new(Mathf.FloorToInt(pos.X / _chunkSize) * _chunkSize, Mathf.FloorToInt(pos.Y / _chunkSize) * _chunkSize);
    }


}
