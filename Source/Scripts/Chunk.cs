using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public struct BasicVertexInfo
{
    public Vector2I pos;
    public Vector2I vPoint1;
    public Vector2I vPoint2;
    public Vector2I vPoint3;
    public float angle;
}

public class Chunk
{
    private readonly Options options;
    private Vector2I origin;
    public Vector2I[] voronoiOrigins3x3;
    public List<Tuple<BasicVertexInfo, BasicVertexInfo>> edges;

    public Chunk( Options options, Vector2I origin)
    {
        this.options = options;
        this.origin = origin;
        SetVoronoiOrigins3x3();
        SetEdges();
    }

    private void SetVoronoiOrigins3x3()
    {
        this.voronoiOrigins3x3 = new Vector2I[9]{
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
    
    private Vector2I GetVoronoiPoint(Vector2I origin)
    {
        Vector2I offset = new(origin.X + options.ChunkSize / 4, origin.Y + options.ChunkSize / 4);
        return options.rng.GetRandVec2I(origin, options.ChunkSize / 2, options.ChunkSize / 2) + offset;
    }

    private void SetEdges()
    {
        edges = new();
        List<BasicVertexInfo> vertexes = GetVertexes();
        int firstIndex = vertexes.Count - 1;
        for(int i = -1; i < vertexes.Count - 1; i++)
        {
            BasicVertexInfo v1 = vertexes[firstIndex];
            BasicVertexInfo v2 = vertexes[i + 1];
            edges.Add(GetEdgeID(v1, v2));
            firstIndex = i + 1;
        }
    }

    private List<BasicVertexInfo> GetVertexes()
    {
        List<BasicVertexInfo> unorderedVertexes = new();
        Span<Vector2I> vecs = stackalloc Vector2I[4];
        vecs[0] = voronoiOrigins3x3[4];
        for(int y = 0; y <= 2; y += 2)
        {
            for(int x = 0; x <= 2; x += 2)
            {
                int cornerIndex = x + y * 3;
                vecs[1] = voronoiOrigins3x3[cornerIndex];
                vecs[2] = voronoiOrigins3x3[cornerIndex - (x - 1)];
                vecs[3] = voronoiOrigins3x3[cornerIndex - (y - 1) * 3];
                TryAddChunkVertex(unorderedVertexes, vecs[0], vecs[1], vecs[2], vecs[3]);
                TryAddChunkVertex(unorderedVertexes, vecs[0], vecs[1], vecs[3], vecs[2]);
                TryAddChunkVertex(unorderedVertexes, vecs[0], vecs[2], vecs[3], vecs[1]);
            }
        }
        return unorderedVertexes.OrderBy(vertex => vertex.angle).ToList();
    }
    
    private static void TryAddChunkVertex(List<BasicVertexInfo> unorderedVertexes, Vector2I P1, Vector2I P2, Vector2I P3, Vector2I checkP)
    {
        Vector2I vertexPos = GetVertexPosition(P1, P2, P3);
        float centerDist = vertexPos.DistanceTo(P1);
        float checkDist = vertexPos.DistanceTo(checkP);
        if(centerDist < checkDist)
        {
            float angle = ((Vector2)P1).AngleToPoint((Vector2)vertexPos);
            unorderedVertexes.Add(new(){pos = vertexPos, vPoint1 = P1, vPoint2 = P2, vPoint3 = P3, angle = angle});
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

    private static Tuple<BasicVertexInfo, BasicVertexInfo> GetEdgeID(BasicVertexInfo v1, BasicVertexInfo v2)
    {
        if(v1.pos.X < v2.pos.X)
        {
            return new(v1, v2);
        }
        else if(v1.pos.X == v2.pos.X && v1.pos.Y <= v2.pos.Y)
        {
            return new(v1, v2);
        }
        else
        {
            return new(v2, v1);
        }
    }

}