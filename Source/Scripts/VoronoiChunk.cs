using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class VoronoiChunk
{
    public Vector2I gridOrigin;
    public int gridSize;
    private readonly RNG rng;
    public Vector2 voronoiOrigin;
    private readonly Vector2[] voronoiOrigins3x3;
    public List<Tuple<Vector2, float>> chunkVertexes;

    public VoronoiChunk(Vector2I _gridOrigin, int _gridSize, RNG _rng){
        gridOrigin = _gridOrigin;
        gridSize = _gridSize;
        rng = _rng;
        voronoiOrigin = GetVoronoiPoint(gridOrigin);
        voronoiOrigins3x3 = GetVoronoiOrigins3x3();
        chunkVertexes = GetChunkVertexes();
    }

    public HashSet<Vector2I> GetBorderTiles()
    {
        HashSet<Vector2I> borderTiles = new();
        HashSet<Vector2I> edgeTiles = GetEdges();
        foreach (Vector2I edgeTile in edgeTiles)
        {
            if(GetClosestVoronoiOrigin(edgeTile) != voronoiOrigin)
            {
                // int counter = 0;
                foreach (Vector2I offsetTile in GetHVTiles(edgeTile, 1))
                {
                    if(!borderTiles.Contains(offsetTile))
                    {
                        if(GetClosestVoronoiOrigin(offsetTile) == voronoiOrigin)
                        {
                            borderTiles.Add(offsetTile);
                        }
                        // else
                        // {
                        //     counter++;
                        // }
                    }
                }
                // if(4 == counter)
                // {
                //     Vector2I localTile = edgeTile - gridOrigin;
                //     GD.Print("Chunk: ", gridOrigin, " localTile: ", localTile, " has no valid HVNeighbors.");
                // }
            }
            else
            {
                borderTiles.Add(edgeTile);
            }
        }
        return borderTiles;
    }

    public HashSet<Vector2I> GetEdges()
    {
        HashSet<Vector2I> edgeTiles = new();
        for(int i = 0; i < chunkVertexes.Count - 1; i++)
        {
            Vector2 start = chunkVertexes[i].Item1;
            Vector2 end = chunkVertexes[i+1].Item1;
            AddEdge(edgeTiles, start, end);
        }
        AddEdge(edgeTiles, chunkVertexes[chunkVertexes.Count - 1].Item1, chunkVertexes[0].Item1);
        return edgeTiles;
    }

    private static void AddEdge(HashSet<Vector2I> edgeTiles, Vector2 start, Vector2 end)
    {
        Vector2 dir = (end - start).Normalized();
        Vector2 unitStepSize = new(Mathf.Sqrt(1.0f + (dir.Y / dir.X) * (dir.Y / dir.X)), Mathf.Sqrt(1.0f + (dir.X / dir.Y) * (dir.X / dir.Y)));
        Vector2I currEdgeTile = new(Mathf.FloorToInt(start.X), Mathf.FloorToInt(start.Y));
        Vector2 lengthIn1D;
        Vector2I step;
        if(dir.X < 0)
        {
            step.X = -1;
            lengthIn1D.X = (start.X - currEdgeTile.X) * unitStepSize.X;
        }
        else
        {
            step.X = 1;
            lengthIn1D.X = (currEdgeTile.X + 1 - start.X) * unitStepSize.X;
        }
        if(dir.Y < 0)
        {
            step.Y = -1;
            lengthIn1D.Y = (start.Y - currEdgeTile.Y) * unitStepSize.Y;
        }
        else
        {
            step.Y = 1;
            lengthIn1D.Y = (currEdgeTile.Y + 1 - start.Y) * unitStepSize.Y;
        }
        Vector2I endTile = new(Mathf.FloorToInt(end.X), Mathf.FloorToInt(end.Y));
        float maxLength = (endTile - start).Length();
        while((currEdgeTile - start).Length() <= maxLength)
        {
            edgeTiles.Add(currEdgeTile);
            if(lengthIn1D.X < lengthIn1D.Y)
            {
                currEdgeTile.X += step.X;
                lengthIn1D.X += unitStepSize.X;
            }
            else
            {
                currEdgeTile.Y += step.Y;
                lengthIn1D.Y += unitStepSize.Y;
            }
        }
    }

    // public HashSet<Vector2I> GetChunkTiles()
    // {
    //     HashSet<Vector2I> chunkTiles = new();
    //     HashSet<Vector2I> seenTiles = new();
    //     Queue<Vector2I> tileQ = new();
    //     tileQ.Enqueue(voronoiOrigin);
    //     while(0 < tileQ.Count)
    //     {
    //         Vector2I currTile = tileQ.Dequeue();
    //         chunkTiles.Add(currTile);
    //         foreach (Vector2I nextTile in GetHVNeighbors(currTile, 1))
    //         {
    //             if(!seenTiles.Contains(nextTile))
    //             {
    //                 seenTiles.Add(nextTile);
    //                 if(GetClosestVoronoiOrigin(nextTile) == voronoiOrigin)
    //                 {
    //                     tileQ.Enqueue(nextTile);
    //                 }
    //             }
    //         }
    //     }
    //     return chunkTiles;
    // }

    private Vector2 GetClosestVoronoiOrigin(Vector2 pos)
    {
        float minDist = float.MaxValue;
        Vector2 minVoronoiOrigin = voronoiOrigins3x3[0];
        foreach (Vector2 currVoronoiOrigin in voronoiOrigins3x3)
        {
            float currDist = pos.DistanceTo(currVoronoiOrigin);
            if(currDist < minDist)
            {
                minVoronoiOrigin = currVoronoiOrigin;
                minDist = currDist;
            }
        }
        return minVoronoiOrigin;
    }

    private List<Tuple<Vector2, float>> GetChunkVertexes()
    {
        List<Tuple<Vector2, float>> vertexes = new();
        Vector2[] HVDNeighborVoronoiOrigins = GetHVDNeighborVoronoiOrigins();
        for(int i = 0; i < HVDNeighborVoronoiOrigins.Length; i++)
        {
            for (int j = i + 1; j < HVDNeighborVoronoiOrigins.Length; j++)
            {
                Vector2 vertex = GetVertex(voronoiOrigin, HVDNeighborVoronoiOrigins[i], HVDNeighborVoronoiOrigins[j]);
                Vector2I vertexChunk = GetGridChunkOrigin(vertex);
                if(gridOrigin.X - gridSize <= vertexChunk.X && vertexChunk.X <= gridOrigin.X + gridSize &&
                gridOrigin.Y - gridSize <= vertexChunk.Y && vertexChunk.Y <= gridOrigin.Y + gridSize)
                {
                    float minDist1 = vertex.DistanceTo(voronoiOrigin);
                    float minDist2 = float.MaxValue;
                    float minDist3 = float.MaxValue;
                    foreach (Vector2 currVoronoiOrigin in HVDNeighborVoronoiOrigins)
                    {
                        float currDist = vertex.DistanceTo(currVoronoiOrigin);
                        if(currDist < minDist1)
                        {
                            minDist3 = minDist2;
                            minDist2 = minDist1;
                            minDist1 = currDist;
                        }
                        else if(currDist < minDist2)
                        {
                            minDist3 = minDist2;
                            minDist2 = currDist;
                        }
                        else if(currDist < minDist3)
                        {
                            minDist3 = currDist;
                        }
                    }
                    if(minDist3 - minDist1 < 0.1f)
                    {
                        vertexes.Add(new(vertex, voronoiOrigin.AngleToPoint(vertex)));
                    }
                }
            }
        }
        return vertexes.OrderBy(vertex => vertex.Item2).ToList();
    }

    private static Vector2 GetVertex(Vector2 voronoiA, Vector2 voronoiB, Vector2 voronoiC)
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

    private Vector2[] GetVoronoiOrigins3x3()
    {
        return new Vector2[9]{
            GetVoronoiPoint(new(gridOrigin.X - gridSize, gridOrigin.Y - gridSize)),
            GetVoronoiPoint(new(gridOrigin.X           , gridOrigin.Y - gridSize)),
            GetVoronoiPoint(new(gridOrigin.X + gridSize, gridOrigin.Y - gridSize)),

            GetVoronoiPoint(new(gridOrigin.X - gridSize, gridOrigin.Y)),
            voronoiOrigin,
            GetVoronoiPoint(new(gridOrigin.X + gridSize, gridOrigin.Y)),

            GetVoronoiPoint(new(gridOrigin.X - gridSize, gridOrigin.Y + gridSize)),
            GetVoronoiPoint(new(gridOrigin.X           , gridOrigin.Y + gridSize)),
            GetVoronoiPoint(new(gridOrigin.X + gridSize, gridOrigin.Y + gridSize))
        };
    }

    private Vector2[] GetHVDNeighborVoronoiOrigins()
    {
        return new Vector2[8]{
            GetVoronoiPoint(new(gridOrigin.X - gridSize, gridOrigin.Y - gridSize)),
            GetVoronoiPoint(new(gridOrigin.X           , gridOrigin.Y - gridSize)),
            GetVoronoiPoint(new(gridOrigin.X + gridSize, gridOrigin.Y - gridSize)),

            GetVoronoiPoint(new(gridOrigin.X - gridSize, gridOrigin.Y)),
            GetVoronoiPoint(new(gridOrigin.X + gridSize, gridOrigin.Y)),

            GetVoronoiPoint(new(gridOrigin.X - gridSize, gridOrigin.Y + gridSize)),
            GetVoronoiPoint(new(gridOrigin.X           , gridOrigin.Y + gridSize)),
            GetVoronoiPoint(new(gridOrigin.X + gridSize, gridOrigin.Y + gridSize))
        };
    }

    private static Vector2I[] GetHVDTiles(Vector2I p, int offset)
    {
        return new Vector2I[8]{
            new(p.X - offset, p.Y - offset), new(p.X, p.Y - offset), new(p.X + offset, p.Y - offset), 
            new(p.X - offset, p.Y         ),                         new(p.X + offset, p.Y         ), 
            new(p.X - offset, p.Y + offset), new(p.X, p.Y + offset), new(p.X + offset, p.Y + offset)};
    }

    private static Vector2I[] GetHVTiles(Vector2I p, int offset)
    {
        return new Vector2I[4]{new(p.X, p.Y - offset), new(p.X - offset, p.Y), new(p.X + offset, p.Y), new(p.X, p.Y + offset)};
    }

    private Vector2 GetVoronoiPoint(Vector2I gridChunkOrigin)
    {
        return rng.GetRandVec2(gridChunkOrigin, gridSize / 2, gridSize / 2) + gridChunkOrigin + new Vector2(gridSize, gridSize) / 4;
    }

    private Vector2I GetGridChunkOrigin(Vector2 pos)
    {
        return new(Mathf.FloorToInt(pos.X / gridSize) * gridSize, Mathf.FloorToInt(pos.Y / gridSize) * gridSize);
    }
}