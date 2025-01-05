using System;
using Godot;

public class VoronoiGrid
{
    private Options options;
    private int gridSize;
    private int cellSize;
    private VoronoiCell[] vCells;
    public VoronoiGrid(Options options)
    {
        this.options = options;
        this.gridSize = options.VGridSize;
        this.cellSize = options.VCellSize;
        SetVCells();
    }

    private void SetVCells()
    {
        vCells = new VoronoiCell[gridSize * gridSize];
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                int offset = gridSize * cellSize / 2;
                int xPos = x * cellSize - offset;
                int yPos = y * cellSize - offset;
                vCells[x + y * gridSize] = new(options, new(xPos, yPos));
            }
        }
    }

    public ImageTexture GetImgTex()
    {
        int pixelsPerSide = (gridSize + 2) * cellSize;
        byte[] data = new byte[pixelsPerSide * pixelsPerSide * 3];
        foreach (VoronoiCell vCell in vCells)
        {
            foreach (Vector2I tile in vCell.borderTiles)
            {
                TryPutData(data, tile, 0);
            }
        }
        Image img = Image.CreateFromData(pixelsPerSide, pixelsPerSide, false, Image.Format.Rgb8, data);
        return ImageTexture.CreateFromImage(img);
    }

    public void TryPutData(byte[] data, Vector2I pos, int byteOffset)
    {
        int pixelsPerSide = (gridSize + 2) * cellSize;
        if (-pixelsPerSide / 2 < pos.X && pos.X < pixelsPerSide / 2 &&
        -pixelsPerSide / 2 < pos.Y && pos.Y < pixelsPerSide / 2)
        {
            data[WorldPosToImgDataIndex(pos) + byteOffset] = 255;
        }
    }

    private int WorldPosToImgDataIndex(Vector2I pos)
    {
        int offset = (gridSize + 2) * cellSize / 2;
        return ((pos.X + offset) + (pos.Y + offset) * offset * 2) * 3;
    }
}