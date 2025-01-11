using System;
using System.Collections.Generic;
using Godot;


public class Cell
{
    private enum TileType
    {
        Land = 0,
        River = 1,
        Lake = 2,
        Ocean = 3,
    };
    private readonly Options options;
    public readonly Vector2I cellOrigin;
    private readonly int cellSize;

    private readonly Vector2I riverPoint;
    private readonly float riverPointHeight;

    private readonly Vector2I exitCellOrigin;
    private readonly Vector2I exitRiverPoint;
    private readonly float exitRiverPointHeight;

    private readonly (float, TileType)[,] tiles;
    public Cell(Options options, Vector2I cellOrigin)
    {
        this.options = options;
        this.cellOrigin = cellOrigin;
        this.cellSize = options.CellSize;
        (this.riverPoint, this.riverPointHeight) = GetRiverPoint(cellOrigin);
        (this.exitCellOrigin, this.exitRiverPoint, this.exitRiverPointHeight) = GetExitCellInfo(cellOrigin);
        this.tiles = new (float, TileType)[cellSize, cellSize];
        SetRivers();
        SetTiles();
    }

    private (Vector2I, float) GetRiverPoint(Vector2I myCellOrigin)
    {
        Vector2I pos = myCellOrigin + new Vector2I(cellSize / 4, cellSize / 4);
        Vector2I randPos = options.rng.GetRandVec2I(myCellOrigin, cellSize / 2, cellSize / 2) + pos;
        return (randPos, GetHeight(randPos.X, randPos.Y));
    }

    private float GetHeight(float x, float y)
    {
        float baseHeight = (options.noise.GetNoise2D(x, y) + 1.0f) * 0.5f;
        float xDistanceMultiplier = 1f - Mathf.Pow(x * 2 / options.MapSize, 2f);
        float yDistanceMultiplier = 1f - Mathf.Pow(y * 2 / options.MapSize, 2f);
        return baseHeight * xDistanceMultiplier * yDistanceMultiplier;
    }

    public void SetRivers()
    {
        List<Tuple<Vector2I, float>> riverLineTiles = new();
        for (int i = -1; i <= 1; i += 2)
        {
            int tmp = i * cellSize;
            for (int j = 0; j < 2; j++)
            {
                Vector2I neighborOrigin = cellOrigin + new Vector2I(j, 1 - j) * tmp;
                (Vector2I, Vector2I, float) neighborExitCellInfo = GetExitCellInfo(neighborOrigin);
                if (cellOrigin == neighborExitCellInfo.Item1)
                {
                    // river flows into my chunk
                    SetRiverLine(riverLineTiles, riverPoint, riverPointHeight, neighborExitCellInfo.Item2, neighborExitCellInfo.Item3);
                    GD.Print(riverLineTiles.Count);
                    foreach (Tuple<Vector2I, float> riverTile in riverLineTiles)
                    {
                        Vector2I indexVec = riverTile.Item1 - cellOrigin;
                        // if (cellOrigin == new Vector2I(320, 0))
                        // {
                        //     // GD.Print(indexVec);
                        // }
                        if (0 <= indexVec.X && indexVec.X < cellSize &&
                        0 <= indexVec.Y && indexVec.Y < cellSize)
                        {
                            // GD.Print(cellOrigin, " has neighbor: ", neighborOrigin, " that flows in");
                            tiles[indexVec.X, indexVec.Y] = (riverTile.Item2, TileType.River);
                        }
                    }
                    riverLineTiles.Clear();
                }
            }
        }
        if (cellOrigin != exitCellOrigin)
        {
            SetRiverLine(riverLineTiles, riverPoint, riverPointHeight, exitRiverPoint, exitRiverPointHeight);
            foreach (Tuple<Vector2I, float> riverTile in riverLineTiles)
            {
                Vector2I indexVec = riverTile.Item1 - cellOrigin;
                if (0 <= indexVec.X && indexVec.X < cellSize &&
                0 <= indexVec.Y && indexVec.Y < cellSize)
                {
                    tiles[indexVec.X, indexVec.Y] = (riverTile.Item2, TileType.River);
                }
            }
        }
    }

    private (Vector2I, Vector2I, float) GetExitCellInfo(Vector2I myCellOrigin)
    {
        (Vector2I, float) myRiverPoint = GetRiverPoint(myCellOrigin);
        (Vector2I, Vector2I, float) exitCellInfo = (myCellOrigin, myRiverPoint.Item1, myRiverPoint.Item2);
        for (int i = -1; i <= 1; i += 2)
        {
            int tmp = i * cellSize;
            for (int j = 0; j < 2; j++)
            {
                Vector2I otherCellOrigin = myCellOrigin + new Vector2I(j, 1 - j) * tmp;
                (Vector2I, float) otherRiverPoint = GetRiverPoint(otherCellOrigin);
                if (otherRiverPoint.Item2 < exitCellInfo.Item3)
                {
                    exitCellInfo = (otherCellOrigin, otherRiverPoint.Item1, otherRiverPoint.Item2);
                }
            }
        }
        return exitCellInfo;
    }

    // private (Vector2I, float, Vector2I, float) GetExitRiverPoints(Vector2I myCellOrigin)
    // {
    //     (Vector2I, float) myRiverPoint = GetRiverPoint(myCellOrigin);
    //     (Vector2I, float) currExitCell = (myRiverPoint, myRiverPointHeight);
    //     for (int i = -1; i <= 1; i += 2)
    //     {
    //         int tmp = i * cellSize;
    //         for (int j = 0; j < 2; j++)
    //         {
    //             Vector2I currOrigin = cellOrigin + new Vector2I(j, 1 - j) * tmp;
    //             Vector2I currRiverPoint = currOrigin + options.rng.GetRandVec2I(currOrigin, cellSize, cellSize);
    //             float height = GetHeight(currRiverPoint.X, currRiverPoint.Y);
    //             if (height < currExitCell.Item2)
    //             {
    //                 currExitCell = (currRiverPoint, height);
    //             }
    //         }
    //     }
    //     return (myRiverPoint, myRiverPointHeight, currExitCell.Item1, currExitCell.Item2);
    // }

    // Span<(Vector2I, float)> riverPoints5x5 = stackalloc (Vector2I, float)[25];
    // for (int x = -2; x < 3; x++)
    // {
    //     for (int y = -2; y < 3; y++)
    //     {
    //         Vector2I currOrigin = cellOrigin + new Vector2I(x, y) * options.CellSize;
    //         int currIndex = (x + 2) + (y + 2) * 5;
    //         int size = options.CellSize / 2;
    //         Vector2I offset = Vector2I.One * (options.CellSize / 4);
    //         Vector2I riverPoint = cellOrigin + options.rng.GetRandVec2I(currOrigin, size, size) + offset;
    //         float riverPointHeight = GetHeight(riverPoint.X, riverPoint.Y);
    //         riverPoints5x5[currIndex] = (riverPoint, riverPointHeight);
    //     }
    // }
    // List<Vector2I> riverTiles = new();
    // for (int x = -1; x < 2; x++)
    // {
    //     for (int y = -1; y < 2; y++)
    //     {
    //         Vector2I currOrigin = cellOrigin + new Vector2I(x, y) * options.CellSize;
    //         int currIndex = (x + 2) + (y + 2) * 5;
    //         int exitCellIndex = GetExitIndex(riverPoints5x5, currIndex);
    //         if (currIndex != 12)
    //         {
    //             if (exitCellIndex == 12)
    //             {
    //                 SetLine(riverTiles, riverPoints5x5[currIndex].Item1, riverPoints5x5[exitCellIndex].Item1);
    //                 foreach (Vector2I riverTile in riverTiles)
    //                 {
    //                     if (cellOrigin.X <= riverTile.X && riverTile.X < cellOrigin.X + options.CellSize &&
    //                     cellOrigin.Y <= riverTile.Y && riverTile.Y < cellOrigin.Y + options.CellSize)
    //                     {
    //                         int xPos = riverTile.X - cellOrigin.X;
    //                         int yPos = riverTile.Y - cellOrigin.Y;
    //                         tiles[xPos, yPos].Item2 = TileType.River;
    //                     }
    //                 }
    //                 riverTiles.Clear();
    //             }
    //         }
    //         else
    //         {
    //             if (exitCellIndex == 12)
    //             {
    //                 // has lake
    //             }
    //             else
    //             {
    //                 // river flows out
    //             }
    //         }
    //         // if ((i == 12 && exit != 12) || (i != 12 && exit == 12))
    //         // {
    //         //     SetLine(riverTiles, neighbors5x5[i].Item1, neighbors5x5[exit].Item1);
    //         //     foreach (Vector2I riverTile in riverTiles)
    //         //     {
    //         //         if (cellOrigin.X <= riverTile.X && riverTile.X < cellOrigin.X + options.CellSize &&
    //         //         cellOrigin.Y <= riverTile.Y && riverTile.Y < cellOrigin.Y + options.CellSize)
    //         //         {
    //         //             int xPos = riverTile.X - cellOrigin.X;
    //         //             int yPos = riverTile.Y - cellOrigin.Y;
    //         //             tileHeights[xPos, yPos] = 1.0f;
    //         //         }
    //         //     }
    //         // }
    //     }
    // }

    // public static int GetExitIndex(Span<(Vector2I, float)> riverPoints5x5, int i)
    // {
    //     int exit = i;
    //     for (int x = -1; x < 2; x++)
    //     {
    //         for (int y = -1; y < 2; y++)
    //         {
    //             int otherI = i + x + y * 5;
    //             if (otherI != i && riverPoints5x5[otherI].Item2 < riverPoints5x5[exit].Item2)
    //             {
    //                 exit = otherI;
    //             }
    //         }
    //     }
    //     return exit;
    // }

    private static void SetRiverLine(List<Tuple<Vector2I, float>> riverLineTiles, Vector2I v0, float v0Height, Vector2I v1, float v1Height)
    {
        int dX = Mathf.Abs(v1.X - v0.X);
        int dY = Mathf.Abs(v1.Y - v0.Y);
        int isVertical = 0;
        int isHorizontal = 1;
        if (dX < dY)
        {
            isVertical = 1;
            isHorizontal = 0;
            v0 = new(v0.Y, v0.X);
            v1 = new(v1.Y, v1.X);
        }

        if (v1.X < v0.X)
        {
            (v0, v1) = (v1, v0);
            (v0Height, v1Height) = (v1Height, v0Height);
        }
        Vector2I delta = new(v1.X - v0.X, v1.Y - v0.Y);
        float heightRateOfChange = (v1Height - v0Height) / delta.X;
        int dir = delta.Y < 0 ? -1 : 1;
        delta.Y *= dir;
        if (delta.X != 0)
        {
            int y = v0.Y;
            int p = 2 * delta.Y - delta.X;
            for (int i = 0; i < delta.X; i++)
            {
                int tileX = (v0.X + i) * isHorizontal + y * isVertical;
                int tileY = (v0.X + i) * isVertical + y * isHorizontal;
                riverLineTiles.Add(new(new(tileX, tileY), v0Height + heightRateOfChange * i));
                if (0 <= p)
                {
                    y += dir;
                    p -= 2 * delta.X;
                }
                p += 2 * delta.Y;
            }
        }
    }

    private void SetTiles()
    {
        for (int x = 0; x < cellSize; x++)
        {
            for (int y = 0; y < cellSize; y++)
            {
                (float, TileType) tile = tiles[x, y];
                if (tile.Item2 != TileType.River)
                {
                    tile.Item1 = GetHeight(x + cellOrigin.X, y + cellOrigin.Y);
                    tiles[x, y].Item1 = tile.Item1;
                    if (tile.Item1 < 0.2f)
                    {
                        tiles[x, y].Item2 = TileType.Ocean;
                    }
                    else
                    {
                        tiles[x, y].Item2 = TileType.Land;
                    }
                }
            }
        }
    }

    public ImageTexture GetIMGTEX()
    {
        byte[] data = new byte[cellSize * cellSize * 3];
        for (int x = 0; x < cellSize; x++)
        {
            for (int y = 0; y < cellSize; y++)
            {
                int i = (x + y * cellSize) * 3;
                (float, TileType) tile = tiles[x, y];
                if (x == 0 || y == 0 || x == cellSize - 1 || y == cellSize - 1)
                {
                    data[i] = 255;
                }
                else if (tile.Item2 == TileType.River)
                {
                    data[i + 2] = (byte)(Mathf.FloorToInt(tile.Item1 * 50) * 5);
                }
                else if (tile.Item2 == TileType.Land)
                {
                    data[i + 1] = (byte)(Mathf.FloorToInt(tile.Item1 * 50) * 5);
                }
                else
                {
                    data[i + 2] = (byte)(Mathf.FloorToInt(tile.Item1 * 50) * 5);
                }
            }
        }
        Image img = Image.CreateFromData(cellSize, cellSize, false, Image.Format.Rgb8, data);
        return ImageTexture.CreateFromImage(img);
    }

}