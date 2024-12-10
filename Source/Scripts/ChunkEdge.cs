using System;
using System.Collections.Generic;
using Godot;

public class ChunkEdge
{
    public ChunkVertex vertex1;
    public ChunkVertex vertex2;
    public Vector2I[] voronoiOrigins = new Vector2I[2];
    public Dictionary<Vector2, float> edgeTiles;
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
        Vector2 start = GetTileCenter(vertex1.position);
        Vector2 end = GetTileCenter(vertex2.position);
        Vector2 dir = (end - start).Normalized();
        Vector2 unitStepSize = new(Mathf.Sqrt(1f + (dir.Y / dir.X) * (dir.Y / dir.X)), Mathf.Sqrt(1f + (dir.X / dir.Y) * (dir.X / dir.Y)));
        Vector2 currTileCenter = new(start.X, start.Y);
        Vector2 lengthIn1D = unitStepSize * 0.5f;
        Vector2 step;
        if(dir.X < 0f) { step.X = -1f; }
        else { step.X = 1f; }

        if(dir.Y < 0f) { step.Y = -1f; }
        else { step.Y = 1f; }

        float currLength = 0f;
        float maxLength = (end - start).Length();
        while(currLength <= maxLength)
        {
            float currHeight = Mathf.Lerp(vertex1.height, vertex2.height, Mathf.InverseLerp(0f, maxLength, currLength));
            edgeTiles[currTileCenter] = currHeight;
            if(lengthIn1D.X < lengthIn1D.Y)
            {
                currTileCenter.X += step.X;
                lengthIn1D.X += unitStepSize.X;
            }
            else
            {
                currTileCenter.Y += step.Y;
                lengthIn1D.Y += unitStepSize.Y;
            }
            currLength = (currTileCenter - start).Length();
        }
    }

    public Dictionary<Vector2I, float> GetBorderTiles(Vector2I voronoiOrigin)
    {
        Vector2I otherVoronoiOrigin;
        if(voronoiOrigins[0] == voronoiOrigin) otherVoronoiOrigin = voronoiOrigins[1];
        else otherVoronoiOrigin = voronoiOrigins[0];
        Vector2I offsetDir = GetEdgeOffsetDir(otherVoronoiOrigin, voronoiOrigin);

        Dictionary<Vector2I, float> borderTiles = new();
        foreach (KeyValuePair<Vector2, float> edgeTile in edgeTiles)
        {
            Vector2I edgeTilePos = new(Mathf.FloorToInt(edgeTile.Key.X), Mathf.FloorToInt(edgeTile.Key.Y));
            float dist = edgeTile.Key.DistanceTo(voronoiOrigin);
            float otherDist = edgeTile.Key.DistanceTo(otherVoronoiOrigin);
            if(dist < otherDist || (dist == otherDist && VpointHasPriority(voronoiOrigin, otherVoronoiOrigin)) )
            {
                borderTiles[edgeTilePos] = edgeTile.Value;
            }
            else
            {
                Vector2I offsetEdgeTile = edgeTilePos + offsetDir;
                if(!edgeTiles.ContainsKey(offsetEdgeTile))
                {
                    borderTiles[offsetEdgeTile] = edgeTile.Value;
                }
            }
        }
        return borderTiles;
    }

    private static bool VpointHasPriority(Vector2I vPoint, Vector2I otherVPoint)
    {
        if(otherVPoint.X < vPoint.X)
        {
            return true;
        }
        else if(otherVPoint.X == vPoint.X && otherVPoint.Y < vPoint.Y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private static Vector2I GetEdgeOffsetDir(Vector2I otherVOrigin, Vector2I VOrigin)
    {
        Vector2 orthogonalAngle = (GetTileCenter(VOrigin) - GetTileCenter(otherVOrigin)).Normalized();
        float xVal = Mathf.Abs(orthogonalAngle.X);
        float yVal = Mathf.Abs(orthogonalAngle.Y);
        if(xVal < yVal)
        {
            return new(0, (int)(orthogonalAngle.Y / yVal));
        }
        else
        {
            return new((int)(orthogonalAngle.X / xVal), 0);
        }
    }

    private static Vector2 GetTileCenter(Vector2I pos)
    {
        return new(pos.X + 0.5f, pos.Y + 0.5f);
    }

    // public Dictionary<Vector2I, float> GetBorderTiles(Vector2I voronoiOrigin)
    // {
    //     Vector2I otherVoronoiOrigin;
    //     if(voronoiOrigins[0] == voronoiOrigin) otherVoronoiOrigin = voronoiOrigins[1];
    //     else otherVoronoiOrigin = voronoiOrigins[0];
    //     Span<Vector2I> offsets = stackalloc Vector2I[]{ new(0,-1), new(-1,0), new(1,0), new(0,1) };

    //     Dictionary<Vector2I, float> borderTiles = new();
    //     foreach (KeyValuePair<Vector2I, float> edgeTile in edgeTiles)
    //     {
    //         float dist = edgeTile.Key.DistanceTo(voronoiOrigin);
    //         float otherDist = edgeTile.Key.DistanceTo(otherVoronoiOrigin);
    //         if(dist < otherDist)
    //         {
    //             borderTiles[edgeTile.Key] = edgeTile.Value;
    //         }
    //         else
    //         {
    //             foreach (Vector2I offset in offsets)
    //             {
    //                 Vector2I offsetEdgeTile = edgeTile.Key + offset;
    //                 if(!edgeTiles.ContainsKey(offsetEdgeTile) && !borderTiles.ContainsKey(offsetEdgeTile))
    //                 {
    //                     float offsetDist = offsetEdgeTile.DistanceTo(voronoiOrigin);
    //                     if(offsetDist < dist)
    //                     {
    //                         borderTiles[offsetEdgeTile] = edgeTile.Value;
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //     return borderTiles;
    // }

}