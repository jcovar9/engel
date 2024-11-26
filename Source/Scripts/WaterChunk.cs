using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class WaterChunk
{
    public Vector2I origin;
    public int size;
    private RNG rng;
    public Vector2I voronoiPoint;
    public int[] heightData;
    public List<Tuple<Vector2, float>> lakeSeeds;

    public WaterChunk(Vector2I _origin, int _size, RNG _rng){
        origin = _origin;
        size = _size;
        rng = _rng;
        voronoiPoint = GetVoronoiPoint(origin);
        heightData = new int[_size * _size];
        lakeSeeds = GetValidLakeSeeds().OrderBy(x => x.Item2).ToList();
        foreach (Tuple<Vector2, float> lakeSeed in lakeSeeds)
        {
            Vector2I lakeSeedOrigin = GetChunkOrigin(lakeSeed.Item1);
            if(origin == lakeSeedOrigin)
            {
                Vector2I localLakeSeed = new Vector2I(Mathf.FloorToInt(lakeSeed.Item1.X), Mathf.FloorToInt(lakeSeed.Item1.Y)) - lakeSeedOrigin;
                heightData[localLakeSeed.X + localLakeSeed.Y * size] = 4;
            }
        }
    }

    private List<Tuple<Vector2, float>> GetValidLakeSeeds()
    {
        List<Vector2I> voronoiNeighbors = GetVoronoiNeighbors(origin);
        List<Tuple<Vector2, float>> validLakeSeeds = new();
        for(int i = 0; i < voronoiNeighbors.Count; i++)
        {
            for (int j = i + 1; j < voronoiNeighbors.Count; j++)
            {
                Vector2 circumcenter = GetCircumcenter(voronoiPoint, voronoiNeighbors[i], voronoiNeighbors[j]);
                Vector2I circumcenterChunk = GetChunkOrigin(circumcenter);
                if(origin.X - size <= circumcenterChunk.X && circumcenterChunk.X <= origin.X + size &&
                origin.Y - size <= circumcenterChunk.Y && circumcenterChunk.Y <= origin.Y + size)
                {
                    float minDist1 = circumcenter.DistanceTo(voronoiPoint);
                    float minDist2 = float.MaxValue;
                    float minDist3 = float.MaxValue;
                    foreach (Vector2I neighborVoronoiPoint in voronoiNeighbors)
                    {
                        float currDist = circumcenter.DistanceTo(neighborVoronoiPoint);
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
                    if(minDist3 - minDist1 < 0.5f)
                    {
                        validLakeSeeds.Add(new(circumcenter, ((Vector2)voronoiPoint).AngleToPoint(circumcenter)));
                    }
                }
            }
        }
        return validLakeSeeds;
    }

    public Sprite2D GetChunkSprite(Shader shader)
    {
        Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        ImageTexture imageTex = ImageTexture.CreateFromImage(image);
        ShaderMaterial shaderMaterial = new()
        {
            Shader = shader
        };
        shaderMaterial.SetShaderParameter("chunkHeightData", heightData);
        shaderMaterial.SetShaderParameter("chunkSize", size);
        shaderMaterial.SetShaderParameter("voronoiPoint", voronoiPoint - origin);
        Sprite2D sprite2D = new()
        {
            Position = origin + new Vector2I(size / 2, size / 2),
            Texture = imageTex,
            Material = shaderMaterial
        };
        return sprite2D;
    }

    private static Vector2 GetCircumcenter(Vector2I voronoiA, Vector2I voronoiB, Vector2I voronoiC)
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

    private List<Vector2I> GetVoronoiNeighbors(Vector2I chunkOrigin)
    {
        List<Vector2I> origins = GetChunkNeighbors(chunkOrigin);
        for(int x = 0; x < 8; x++)
        {
            origins[x] = GetVoronoiPoint(origins[x]);
        }
        return origins;
    }

    private List<Vector2I> GetChunkNeighbors(Vector2I chunkOrigin)
    {
        return new(){
            new Vector2I(-1, -1) * size + chunkOrigin,
            new Vector2I( 0, -1) * size + chunkOrigin,
            new Vector2I( 1, -1) * size + chunkOrigin,

            new Vector2I(-1,  0) * size + chunkOrigin,
            new Vector2I( 1,  0) * size + chunkOrigin,

            new Vector2I(-1,  1) * size + chunkOrigin,
            new Vector2I( 0,  1) * size + chunkOrigin,
            new Vector2I( 1,  1) * size + chunkOrigin,
        };
    }

    private Vector2I GetVoronoiPoint(Vector2I chunkOrigin)
    {
        return rng.GetRandVec2I(chunkOrigin, size / 2, size / 2) + chunkOrigin + new Vector2I(size, size) / 4;
    }

    private Vector2I GetChunkOrigin(Vector2 pos)
    {
        return new(Mathf.FloorToInt(pos.X / size) * size, Mathf.FloorToInt(pos.Y / size) * size);
    }
}