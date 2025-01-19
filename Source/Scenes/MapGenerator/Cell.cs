using System;
using System.Collections.Generic;
using Godot;


public class Cell
{
    private struct CellWaterInfo
    {
        public int cellSize;
        public Vector2I cellOrigin;
        public Vector2I riverPoint;
        public float riverPointHeight;
        public Vector2I exitCellOrigin;
        public Vector2I exitCellRiverPoint;
        public float exitCellRiverPointHeight;
        public CellWaterInfo(Options options, Vector2I cellOrigin)
        {
            this.cellSize = options.CellSize;
            this.cellOrigin = cellOrigin;
            this.riverPoint = GetRiverPoint(options, cellOrigin);
            this.riverPointHeight = GetHeight(options, riverPoint);
            Span<Vector2I> otherCellOrigins = stackalloc Vector2I[]
            {
                new Vector2I(cellOrigin.X, cellOrigin.Y - cellSize),
                new Vector2I(cellOrigin.X - cellSize, cellOrigin.Y),
                new Vector2I(cellOrigin.X + cellSize, cellOrigin.Y),
                new Vector2I(cellOrigin.X, cellOrigin.Y + cellSize),
            };
            this.exitCellOrigin = cellOrigin;
            this.exitCellRiverPoint = riverPoint;
            this.exitCellRiverPointHeight = riverPointHeight;
            foreach (Vector2I otherCellOrigin in otherCellOrigins)
            {
                Vector2I otherCellRiverPoint = GetRiverPoint(options, otherCellOrigin);
                float otherCellRiverPointHeight = GetHeight(options, otherCellRiverPoint);
                if(otherCellRiverPointHeight < exitCellRiverPointHeight)
                {
                    this.exitCellOrigin = otherCellOrigin;
                    this.exitCellRiverPoint = otherCellRiverPoint;
                    this.exitCellRiverPointHeight = otherCellRiverPointHeight;
                }
            }
        }
    }
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
    private CellWaterInfo cellWaterInfo;

    private readonly (float, TileType)[,] tiles;
    public Cell(Options options, Vector2I cellOrigin)
    {
        this.options = options;
        this.cellOrigin = cellOrigin;
        this.cellSize = options.CellSize;
        this.cellWaterInfo = new(options, cellOrigin);
        this.tiles = new (float, TileType)[cellSize, cellSize];
        SetRivers();
        SetTiles();
    }

    private static Vector2I GetRiverPoint(Options options, Vector2I origin)
    {
        return options.rng.GetRandVec2I(origin, options.CellSize / 2, options.CellSize / 2) + origin + new Vector2I(options.CellSize / 4, options.CellSize / 4);
    }

    private static float GetHeight(Options options, Vector2I v)
    {
        float baseHeight = (options.noise.GetNoise2D(v.X, v.Y) + 1.0f) * 0.5f;
        float xDistanceMultiplier = 1f - Mathf.Pow(v.X * 2 / options.MapSize, 2f);
        float yDistanceMultiplier = 1f - Mathf.Pow(v.Y * 2 / options.MapSize, 2f);
        return baseHeight * xDistanceMultiplier * yDistanceMultiplier;
    }

    public void SetRivers()
    {
        List<Tuple<Vector2I, float>> riverLineTiles = new();
        Span<Vector2I> otherCellOrigins = stackalloc Vector2I[]
        {
            new Vector2I(cellOrigin.X, cellOrigin.Y - cellSize),
            new Vector2I(cellOrigin.X - cellSize, cellOrigin.Y),
            new Vector2I(cellOrigin.X + cellSize, cellOrigin.Y),
            new Vector2I(cellOrigin.X, cellOrigin.Y + cellSize),
        };
        foreach (Vector2I otherCellOrigin in otherCellOrigins)
        {
            CellWaterInfo otherCellWaterInfo = new(options, otherCellOrigin);
            if(cellOrigin == otherCellWaterInfo.exitCellOrigin)
            {
                SetRiverLine(riverLineTiles, cellWaterInfo.riverPoint, cellWaterInfo.riverPointHeight, otherCellWaterInfo.riverPoint, otherCellWaterInfo.riverPointHeight);
                // GD.Print(riverLineTiles.Count);
                foreach (Tuple<Vector2I, float> riverTile in riverLineTiles)
                {
                    Vector2I indexVec = riverTile.Item1 - cellOrigin;
                    if (0 <= indexVec.X && indexVec.X < cellSize &&
                    0 <= indexVec.Y && indexVec.Y < cellSize)
                    {
                        tiles[indexVec.X, indexVec.Y] = (riverTile.Item2, TileType.River);
                    }
                }
                riverLineTiles.Clear();
            }
        }
        if (cellOrigin != cellWaterInfo.exitCellOrigin)
        {
            SetRiverLine(riverLineTiles, cellWaterInfo.riverPoint, cellWaterInfo.riverPointHeight, cellWaterInfo.exitCellRiverPoint, cellWaterInfo.exitCellRiverPointHeight);
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
        // for (int i = -1; i <= 1; i += 2)
        // {
        //     int tmp = i * cellSize;
        //     for (int j = 0; j < 2; j++)
        //     {
        //         Vector2I neighborOrigin = cellOrigin + new Vector2I(j, 1 - j) * tmp;
        //         (Vector2I, Vector2I, float) neighborExitCellInfo = GetExitCellInfo(neighborOrigin);
        //         if (cellOrigin == neighborExitCellInfo.Item1)
        //         {
        //             // river flows into my cell
        //             // riverLineTiles list is coming back empty everytime
        //             SetRiverLine(riverLineTiles, riverPoint, riverPointHeight, neighborExitCellInfo.Item2, neighborExitCellInfo.Item3);
        //             GD.Print(riverLineTiles.Count);
        //             foreach (Tuple<Vector2I, float> riverTile in riverLineTiles)
        //             {
        //                 Vector2I indexVec = riverTile.Item1 - cellOrigin;
        //                 if (0 <= indexVec.X && indexVec.X < cellSize &&
        //                 0 <= indexVec.Y && indexVec.Y < cellSize)
        //                 {
        //                     tiles[indexVec.X, indexVec.Y] = (riverTile.Item2, TileType.River);
        //                 }
        //             }
        //             riverLineTiles.Clear();
        //         }
        //     }
        // }
    }

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
                    tile.Item1 = GetHeight(options, cellOrigin + new Vector2I(x, y));//x + cellOrigin.X, y + cellOrigin.Y);
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