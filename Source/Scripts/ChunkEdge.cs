using System;
using System.Collections.Generic;
using Godot;

public class ChunkEdge
{
    public ChunkVertex vertex1;
    public ChunkVertex vertex2;
    public Vector2I[] voronoiOrigins = new Vector2I[2];
    public Dictionary<Vector2I, float> edgeTiles;
    public ChunkEdge(ChunkVertex vertex1, ChunkVertex vertex2)
    {
        this.vertex1 = vertex1;
        this.vertex2 = vertex2;
        SetVoronoiOrigins();
        SetEdgeTiles();
    }

    private void SetVoronoiOrigins()
    {
        int counter = 0;
        foreach (Vector2I voronoiP in vertex1.voronoiOrigins)
        {
            if(vertex2.voronoiOrigins.Contains(voronoiP))
            {
                voronoiOrigins[counter] = voronoiP;
                counter++;
            }
        }
    }

    private void SetEdgeTiles()
    {
        edgeTiles = new();
        Vector2I start = vertex1.position;
        Vector2I end = vertex2.position;
        Vector2 dir = ((Vector2)(end - start)).Normalized();
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
        float currLength = (currEdgeTile - start).Length();
        float maxLength = (end - start).Length();
        while(currLength <= maxLength)
        {
            float currHeight = Mathf.Lerp(vertex1.height, vertex2.height, Mathf.InverseLerp(0.0f, maxLength, currLength));
            edgeTiles[currEdgeTile] = currHeight;
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
            currLength = (currEdgeTile - start).Length();
        }
    }

    public Dictionary<Vector2I, float> GetBorderTiles(Vector2I voronoiOrigin)
    {
        Vector2I otherVoronoiOrigin;
        if(voronoiOrigins[0] == voronoiOrigin) otherVoronoiOrigin = voronoiOrigins[1];
        else otherVoronoiOrigin = voronoiOrigins[0];
        Span<Vector2I> offsets = stackalloc Vector2I[]{ new(0,-1), new(-1,0), new(1,0), new(0,1) };

        Dictionary<Vector2I, float> borderTiles = new();
        foreach (KeyValuePair<Vector2I, float> edgeTile in edgeTiles)
        {
            float dist = edgeTile.Key.DistanceTo(voronoiOrigin);
            float otherDist = edgeTile.Key.DistanceTo(otherVoronoiOrigin);
            if(dist < otherDist)
            {
                borderTiles[edgeTile.Key] = edgeTile.Value;
            }
            else
            {
                foreach (Vector2I offset in offsets)
                {
                    Vector2I offsetEdgeTile = edgeTile.Key + offset;
                    if(!edgeTiles.ContainsKey(offsetEdgeTile) && !borderTiles.ContainsKey(offsetEdgeTile))
                    {
                        float offsetDist = offsetEdgeTile.DistanceTo(voronoiOrigin);
                        if(offsetDist < dist)
                        {
                            borderTiles[offsetEdgeTile] = edgeTile.Value;
                        }
                    }
                }
                // TryAddOffsetTiles(borderTiles, edgeTile, voronoiOrigin, otherVoronoiOrigin);
            }
        }
        return borderTiles;
    }

    // private void TryAddOffsetTiles(Dictionary<Vector2I, float> borderTiles, KeyValuePair<Vector2I, float> edgeTile, Vector2I voronoiOrigin, Vector2I otherVoronoiOrigin)
    // {
    //     Span<Vector2I> HVTiles = stackalloc Vector2I[4]{
    //         new(edgeTile.Key.X    , edgeTile.Key.Y - 1),
    //         new(edgeTile.Key.X - 1, edgeTile.Key.Y    ),
    //         new(edgeTile.Key.X + 1, edgeTile.Key.Y    ),
    //         new(edgeTile.Key.X    , edgeTile.Key.Y + 1),
    //     };
    //     foreach (Vector2I offsetTile in HVTiles)
    //     {
    //         float offsetDist = offsetTile.DistanceTo(voronoiOrigin);
    //         float otherOffsetDist = offsetTile.DistanceTo(otherVoronoiOrigin);
    //         if(!borderTiles.ContainsKey(offsetTile) && offsetDist < otherOffsetDist)
    //         {
    //             borderTiles[offsetTile] = edgeTile.Value;
    //         }
    //     }
    // }

}