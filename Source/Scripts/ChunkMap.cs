using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class ChunkMap : Node2D
{
    [Export] public Options options;
    private Dictionary<Vector2I, List<Tuple<Vector2I, Vector2I>>> chunks;
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
                    Vector2I[] voronoiOrigins3x3 = GetVoronoiOrigins3x3(origin);
                    AddVertexesAndEdges(voronoiOrigins3x3);
                }
            }
            foreach (ChunkVertex vertex in vertexes.Values)
            {
                vertex.SetupConnections();
            }
            foreach (ChunkEdge edge in edges.Values)
            {
                foreach (KeyValuePair<Vector2I, float> borderTile in edge.GetBorderTiles(edge.voronoiOrigins[0]))
                {
                    SetTileHeight(borderTile.Key, borderTile.Value);
                    SetTileType(borderTile.Key, 4);
                }
                foreach (KeyValuePair<Vector2I, float> borderTile in edge.GetBorderTiles(edge.voronoiOrigins[1]))
                {
                    SetTileHeight(borderTile.Key, borderTile.Value);
                    SetTileType(borderTile.Key, 4);
                }
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

    // public HashSet<Tuple<Vector2I, float>> GetBorderTiles(Vector2I[] voronoiOrigins3x3, Vector2I origin)
    // {
    //     HashSet<Tuple<Vector2I, float>> borderTiles = new();
    //     HashSet<Tuple<Vector2I, float>> edgeTiles = GetEdges(origin);
    //     foreach (Tuple<Vector2I, float> edgeTile in edgeTiles)
    //     {
    //         if(GetClosestVoronoiOrigin(voronoiOrigins3x3, edgeTile.Item1) != voronoiOrigins3x3[4])
    //         {
    //             foreach (Vector2I offsetTile in GetHVTiles(edgeTile.Item1, 1))
    //             {
    //                 Tuple<Vector2I, float> offsetTileWithHeight = new(offsetTile, edgeTile.Item2);
    //                 if(!borderTiles.Contains(offsetTileWithHeight) && GetClosestVoronoiOrigin(voronoiOrigins3x3, offsetTile) == voronoiOrigins3x3[4])
    //                 {
    //                     borderTiles.Add(offsetTileWithHeight);
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             borderTiles.Add(edgeTile);
    //         }
    //     }
    //     return borderTiles;
    // }

    // public HashSet<Tuple<Vector2I, float>> GetEdges(Vector2I origin)
    // {
    //     HashSet<Tuple<Vector2I, float>> edgeTiles = new();
    //     foreach (Tuple<Vector2I, Vector2I> connection in chunks[origin])
    //     {
    //         AddEdge(edgeTiles, vertexes[connection.Item1], vertexes[connection.Item2]);
    //     }
    //     return edgeTiles;
    // }

    // private static Vector2I GetClosestVoronoiOrigin(Vector2I[] voronoiOrigins3x3, Vector2I pos)
    // {
    //     float minDist = float.MaxValue;
    //     Vector2I minVoronoiOrigin = voronoiOrigins3x3[0];
    //     foreach (Vector2I currVoronoiOrigin in voronoiOrigins3x3)
    //     {
    //         float currDist = pos.DistanceTo(currVoronoiOrigin);
    //         if(currDist < minDist)
    //         {
    //             minVoronoiOrigin = currVoronoiOrigin;
    //             minDist = currDist;
    //         }
    //     }
    //     return minVoronoiOrigin;
    // }

    private void AddVertexesAndEdges(Vector2I[] voronoiOrigins3x3)//, Vector2I origin)
    {
        List<Tuple<ChunkVertex, float>> chunkVertexes = GetVertexes(voronoiOrigins3x3);
        int firstIndex = chunkVertexes.Count - 1;
        for(int i = -1; i < chunkVertexes.Count - 1; i++)
        {
            ChunkVertex vertex1 = chunkVertexes[firstIndex].Item1;
            ChunkVertex vertex2 = chunkVertexes[i + 1].Item1;
            Tuple<Vector2I, Vector2I> edgeID = ChunkEdge.GetEdgeID(vertex1, vertex2);
            if(!edges.ContainsKey(edgeID))
            {
                ChunkEdge edge = new(vertex1, vertex2);
                edges[edgeID] = edge;

                if(!vertexes.ContainsKey(edge.vertex1.position))
                {
                    vertexes[edge.vertex1.position] = edge.vertex1;
                }
                if(!vertexes.ContainsKey(edge.vertex2.position))
                {
                    vertexes[edge.vertex2.position] = edge.vertex2;
                }
                vertexes[edge.vertex1.position].connections.Add(edge.vertex2);
                vertexes[edge.vertex2.position].connections.Add(edge.vertex1);
            }
            // if(!chunks.ContainsKey(origin))
            // {
            //     chunks[origin] = new();
            // }
            // chunks[origin].Add(new(currVertexTile, nextVertexTile));
            firstIndex = i + 1;
        }
    }

    private List<Tuple<ChunkVertex, float>> GetVertexes(Vector2I[] voronoiOrigins3x3)
    {
        List<Tuple<ChunkVertex, float>> unorderedVertexes = new();
        for(int y = 0; y <= 2; y += 2)
        {
            for(int x = 0; x <= 2; x += 2)
            {
                int cornerIndex = x + y * 3;
                Vector2I cornerP = voronoiOrigins3x3[cornerIndex];
                Vector2I sideP1 = voronoiOrigins3x3[cornerIndex - (x - 1)];
                Vector2I sideP2 = voronoiOrigins3x3[cornerIndex - (y - 1) * 3];
                TryAddChunkVertex(unorderedVertexes, voronoiOrigins3x3[4], cornerP, sideP1, sideP2);
                TryAddChunkVertex(unorderedVertexes, voronoiOrigins3x3[4], cornerP, sideP2, sideP1);
                TryAddChunkVertex(unorderedVertexes, voronoiOrigins3x3[4], sideP1, sideP2, cornerP);
            }
        }
        return unorderedVertexes.OrderBy(vertex => vertex.Item2).ToList();
    }

    private void TryAddChunkVertex(List<Tuple<ChunkVertex, float>> unorderedVertexes, Vector2I P1, Vector2I P2, Vector2I P3, Vector2I checkP)
    {
        Vector2I vertexPos = GetVertexPosition(P1, P2, P3);
        float centerDist = vertexPos.DistanceTo(P1);
        float checkDist = vertexPos.DistanceTo(checkP);
        if(centerDist < checkDist)
        {
            float height = (options.chunkVertexNoise.GetNoise2Dv(vertexPos) + 1.0f) * 0.5f;
            float angle = ((Vector2)P1).AngleToPoint((Vector2)vertexPos);
            ChunkVertex v = new(vertexPos, height, new() { P1, P2, P3, });
            unorderedVertexes.Add(new(v, angle));
        }
    }

    private static Vector2I GetVertexPosition(Vector2I voronoiA, Vector2I voronoiB, Vector2I voronoiC)
    {
        int voronoiASq = voronoiA.X * voronoiA.X + voronoiA.Y * voronoiA.Y;
        int voronoiBSq = voronoiB.X * voronoiB.X + voronoiB.Y * voronoiB.Y;
        int voronoiCSq = voronoiC.X * voronoiC.X + voronoiC.Y * voronoiC.Y;
        int BYminusCY = voronoiB.Y - voronoiC.Y;
        int CYminusAY = voronoiC.Y - voronoiA.Y;
        int AYminusBY = voronoiA.Y - voronoiB.Y;
        float denominator = 2 * (voronoiA.X * BYminusCY + voronoiB.X * CYminusAY + voronoiC.X * AYminusBY);
        float x = (voronoiASq * BYminusCY + voronoiBSq * CYminusAY + voronoiCSq * AYminusBY) / denominator;
        float y = (voronoiASq * (voronoiC.X - voronoiB.X) + voronoiBSq * (voronoiA.X - voronoiC.X) + voronoiCSq * (voronoiB.X - voronoiA.X)) / denominator;
        return new(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
    }

    private Vector2I[] GetVoronoiOrigins3x3(Vector2I origin)
    {
        return new Vector2I[9]{
            GetVoronoiPoint(new(origin.X - options.ChunkSize, origin.Y - options.ChunkSize)),
            GetVoronoiPoint(new(origin.X                    , origin.Y - options.ChunkSize)),
            GetVoronoiPoint(new(origin.X + options.ChunkSize, origin.Y - options.ChunkSize)),

            GetVoronoiPoint(new(origin.X - options.ChunkSize, origin.Y)),
            GetVoronoiPoint(origin),
            GetVoronoiPoint(new(origin.X + options.ChunkSize, origin.Y)),

            GetVoronoiPoint(new(origin.X - options.ChunkSize, origin.Y + options.ChunkSize)),
            GetVoronoiPoint(new(origin.X                    , origin.Y + options.ChunkSize)),
            GetVoronoiPoint(new(origin.X + options.ChunkSize, origin.Y + options.ChunkSize))
        };
    }

    // private Vector2[] GetHVDVoronoiOrigins(Vector2I origin)
    // {
    //     return new Vector2[8]{
    //         GetVoronoiPoint(new(origin.X - options.ChunkSize, origin.Y - options.ChunkSize)),
    //         GetVoronoiPoint(new(origin.X                    , origin.Y - options.ChunkSize)),
    //         GetVoronoiPoint(new(origin.X + options.ChunkSize, origin.Y - options.ChunkSize)),

    //         GetVoronoiPoint(new(origin.X - options.ChunkSize, origin.Y)),
    //         GetVoronoiPoint(new(origin.X + options.ChunkSize, origin.Y)),

    //         GetVoronoiPoint(new(origin.X - options.ChunkSize, origin.Y + options.ChunkSize)),
    //         GetVoronoiPoint(new(origin.X                    , origin.Y + options.ChunkSize)),
    //         GetVoronoiPoint(new(origin.X + options.ChunkSize, origin.Y + options.ChunkSize))
    //     };
    // }

    // private static Vector2I[] GetHVDTiles(Vector2I p, int offset)
    // {
    //     return new Vector2I[8]{
    //         new(p.X - offset, p.Y - offset), new(p.X, p.Y - offset), new(p.X + offset, p.Y - offset), 
    //         new(p.X - offset, p.Y         ),                         new(p.X + offset, p.Y         ), 
    //         new(p.X - offset, p.Y + offset), new(p.X, p.Y + offset), new(p.X + offset, p.Y + offset)};
    // }

    // private static Vector2I[] GetHVTiles(Vector2I p, int offset)
    // {
    //     return new Vector2I[4]{new(p.X, p.Y - offset), new(p.X - offset, p.Y), new(p.X + offset, p.Y), new(p.X, p.Y + offset)};
    // }

    private Vector2I GetVoronoiPoint(Vector2I origin)
    {
        Vector2I offset = new(origin.X + options.ChunkSize / 4, origin.Y + options.ChunkSize / 4);
        return options.rng.GetRandVec2I(origin, options.ChunkSize / 2, options.ChunkSize / 2) + offset;
    }
    private Vector2I GetChunkOrigin(Vector2 pos)
    {
        return new Vector2I(Mathf.FloorToInt(pos.X / options.ChunkSize), Mathf.FloorToInt(pos.Y / options.ChunkSize)) * options.ChunkSize;
    }
}
