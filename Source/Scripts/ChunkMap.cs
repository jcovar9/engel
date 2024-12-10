using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class ChunkMap : Node2D
{
    [Export] public Options options;
    private Dictionary<Vector2I, Chunk> chunks;
    private Dictionary<Vector2I, ChunkVertex> vertexes;
    private Dictionary<Tuple<Vector2I, Vector2I>, ChunkEdge> edges;
    private Sprite2D spriteView;
    private float[] tileHeights;
    private int[] tileTypes;

    public void Init()
    {
        if(IsInsideTree())
        {
            vertexes = new();
            chunks = new();
            edges = new();
            tileHeights = new float[options.ViewSize * options.ViewSize];
            tileTypes = new int[options.ViewSize * options.ViewSize];
            for(int x = -options.MapSize; x <= options.MapSize; x++)
            {
                for(int y = -options.MapSize; y <= options.MapSize; y++)
                {
                    Vector2I origin = new(x * options.ChunkSize, y * options.ChunkSize);
                    Chunk chunk = new(options, origin);
                    chunks[origin] = chunk;
                    foreach (Tuple<BasicVertexInfo, BasicVertexInfo> edge in chunk.edges)
                    {
                        TryAddEdge(edge);
                    }
                }
            }
            foreach (ChunkVertex vertex in vertexes.Values)
            {
                vertex.SetupConnections();
            }
            if(options.EdgesInsteadBorders)
            {
                SetEdgeTiles();
            }
            else
            {
                SetBorderTiles();
            }
            foreach(ChunkVertex vertex in vertexes.Values)
            {
                if(vertex.connectionsHave3Connections)
                {
                    if(vertex.isLocalMinimum)
                    {
                        SetTileType(vertex.position, 1);
                    }
                    else if(vertex.isLocalMaximum)
                    {
                        SetTileType(vertex.position, 2);
                    }
                    else
                    {
                        SetTileType(vertex.position, 3);
                    }
                }
                else if(vertex.has3Connections)
                {
                    SetTileType(vertex.position, -1);
                }
                else
                {
                    SetTileType(vertex.position, -2);
                }
            }

            DrawView();
        }
    }

    private void SetEdgeTiles()
    {
        foreach (ChunkEdge edge in edges.Values)
        {
            foreach (KeyValuePair<Vector2, float> edgeTile in edge.edgeTiles)
            {
                Vector2I edgeTilePos = new(Mathf.FloorToInt(edgeTile.Key.X), Mathf.FloorToInt(edgeTile.Key.Y));
                SetTileHeight(edgeTilePos, edgeTile.Value);
                SetTileType(edgeTilePos, 4);
            }
        }
    }

    private void SetBorderTiles()
    {
        foreach (Chunk chunk in chunks.Values)
        {
            foreach (Tuple<BasicVertexInfo, BasicVertexInfo> basicEdge in chunk.edges)
            {
                ChunkEdge edge = edges[new(basicEdge.Item1.pos, basicEdge.Item2.pos)];
                foreach (KeyValuePair<Vector2I, float> borderTile in edge.GetBorderTiles(chunk.voronoiOrigins3x3[4]))
                {
                    SetTileHeight(borderTile.Key, borderTile.Value);
                    SetTileType(borderTile.Key, 4);
                }
            }
        }
    }

    private void TryAddEdge(Tuple<BasicVertexInfo, BasicVertexInfo> edge)
    {
        Tuple<Vector2I, Vector2I> edgeID = new(edge.Item1.pos, edge.Item2.pos);
        if(!edges.ContainsKey(edgeID))
        {
            ChunkVertex vertex1 = GetChunkVertex(edge.Item1);
            ChunkVertex vertex2 = GetChunkVertex(edge.Item2);
            vertex1.connections.Add(vertex2);
            vertex2.connections.Add(vertex1);
            edges[edgeID] = new(vertex1, vertex2);
        }
    }

    private ChunkVertex GetChunkVertex(BasicVertexInfo v)
    {
        if(vertexes.ContainsKey(v.pos))
        {
            return vertexes[v.pos];
        }
        else
        {
            ChunkVertex vertex = new(v.pos, GetHeight(v.pos), new(){v.vPoint1, v.vPoint2, v.vPoint3});
            vertexes[v.pos] = vertex;
            return vertex;
        }
    }

    private void DrawView()
    {
        if(spriteView != null)
        {
            spriteView.QueueFree();
        }
        Image image = Image.CreateEmpty(options.ViewSize, options.ViewSize, false, Image.Format.Rgba8);
        ImageTexture imageTex = ImageTexture.CreateFromImage(image);
        ShaderMaterial shaderMaterial = new()
        {
            Shader = options.shader
        };
        shaderMaterial.SetShaderParameter("tileHeights", tileHeights);
        shaderMaterial.SetShaderParameter("tileTypes", tileTypes);
        shaderMaterial.SetShaderParameter("viewSize", options.ViewSize);
        shaderMaterial.SetShaderParameter("chunkSize", options.ChunkSize);
        spriteView = new()
        {
            Texture = imageTex,
            Material = shaderMaterial
        };
        AddChild(spriteView);
    }

    private void SetTileType(Vector2I position, int type)
    {
        Vector2I offsetPosition = position + new Vector2I(options.ViewSize - options.ChunkSize, options.ViewSize - options.ChunkSize) / 2;
        if(0 <= offsetPosition.X && offsetPosition.X < options.ViewSize &&
        0 <= offsetPosition.Y && offsetPosition.Y < options.ViewSize)
        {
            tileTypes[offsetPosition.X + offsetPosition.Y * options.ViewSize] = type;
        }
    }
    private void SetTileHeight(Vector2I position, float height)
    {
        Vector2I offsetPosition = position + new Vector2I(options.ViewSize - options.ChunkSize, options.ViewSize - options.ChunkSize) / 2;
        if(0 <= offsetPosition.X && offsetPosition.X < options.ViewSize &&
        0 <= offsetPosition.Y && offsetPosition.Y < options.ViewSize)
        {
            tileHeights[offsetPosition.X + offsetPosition.Y * options.ViewSize] = height;
        }
    }

    private float GetHeight(Vector2I pos)
    {
        return (options.chunkVertexNoise.GetNoise2Dv(pos) + 1.0f) * 0.5f;
    }
    private Vector2I GetChunkOrigin(Vector2 pos)
    {
        return new Vector2I(Mathf.FloorToInt(pos.X / options.ChunkSize), Mathf.FloorToInt(pos.Y / options.ChunkSize)) * options.ChunkSize;
    }
}
