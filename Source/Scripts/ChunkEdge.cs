using System;
using System.Collections.Generic;
using Godot;

public class ChunkEdge
{
    public ChunkVertex vertex1;
    public ChunkVertex vertex2;
    public Vector2I[] voronoiOrigins = new Vector2I[2];
    public Dictionary<Vector2I, float> edgeTiles;
    public ChunkEdge(ChunkVertex _vertex1, ChunkVertex _vertex2)
    {
        SetVertexes(_vertex1, _vertex2);
        SetVoronoiOrigins();
        SetEdgeTiles();
    }

    public static Tuple<Vector2I, Vector2I> GetEdgeID(ChunkVertex v1, ChunkVertex v2)
    {
        if(v1.position.X < v2.position.X)
        {
            return new(v1.position, v2.position);
        }
        else if(v1.position.X == v2.position.X)
        {
            if(v1.position.Y <= v2.position.Y)
            {
                return new(v1.position, v2.position);
            }
            else
            {
                return new(v2.position, v1.position);
            }
        }
        else
        {
            return new(v2.position, v1.position);
        }
    }

    private void SetVertexes(ChunkVertex v1, ChunkVertex v2)
    {
        if(v1.position == GetEdgeID(v1, v2).Item1)
        {
            vertex1 = v1;
            vertex2 = v2;
        }
        else
        {
            vertex1 = v2;
            vertex2 = v1;
        }
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
                TryAddOffsetTiles(borderTiles, edgeTile, voronoiOrigin, otherVoronoiOrigin);
            }
        }
        return borderTiles;
    }

    private void TryAddOffsetTiles(Dictionary<Vector2I, float> borderTiles, KeyValuePair<Vector2I, float> edgeTile, Vector2I voronoiOrigin, Vector2I otherVoronoiOrigin)
    {
        Span<Vector2I> HVTiles = stackalloc Vector2I[4]{
            new(edgeTile.Key.X    , edgeTile.Key.Y - 1),
            new(edgeTile.Key.X - 1, edgeTile.Key.Y    ),
            new(edgeTile.Key.X + 1, edgeTile.Key.Y    ),
            new(edgeTile.Key.X    , edgeTile.Key.Y + 1),
        };
        foreach (Vector2I offsetTile in HVTiles)
        {
            float offsetDist = offsetTile.DistanceTo(voronoiOrigin);
            float otherOffsetDist = offsetTile.DistanceTo(otherVoronoiOrigin);
            if(!borderTiles.ContainsKey(offsetTile) && offsetDist < otherOffsetDist)
            {
                borderTiles[offsetTile] = edgeTile.Value;
            }
        }
    }

}