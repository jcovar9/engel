using System;
using System.Collections;
using System.Collections.Generic;
using Godot;


public class Cell
{
    // private struct CellWaterInfo
    // {
    //     public int cellSize;
    //     public Vector2I cellOrigin;
    //     public Vector2I riverPoint;
    //     public float riverPointHeight;
    //     public Vector2I exitCellOrigin;
    //     public Vector2I exitCellRiverPoint;
    //     public float exitCellRiverPointHeight;
    //     public CellWaterInfo(Options options, Vector2I cellOrigin)
    //     {
    //         this.cellSize = options.CellSize;
    //         this.cellOrigin = cellOrigin;
    //         this.riverPoint = GetRiverPoint(options, cellOrigin);
    //         this.riverPointHeight = GetHeight(options, riverPoint);
    //         Span<Vector2I> otherCellOrigins = stackalloc Vector2I[]
    //         {
    //             new Vector2I(cellOrigin.X, cellOrigin.Y - cellSize),
    //             new Vector2I(cellOrigin.X - cellSize, cellOrigin.Y),
    //             new Vector2I(cellOrigin.X + cellSize, cellOrigin.Y),
    //             new Vector2I(cellOrigin.X, cellOrigin.Y + cellSize),
    //         };
    //         this.exitCellOrigin = cellOrigin;
    //         this.exitCellRiverPoint = riverPoint;
    //         this.exitCellRiverPointHeight = riverPointHeight;
    //         foreach (Vector2I otherCellOrigin in otherCellOrigins)
    //         {
    //             Vector2I otherCellRiverPoint = GetRiverPoint(options, otherCellOrigin);
    //             float otherCellRiverPointHeight = GetHeight(options, otherCellRiverPoint);
    //             if (otherCellRiverPointHeight < exitCellRiverPointHeight)
    //             {
    //                 this.exitCellOrigin = otherCellOrigin;
    //                 this.exitCellRiverPoint = otherCellRiverPoint;
    //                 this.exitCellRiverPointHeight = otherCellRiverPointHeight;
    //             }
    //         }
    //     }
    // }
    // private enum TileType
    // {
    //     Land = 0,
    //     River = 1,
    //     Lake = 2,
    //     Ocean = 3,
    // };
    public readonly Vector2I CellOrigin;
    private readonly int _cellSize;
    private readonly int _mapSize;
    private readonly int _numVLayers;
    private readonly LandNoise landNoise;

    public Cell(Vector2I cellOrigin, int cellSize, int mapSize, uint seed, int numVLayers)
    {
        CellOrigin = cellOrigin;
        _cellSize = cellSize;
        _mapSize = mapSize;
        _numVLayers = numVLayers;
        landNoise = new(new(seed), mapSize, numVLayers);
    }

    // private void RecursiveAddSubVCells(Vector2I origin, int size)
    // {
    //     Span<Vector2I> nearbyCellOrigins = stackalloc Vector2I[21];
    //     SetNearbyCellOrigins(nearbyCellOrigins, origin, size);
    //     int subSize = size / 2;
    //     Span<Vector2I> subCellOffsets = stackalloc Vector2I[]
    //     {
    //         Vector2I.Zero, new(subSize, 0), new(0, subSize), new(subSize, subSize),
    //     };
    //     foreach (Vector2I subCellOffset in subCellOffsets)
    //     {
    //         Vector2I currSubCellOrigin = subCellOffset + origin;
    //         Vector2I currSubVPoint = GetVPoint(currSubCellOrigin, subSize);
    //         Vector2I closestNearbyOrigin = Vector2I.Zero;
    //         Vector2I closestNearbyVPoint = Vector2I.Zero;
    //         float closestDist = float.MaxValue;
    //         foreach (Vector2I nearbyOrigin in nearbyCellOrigins)
    //         {
    //             Vector2I nearbyVPoint = _vCells[size][nearbyOrigin];
    //             float dist = currSubVPoint.DistanceTo(nearbyVPoint);
    //             if (dist < closestDist)
    //             {
    //                 closestDist = dist;
    //                 closestNearbyOrigin = nearbyOrigin;
    //                 closestNearbyVPoint = nearbyVPoint;
    //             }
    //         }

    //     }
    // }

    // private void SetVCells()
    // {
    //     Span<Vector2I> layer0CellOrigins = stackalloc Vector2I[21];
    //     SetNearbyCellOrigins(layer0CellOrigins, CellOrigin, _mapSize);
    //     foreach (Vector2I origin in layer0CellOrigins)
    //     {
    //         Vector2I vPoint = GetVPoint(origin, _mapSize);
    //         _vCells[_mapSize][origin] = (vPoint, CellType.Ocean);
    //     }
    //     _vCells[_mapSize][Vector2I.Zero] = (_vCells[_mapSize][Vector2I.Zero].Item1, CellType.Land);
    //     vCellSize = _mapSize / 2;
    // }

    // private void CreateLayer(Vector2I vCellSize)
    // {
    //     Vector2I topLeftVCellOrigin = GetCellOrigin(CellOrigin, vCellSize.X) - 3 * vCellSize;
    //     Vector2I bottomRightVCellOrigin = GetCellOrigin(CellOrigin + Vector2I.One * (_cellSize - 1), vCellSize.X) + 3 * vCellSize;
    //     int vPointArraySize = (bottomRightVCellOrigin.X / vCellSize.X) - (topLeftVCellOrigin.X / vCellSize.X);
    //     Span<(Vector2I, Vector2I)> layerVCells = stackalloc (Vector2I, Vector2I)[vPointArraySize * vPointArraySize];
    //     for (int x = 0; x <= vPointArraySize; x++)
    //     {
    //         for (int y = 0; y <= vPointArraySize; y++)
    //         {
    //             Vector2I currLayerOrigin = topLeftVCellOrigin + new Vector2I(x, y) * vCellSize.X;
    //             Vector2I currLayerVPoint = GetVPoint(currLayerOrigin, vCellSize.X);
    //             int i = x + y * vPointArraySize;
    //             layerVCells[i] = (currLayerOrigin, currLayerVPoint);
    //         }
    //     }

    //     Span<Vector2I> cellOrigins = stackalloc Vector2I[21];
    //     int upperLayerVCellSize = vCellSize.X * 2;
    //     for (int x = 2; x <= vPointArraySize - 2; x++)
    //     {
    //         for (int y = 2; y <= vPointArraySize - 2; y++)
    //         {
    //             int i = x + y * vPointArraySize;
    //             Vector2I layerOrigin = layerVCells[i].Item1;
    //             Vector2I layerVPoint = layerVCells[i].Item2;
    //             Vector2I upperLayerOrigin = GetCellOrigin(currLayerOrigin, upperLayerVCellSize);
    //             SetNearbyCellOrigins(cellOrigins, upperLayerOrigin, upperLayerVCellSize);
    //             Vector2I closestOrigin = Vector2I.Zero;
    //             Vector2I closestVPoint = Vector2I.Zero;
    //             float closestDist = float.MaxValue;
    //             for (int i = 0; i < 21; i++)
    //             {
    //                 Vector2I currUpperlayerOrigin = cellOrigins[i];
    //                 Vector2I currUpperLayerVPoint = _vCells[upperLayerVCellSize][currUpperlayerOrigin].Item1;
    //                 float dist = currLayerVPoint.DistanceTo(currUpperLayerVPoint);
    //                 if (dist < closestDist)
    //                 {
    //                     closestDist = dist;
    //                     closestOrigin = currUpperlayerOrigin;
    //                     closestVPoint = currUpperLayerVPoint;
    //                 }
    //             }

    //         }
    //     }
    // }

    // private void CreateLayers()
    // {
    //     Span<Vector2I> origins = stackalloc Vector2I[21];
    //     SetNearbyCellOrigins(origins, CellOrigin, _mapSize);
    //     _vCells.Add(_mapSize, new());
    //     foreach (Vector2I origin in origins)
    //     {
    //         Vector2I vPoint = GetVPoint(origin, _mapSize);
    //         CellType cellType = origin == Vector2I.Zero ? CellType.Land : CellType.Ocean;
    //         _vCells[_mapSize][origin] = (vPoint, cellType);
    //     }
    //     int minVCellSize = _mapSize / (int)Mathf.Pow(2, _vLayers);
    //     for (int vCellSize = _mapSize / 2; minVCellSize <= vCellSize; vCellSize /= 2)
    //     {
    //         _vCells.Add(vCellSize, new());
    //         CreateLayer(new(vCellSize, vCellSize));
    //     }

    // }

    // private Dictionary<Vector2I, int> GetLandVPointInfo()
    // {
    //     Vector2I currLayerVCellSize = new(_mapSize, _mapSize);
    //     Dictionary<Vector2I, int> prevLayerVPointInfo = new()
    //     {
    //         {GetVPoint(-currLayerVCellSize, _mapSize), 0},
    //         {GetVPoint(new(0, -currLayerVCellSize.Y), _mapSize), 0},
    //         {GetVPoint(new(currLayerVCellSize.X, -currLayerVCellSize.Y), _mapSize), 0},
    //         {GetVPoint(new(-currLayerVCellSize.X, 0), _mapSize), 0},
    //         {GetVPoint(Vector2I.Zero, _mapSize), 1},
    //         {GetVPoint(new(currLayerVCellSize.X, 0), _mapSize), 0},
    //         {GetVPoint(new(-currLayerVCellSize.X, currLayerVCellSize.Y), _mapSize), 0},
    //         {GetVPoint(new(0, currLayerVCellSize.Y), _mapSize), 0},
    //         {GetVPoint(currLayerVCellSize, _mapSize), 0},
    //     };
    //     Vector2I topLeftVCellOrigin = GetCellOrigin(CellOrigin, currLayerVCellSize.X) - 2 * currLayerVCellSize;
    //     Vector2I bottomRightVCellOrigin = GetCellOrigin(CellOrigin + Vector2I.One * _cellSize, currLayerVCellSize.X) + currLayerVCellSize;
    //     for (int x = topLeftVCellOrigin.X; x <= bottomRightVCellOrigin.X; x += currLayerVCellSize.X)
    //     {
    //         for (int y = topLeftVCellOrigin.Y; y <= bottomRightVCellOrigin.Y; y += currLayerVCellSize.Y)
    //         {
    //             Vector2I currVCellOrigin = new(x, y);
    //             Vector2I currVPoint = GetVPoint(currVCellOrigin, currLayerVCellSize.X);

    //         }
    //     }

    //     int allLand = 0;
    //     int allOcean = 0;
    //     for (int layer = 1; layer <= _vLayers; layer++)
    //     {
    //         allLand = 1;
    //         allOcean = 1;
    //         currLayerVCellSize /= 2;
    //         topLeftVCellOrigin = GetCellOrigin(CellOrigin, currLayerVCellSize.X) - 2 * currLayerVCellSize;
    //         bottomRightVCellOrigin = GetCellOrigin(CellOrigin + Vector2I.One * _cellSize, currLayerVCellSize.X) + currLayerVCellSize;
    //         Dictionary<Vector2I, int> currLayerVPointInfo = new();
    //         for (int x = topLeftVCellOrigin.X; x <= bottomRightVCellOrigin.X; x += currLayerVCellSize.X)
    //         {
    //             for (int y = topLeftVCellOrigin.Y; y <= bottomRightVCellOrigin.Y; y += currLayerVCellSize.Y)
    //             {
    //                 Vector2I currVCellOrigin = new(x, y);
    //                 Vector2I currVPoint = GetVPoint(currVCellOrigin, currLayerVCellSize.X);
    //                 Vector2I minPrevLayerVPoint = GetMinVPoint(currVPoint, currLayerVCellSize.X * 2);
    //                 if (!prevLayerVPointInfo.ContainsKey(minPrevLayerVPoint))
    //                 {
    //                     GD.Print("Error");
    //                 }
    //                 else
    //                 {
    //                     int currIsLand = prevLayerVPointInfo[minPrevLayerVPoint];
    //                     currLayerVPointInfo.Add(currVPoint, currIsLand);
    //                     allLand *= currIsLand;
    //                     allOcean *= 1 - currIsLand;
    //                 }
    //             }
    //         }
    //         prevLayerVPointInfo = currLayerVPointInfo;
    //     }
    //     _isAllLand = allLand;
    //     _isAllOcean = allOcean;
    //     return prevLayerVPointInfo;
    // }

    // private static Vector2I GetRiverPoint(Options options, Vector2I origin)
    // {
    //     return options.rng.GetRandVec2I(origin, options.CellSize / 2, options.CellSize / 2) + origin + new Vector2I(options.CellSize / 4, options.CellSize / 4);
    // }

    // private static float GetHeight(Options options, Vector2I v)
    // {
    //     float baseHeight = (options.GetLandNoise(v) + 1.0f) * 0.5f;
    //     float xDistanceMultiplier = 1f - Mathf.Pow(v.X * 2f / options.MapSize, 2f);
    //     float yDistanceMultiplier = 1f - Mathf.Pow(v.Y * 2f / options.MapSize, 2f);
    //     return baseHeight * xDistanceMultiplier * yDistanceMultiplier;
    // }

    // public void SetRivers()
    // {
    //     List<Tuple<Vector2I, float>> riverLineTiles = new();
    //     Span<Vector2I> otherCellOrigins = stackalloc Vector2I[]
    //     {
    //         new Vector2I(cellOrigin.X, cellOrigin.Y - cellSize),
    //         new Vector2I(cellOrigin.X - cellSize, cellOrigin.Y),
    //         new Vector2I(cellOrigin.X + cellSize, cellOrigin.Y),
    //         new Vector2I(cellOrigin.X, cellOrigin.Y + cellSize),
    //     };
    //     foreach (Vector2I otherCellOrigin in otherCellOrigins)
    //     {
    //         CellWaterInfo otherCellWaterInfo = new(options, otherCellOrigin);
    //         if (cellOrigin == otherCellWaterInfo.exitCellOrigin)
    //         {
    //             SetRiverLine(riverLineTiles, cellWaterInfo.riverPoint, cellWaterInfo.riverPointHeight, otherCellWaterInfo.riverPoint, otherCellWaterInfo.riverPointHeight);
    //             foreach (Tuple<Vector2I, float> riverTile in riverLineTiles)
    //             {
    //                 Vector2I indexVec = riverTile.Item1 - cellOrigin;
    //                 if (0 <= indexVec.X && indexVec.X < cellSize &&
    //                 0 <= indexVec.Y && indexVec.Y < cellSize)
    //                 {
    //                     tiles[indexVec.X, indexVec.Y] = new(riverTile.Item2, TileType.River);
    //                 }
    //             }
    //             riverLineTiles.Clear();
    //         }
    //     }
    //     if (cellOrigin != cellWaterInfo.exitCellOrigin)
    //     {
    //         SetRiverLine(riverLineTiles, cellWaterInfo.riverPoint, cellWaterInfo.riverPointHeight, cellWaterInfo.exitCellRiverPoint, cellWaterInfo.exitCellRiverPointHeight);
    //         foreach (Tuple<Vector2I, float> riverTile in riverLineTiles)
    //         {
    //             Vector2I indexVec = riverTile.Item1 - cellOrigin;
    //             if (0 <= indexVec.X && indexVec.X < cellSize &&
    //             0 <= indexVec.Y && indexVec.Y < cellSize)
    //             {
    //                 tiles[indexVec.X, indexVec.Y] = new(riverTile.Item2, TileType.River);
    //             }
    //         }
    //     }
    // }

    // private static void SetRiverLine(List<Tuple<Vector2I, float>> riverLineTiles, Vector2I v0, float v0Height, Vector2I v1, float v1Height)
    // {
    //     int dX = Mathf.Abs(v1.X - v0.X);
    //     int dY = Mathf.Abs(v1.Y - v0.Y);
    //     int isVertical = 0;
    //     int isHorizontal = 1;
    //     if (dX < dY)
    //     {
    //         isVertical = 1;
    //         isHorizontal = 0;
    //         v0 = new(v0.Y, v0.X);
    //         v1 = new(v1.Y, v1.X);
    //     }

    //     if (v1.X < v0.X)
    //     {
    //         (v0, v1) = (v1, v0);
    //         (v0Height, v1Height) = (v1Height, v0Height);
    //     }
    //     Vector2I delta = new(v1.X - v0.X, v1.Y - v0.Y);
    //     float heightRateOfChange = (v1Height - v0Height) / delta.X;
    //     int dir = delta.Y < 0 ? -1 : 1;
    //     delta.Y *= dir;
    //     if (delta.X != 0)
    //     {
    //         int y = v0.Y;
    //         int p = 2 * delta.Y - delta.X;
    //         for (int i = 0; i < delta.X; i++)
    //         {
    //             int tileX = (v0.X + i) * isHorizontal + y * isVertical;
    //             int tileY = (v0.X + i) * isVertical + y * isHorizontal;
    //             riverLineTiles.Add(new(new(tileX, tileY), v0Height + heightRateOfChange * i));
    //             if (0 <= p)
    //             {
    //                 y += dir;
    //                 p -= 2 * delta.X;
    //             }
    //             p += 2 * delta.Y;
    //         }
    //     }
    // }

    // private void SetTiles()
    // {
    //     for (int x = 0; x < cellSize; x++)
    //     {
    //         for (int y = 0; y < cellSize; y++)
    //         {
    //             if (tiles[x, y] == null)
    //             {
    //                 float height = GetHeight(options, new Vector2I(cellOrigin.X + x, cellOrigin.Y + y));
    //                 if (height < 0.2f)
    //                 {
    //                     tiles[x, y] = new(height, TileType.Ocean);
    //                 }
    //                 else
    //                 {
    //                     tiles[x, y] = new(height, TileType.Land);
    //                 }
    //             }
    //         }
    //     }
    // }

    // public Vector2I GetMinVPoint(Vector2 v, int vCellSize)
    // {
    //     Vector2I centerGridOrigin = GetCellOrigin(v, vCellSize);
    //     Vector2I closestVPoint = GetVPoint(centerGridOrigin, vCellSize);
    //     float minDist = v.DistanceTo(closestVPoint);
    //     Span<Vector2I> neighborGridOrigins = stackalloc Vector2I[]
    //     {
    //         new(centerGridOrigin.X - vCellSize, centerGridOrigin.Y - vCellSize),
    //         new(centerGridOrigin.X            , centerGridOrigin.Y - vCellSize),
    //         new(centerGridOrigin.X + vCellSize, centerGridOrigin.Y - vCellSize),
    //         new(centerGridOrigin.X - vCellSize, centerGridOrigin.Y            ),
    //         new(centerGridOrigin.X + vCellSize, centerGridOrigin.Y            ),
    //         new(centerGridOrigin.X - vCellSize, centerGridOrigin.Y + vCellSize),
    //         new(centerGridOrigin.X            , centerGridOrigin.Y + vCellSize),
    //         new(centerGridOrigin.X + vCellSize, centerGridOrigin.Y + vCellSize),
    //     };
    //     foreach (Vector2I neighborGridOrigin in neighborGridOrigins)
    //     {
    //         Vector2I currVPoint = GetVPoint(neighborGridOrigin, vCellSize);
    //         float currDist = v.DistanceTo(currVPoint);
    //         if (currDist < minDist)
    //         {
    //             minDist = currDist;
    //             closestVPoint = currVPoint;
    //         }
    //     }
    //     return closestVPoint;
    // }

    // private int GetHeight(Vector2I v)
    // {
    //     int currCellSize = _landSize / (int)Mathf.Pow(2, _vLayers);
    //     Span<(int, Vector2I)> minVPoints = stackalloc (int, Vector2I)[_vLayers + 1];
    //     Vector2I currVPoint = v;
    //     for (int i = 0; i <= _vLayers; i++)
    //     {
    //         minVPoints[i] = (currCellSize, GetMinVPoint(currVPoint, currCellSize));
    //         if (landVPoints[currCellSize].Contains(minVPoints[i].Item2))
    //         {
    //             for (int j = 0; j < i; j++)
    //             {
    //                 landVPoints[minVPoints[j].Item1].Add(minVPoints[j].Item2);
    //             }
    //             return 125;
    //         }
    //         if (oceanVPoints[currCellSize].Contains(minVPoints[i].Item2))
    //         {
    //             for (int j = 0; j < i; j++)
    //             {
    //                 oceanVPoints[minVPoints[j].Item1].Add(minVPoints[j].Item2);
    //             }
    //             return 32;
    //         }
    //         currVPoint = minVPoints[i].Item2;
    //         currCellSize *= 2;
    //     }

    //     // Vector2I vOffset = v - Vector2I.One * _mapSize / 2;
    //     // float xDistMult = 1f - Mathf.Pow(vOffset.X * 2f / _mapSize, 2f);
    //     // float yDistMult = 1f - Mathf.Pow(vOffset.Y * 2f / _mapSize, 2f);
    //     // float distMult = Mathf.Lerp(xDistMult * yDistMult, 1f, finalHeight);
    //     // float layer0Height = finalHeight * distMult;
    //     foreach ((int, Vector2I) minVPoint in minVPoints)
    //     {
    //         oceanVPoints[minVPoint.Item1].Add(minVPoint.Item2);
    //     }
    //     return 32;
    // }

    public ImageTexture GetIMGTEX()
    {
        byte[] data = new byte[_cellSize * _cellSize * 3];
        int vCellSize = _mapSize / (int)Mathf.Pow(2, _numVLayers);
        landNoise.PopulateLayers(CellOrigin, _cellSize);
        Vector2I topLeftVCell = LandNoise.GetCellOrigin(CellOrigin, vCellSize);
        Vector2I bottomRightVCell = LandNoise.GetCellOrigin(CellOrigin + Vector2I.One * (_cellSize - 1), vCellSize);
        for (int x = topLeftVCell.X; x <= bottomRightVCell.X; x += vCellSize)
        {
            for (int y = topLeftVCell.Y; y <= bottomRightVCell.Y; y += vCellSize)
            {
                Vector2I currVCellOrigin = new(x, y);
                landNoise.SetByteArray(data, CellOrigin, _cellSize, currVCellOrigin, vCellSize);
            }
        }
        Image img = Image.CreateFromData(_cellSize, _cellSize, false, Image.Format.Rgb8, data);
        return ImageTexture.CreateFromImage(img);
    }

}