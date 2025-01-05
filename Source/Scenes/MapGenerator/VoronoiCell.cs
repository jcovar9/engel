// using System;
using System.Collections.Generic;
// using System.Linq;
using Godot;

// public struct BasicVertexInfo
// {
//     public Vector2I pos;
//     public Vector2I vOrigin1;
//     public Vector2I vOrigin2;
//     public Vector2I vOrigin3;
//     public float angle;
//     public BasicVertexInfo(Vector2I pos, Vector2I vOrigin1, Vector2I vOrigin2, Vector2I vOrigin3, float angle)
//     {
//         this.pos = pos;
//         this.vOrigin1 = vOrigin1;
//         this.vOrigin2 = vOrigin2;
//         this.vOrigin3 = vOrigin3;
//         this.angle = angle;
//     }
// }

public class VoronoiCell
{
    private Options options;
    public Vector2I cellOrigin;
    // public Vector2I[] vOrigins3x3;
    // public List<Tuple<BasicVertexInfo, BasicVertexInfo>> edges;
    public HashSet<Vector2I> borderTiles;
    public VoronoiCell(Options options, Vector2I cellOrigin)
    {
        this.options = options;
        this.cellOrigin = cellOrigin;
        SetBorderTiles();
        // int offset = options.VCellSize;
        // vOrigins3x3 = new Vector2I[9]{
        //     GetVoronoiOrigin(new(cellOrigin.X - offset, cellOrigin.Y - offset)),
        //     GetVoronoiOrigin(new(cellOrigin.X         , cellOrigin.Y - offset)),
        //     GetVoronoiOrigin(new(cellOrigin.X + offset, cellOrigin.Y - offset)),

        //     GetVoronoiOrigin(new(cellOrigin.X - offset, cellOrigin.Y)),
        //     GetVoronoiOrigin(cellOrigin),
        //     GetVoronoiOrigin(new(cellOrigin.X + offset, cellOrigin.Y)),

        //     GetVoronoiOrigin(new(cellOrigin.X - offset, cellOrigin.Y + offset)),
        //     GetVoronoiOrigin(new(cellOrigin.X         , cellOrigin.Y + offset)),
        //     GetVoronoiOrigin(new(cellOrigin.X + offset, cellOrigin.Y + offset))
        // };
        // SetEdges();
    }

    private void SetBorderTiles()
    {
        borderTiles = new();
        for (int x = 0; x < options.VCellSize; x++)
        {
            borderTiles.Add(new(x + cellOrigin.X, cellOrigin.Y));
            borderTiles.Add(new(x + cellOrigin.X, cellOrigin.Y + options.VCellSize - 1));
        }
        for (int y = 1; y < options.VCellSize - 1; y++)
        {
            borderTiles.Add(new(cellOrigin.X, y + cellOrigin.Y));
            borderTiles.Add(new(cellOrigin.X + options.VCellSize - 1, y + cellOrigin.Y));
        }
    }

    // private void SetBorderTiles()
    // {
    //     borderTiles = new();
    //     foreach (Tuple<BasicVertexInfo, BasicVertexInfo> edge in edges)
    //     {
    //         Vector2I otherVOrigin = GetOtherVOrigin(edge.Item1, edge.Item2);
    //         if (EdgeIsMine(otherVOrigin))
    //         {
    //             GetBorder(edge.Item1.pos, edge.Item2.pos, Vector2I.Zero);
    //         }
    //         else
    //         {
    //             Vector2I delta = vOrigins3x3[4] - otherVOrigin;
    //             int xAmount = Mathf.Abs(delta.X);
    //             int yAmount = Mathf.Abs(delta.Y);
    //             Vector2I offset;
    //             if (xAmount < yAmount)
    //             {
    //                 offset = new(0, delta.Y / yAmount);
    //             }
    //             else
    //             {
    //                 offset = new(delta.X / xAmount, 0);
    //             }
    //             GetBorder(edge.Item1.pos, edge.Item2.pos, offset);
    //         }
    //     }
    // }

    // private bool EdgeIsMine(Vector2I otherVOrigin)
    // {
    //     Vector2I otherCellOrigin = new Vector2I(Mathf.FloorToInt((float)otherVOrigin.X / options.VCellSize), Mathf.FloorToInt((float)otherVOrigin.Y / options.VCellSize)) * options.VCellSize;
    //     if (cellOrigin.X < otherCellOrigin.X)
    //     {
    //         return true;
    //     }
    //     else if (cellOrigin.X == otherCellOrigin.X && cellOrigin.Y <= otherCellOrigin.Y)
    //     {
    //         return true;
    //     }
    //     else
    //     {
    //         return false;
    //     }
    // }

    // private Vector2I GetOtherVOrigin(BasicVertexInfo v1, BasicVertexInfo v2)
    // {
    //     Span<Vector2I> v1VOrigins = stackalloc Vector2I[3] { v1.vOrigin1, v1.vOrigin2, v1.vOrigin3 };
    //     Span<Vector2I> v2VOrigins = stackalloc Vector2I[3] { v2.vOrigin1, v2.vOrigin2, v2.vOrigin3 };
    //     foreach (Vector2I vOrigin in v1VOrigins)
    //     {
    //         if (vOrigin != vOrigins3x3[4] && v2VOrigins.Contains(vOrigin))
    //         {
    //             return vOrigin;
    //         }
    //     }
    //     GD.Print("Failed to find other Voronoi Origin for edge: ", v1.pos, " -> ", v2.pos);
    //     return Vector2I.Zero;
    // }

    // private void GetBorder(Vector2I vec0, Vector2I vec1, Vector2I edgeOffset)
    // {
    //     if (Mathf.Abs(vec1.Y - vec0.Y) < Mathf.Abs(vec1.X - vec0.X))
    //     {
    //         GetLineH(borderTiles, vec0.X, vec0.Y, vec1.X, vec1.Y, edgeOffset);
    //     }
    //     else
    //     {
    //         GetLineV(borderTiles, vec0.X, vec0.Y, vec1.X, vec1.Y, edgeOffset);
    //     }
    // }
    // private static void GetLineV(HashSet<Vector2I> tiles, int x0, int y0, int x1, int y1, Vector2I offset)
    // {
    //     if (y1 < y0)
    //     {
    //         int tmp = x0;
    //         x0 = x1;
    //         x1 = tmp;
    //         tmp = y0;
    //         y0 = y1;
    //         y1 = tmp;
    //     }
    //     int dx = x1 - x0;
    //     int dy = y1 - y0;
    //     int dir = dx < 0 ? -1 : 1;
    //     dx *= dir;
    //     if (dy != 0)
    //     {
    //         int x = x0;
    //         int p = 2 * dx - dy;
    //         for (int i = 0; i < dy + 1; i++)
    //         {
    //             tiles.Add(new Vector2I(x, y0 + i) + offset);
    //             if (0 <= p)
    //             {
    //                 x += dir;
    //                 p -= 2 * dy;
    //             }
    //             p += 2 * dx;
    //         }
    //     }
    // }
    // private static void GetLineH(HashSet<Vector2I> tiles, int x0, int y0, int x1, int y1, Vector2I offset)
    // {
    //     if (x1 < x0)
    //     {
    //         int tmp = x0;
    //         x0 = x1;
    //         x1 = tmp;
    //         tmp = y0;
    //         y0 = y1;
    //         y1 = tmp;
    //     }
    //     int dx = x1 - x0;
    //     int dy = y1 - y0;
    //     int dir = dy < 0 ? -1 : 1;
    //     dy *= dir;
    //     if (dx != 0)
    //     {
    //         int y = y0;
    //         int p = 2 * dy - dx;
    //         for (int i = 0; i < dx + 1; i++)
    //         {
    //             tiles.Add(new Vector2I(x0 + i, y) + offset);
    //             if (0 <= p)
    //             {
    //                 y += dir;
    //                 p -= 2 * dx;
    //             }
    //             p += 2 * dy;
    //         }
    //     }
    // }

    // private Vector2I GetVoronoiOrigin(Vector2I cellOrigin)
    // {
    //     Vector2I offset = new(cellOrigin.X + options.VCellSize / 4, cellOrigin.Y + options.VCellSize / 4);
    //     return options.rng.GetRandVec2I(cellOrigin, options.VCellSize / 2, options.VCellSize / 2) + offset;
    // }

    // private void SetEdges()
    // {
    //     edges = new();
    //     List<BasicVertexInfo> vertexes = GetVertexes();
    //     int firstIndex = vertexes.Count - 1;
    //     for (int i = -1; i < vertexes.Count - 1; i++)
    //     {
    //         BasicVertexInfo v1 = vertexes[firstIndex];
    //         BasicVertexInfo v2 = vertexes[i + 1];
    //         edges.Add(GetEdgeID(v1, v2));
    //         firstIndex = i + 1;
    //     }
    // }

    // private List<BasicVertexInfo> GetVertexes()
    // {
    //     List<BasicVertexInfo> unorderedVertexes = new();
    //     Span<Vector2I> vecs = stackalloc Vector2I[4];
    //     vecs[0] = vOrigins3x3[4];
    //     for (int y = 0; y <= 2; y += 2)
    //     {
    //         for (int x = 0; x <= 2; x += 2)
    //         {
    //             int cornerIndex = x + y * 3;
    //             vecs[1] = vOrigins3x3[cornerIndex];
    //             vecs[2] = vOrigins3x3[cornerIndex - (x - 1)];
    //             vecs[3] = vOrigins3x3[cornerIndex - (y - 1) * 3];
    //             TryAddChunkVertex(unorderedVertexes, vecs[0], vecs[1], vecs[2], vecs[3]);
    //             TryAddChunkVertex(unorderedVertexes, vecs[0], vecs[1], vecs[3], vecs[2]);
    //             TryAddChunkVertex(unorderedVertexes, vecs[0], vecs[2], vecs[3], vecs[1]);
    //         }
    //     }
    //     return unorderedVertexes.OrderBy(vertex => vertex.angle).ToList();
    // }

    // private static void TryAddChunkVertex(List<BasicVertexInfo> unorderedVertexes, Vector2I P1, Vector2I P2, Vector2I P3, Vector2I checkP)
    // {
    //     Vector2 vertexPos = GetVertexPosition(P1, P2, P3);
    //     float centerDist = vertexPos.DistanceTo(P1);
    //     float checkDist = vertexPos.DistanceTo(checkP);
    //     if (centerDist < checkDist)
    //     {
    //         float angle = ((Vector2)P1).AngleToPoint(vertexPos);
    //         Vector2I posInt = new(Mathf.FloorToInt(vertexPos.X), Mathf.FloorToInt(vertexPos.Y));
    //         unorderedVertexes.Add(new(posInt, P1, P2, P3, angle));
    //     }
    // }

    // private static Vector2 GetVertexPosition(Vector2I voronoiA, Vector2I voronoiB, Vector2I voronoiC)
    // {
    //     int voronoiASq = voronoiA.X * voronoiA.X + voronoiA.Y * voronoiA.Y;
    //     int voronoiBSq = voronoiB.X * voronoiB.X + voronoiB.Y * voronoiB.Y;
    //     int voronoiCSq = voronoiC.X * voronoiC.X + voronoiC.Y * voronoiC.Y;
    //     int BYminusCY = voronoiB.Y - voronoiC.Y;
    //     int CYminusAY = voronoiC.Y - voronoiA.Y;
    //     int AYminusBY = voronoiA.Y - voronoiB.Y;
    //     float denominator = 2 * (voronoiA.X * BYminusCY + voronoiB.X * CYminusAY + voronoiC.X * AYminusBY);
    //     float x = (voronoiASq * BYminusCY + voronoiBSq * CYminusAY + voronoiCSq * AYminusBY) / denominator;
    //     float y = (voronoiASq * (voronoiC.X - voronoiB.X) + voronoiBSq * (voronoiA.X - voronoiC.X) + voronoiCSq * (voronoiB.X - voronoiA.X)) / denominator;
    //     return new(x, y);
    // }

    // private static Tuple<BasicVertexInfo, BasicVertexInfo> GetEdgeID(BasicVertexInfo v1, BasicVertexInfo v2)
    // {
    //     if (v1.pos.X < v2.pos.X)
    //     {
    //         return new(v1, v2);
    //     }
    //     else if (v1.pos.X == v2.pos.X && v1.pos.Y <= v2.pos.Y)
    //     {
    //         return new(v1, v2);
    //     }
    //     else
    //     {
    //         return new(v2, v1);
    //     }
    // }

}