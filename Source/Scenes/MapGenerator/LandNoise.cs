
using System;
using System.Collections.Generic;
using Godot;
using Godot.NativeInterop;

public class LandNoise
{
    private enum VCellType
    {
        Ocean = 0,
        Land = 1,
    };
    private readonly RNG _rng;
    private readonly int minVCellSize;
    private readonly Dictionary<int, Tuple<Dictionary<Vector2I, Vector2I>, Dictionary<Vector2I, VCellType>>> layers = new();
    public LandNoise(RNG rng, int mapSize, int numLayers)
    {
        _rng = rng;
        minVCellSize = mapSize / (int)Mathf.Pow(2, numLayers);
        int currLayerCellSize = minVCellSize;
        while (currLayerCellSize <= mapSize)
        {
            layers.Add(currLayerCellSize, new(new(), new()));
            currLayerCellSize *= 2;
        }
        for (int x = -2; x <= 2; x++)
        {
            int bound = 2 - Mathf.Abs(x / 2);
            for (int y = -bound; y <= bound; y++)
            {
                Vector2I currOrigin = new(x * mapSize, y * mapSize);
                GetVPoint(currOrigin, mapSize);
                layers[mapSize].Item2.Add(currOrigin, VCellType.Ocean);
            }
        }
        layers[mapSize].Item2[Vector2I.Zero] = VCellType.Land;
    }

    public static Vector2I GetCellOrigin(Vector2 v, int cellSize)
    {
        return new Vector2I(Mathf.FloorToInt(v.X / cellSize), Mathf.FloorToInt(v.Y / cellSize)) * cellSize;
    }

    private (Vector2I, Vector2I, float) GetMinVCell(Vector2I point, int size)
    {
        Vector2I vCellOrigin = GetCellOrigin(point, size);
        (Vector2I, Vector2I, float) minVCell = (Vector2I.Zero, Vector2I.Zero, float.MaxValue);
        for (int x = -2; x <= 2; x++)
        {
            int bound = 2 - Mathf.Abs(x / 2);
            for (int y = -bound; y <= bound; y++)
            {
                Vector2I currOrigin = vCellOrigin + new Vector2I(x * size, y * size);
                Vector2I currVPoint = GetVPoint(currOrigin, size);
                float currDist = point.DistanceTo(currVPoint);
                if (currDist < minVCell.Item3)
                {
                    minVCell = (currOrigin, currVPoint, currDist);
                }
            }
        }
        return minVCell;
    }

    private Vector2I GetVPoint(Vector2I origin, int vCellSize)
    {
        Vector2I vPoint;
        if (!layers[vCellSize].Item1.TryGetValue(origin, out vPoint))
        {
            Vector2I rngPoint = origin + Vector2I.One * (vCellSize / 2);
            int spawnRange = (vCellSize / 2) * (vCellSize / 4 + 1);
            int index = _rng.GetRandRange(rngPoint, 0, spawnRange);
            index -= spawnRange / 2;
            int x;
            if (index < 0)
            {
                index += spawnRange / 2;
                x = Mathf.FloorToInt((-1 + Mathf.Sqrt(1 + 4 * index)) / 2) - vCellSize / 4;
            }
            else
            {
                index -= spawnRange / 2;
                index *= -1;
                x = -Mathf.FloorToInt((-1 + Mathf.Sqrt(1 + 4 * index)) / 2) - vCellSize / 4;
            }

            vPoint = _rng.GetRandVec2I(origin, vCellSize, vCellSize) + origin;
            layers[vCellSize].Item1.Add(origin, vPoint);
        }
        return vPoint;
    }

    private VCellType GetVCellType(Vector2I vCellOrigin, int vCellSize)
    {
        VCellType vCellType;
        if (!layers[vCellSize].Item2.TryGetValue(vCellOrigin, out vCellType))
        {
            Vector2I vPoint = GetVPoint(vCellOrigin, vCellSize);
            (Vector2I, Vector2I, float) minVCell = GetMinVCell(vPoint, vCellSize * 2);
            vCellType = GetVCellType(minVCell.Item1, vCellSize * 2);
            layers[vCellSize].Item2.Add(vCellOrigin, vCellType);
        }
        return vCellType;
    }

    public void PopulateLayers(Vector2I cellOrigin, int cellSize)
    {
        Vector2I topLeftVCell = GetCellOrigin(cellOrigin, minVCellSize);
        Vector2I bottomRightVCell = GetCellOrigin(cellOrigin + Vector2I.One * (cellSize - 1), minVCellSize);
        for (int x = topLeftVCell.X; x <= bottomRightVCell.X; x += minVCellSize)
        {
            for (int y = topLeftVCell.Y; y <= bottomRightVCell.Y; y += minVCellSize)
            {
                Vector2I currVCellOrigin = new(x, y);
                VCellType vCellType = GetVCellType(currVCellOrigin, minVCellSize);
            }
        }
    }

    private bool IsAllOceanOrLand(Vector2I vCellOrigin, int vCellSize)
    {
        bool allOcean = true;
        bool allLand = true;
        for (int x = -2; x <= 2; x++)
        {
            int bound = 2 - Mathf.Abs(x / 2);
            for (int y = -bound; y <= bound; y++)
            {
                Vector2I currOrigin = vCellOrigin + new Vector2I(x * vCellSize, y * vCellSize);
                VCellType vCellType;
                if (!layers[vCellSize].Item2.TryGetValue(currOrigin, out vCellType))
                {
                    return false;
                }
                allOcean = allOcean && vCellType == VCellType.Ocean;
                allLand = allLand && vCellType == VCellType.Land;
            }
        }
        return allOcean || allLand;
    }

    public void SetByteArray(byte[] a, Vector2I cellOrigin, int cellSize, Vector2I vCellOrigin, int vCellSize)
    {
        Vector2I index = vCellOrigin - cellOrigin;
        bool isAllOceanOrLand = IsAllOceanOrLand(vCellOrigin, vCellSize);
        VCellType vCellType = layers[vCellSize].Item2[vCellOrigin];
        for (int x = 0; x < vCellSize; x++)
        {
            for (int y = 0; y < vCellSize; y++)
            {
                int i = (index.X + x + (index.Y + y) * cellSize) * 3;
                if (isAllOceanOrLand)
                {
                    if (vCellType == VCellType.Ocean)
                    {
                        a[i + 2] = 125;
                    }
                    else
                    {
                        a[i + 1] = 125;
                    }
                }
                else
                {
                    (Vector2I, Vector2I, float) minVCell = GetMinVCell(vCellOrigin + new Vector2I(x, y), vCellSize);
                    VCellType currVCellType = GetVCellType(minVCell.Item1, vCellSize);
                    if (currVCellType == VCellType.Ocean)
                    {
                        a[i + 2] = 125;
                    }
                    else
                    {
                        a[i + 1] = 125;
                    }
                }
            }
        }

    }

}