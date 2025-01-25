using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

[Tool]
public partial class WorldVisualizer : Node2D
{
    [Export] public Options options;

    private readonly List<Tuple<Cell, Task<ImageTexture>>> cells = new();

    private readonly List<Sprite2D> cellSprites = new();

    public void Init()
    {
        Reset();
        CreateCells();
    }

    private void Reset()
    {
        foreach (Sprite2D sprite in cellSprites)
        {
            sprite.QueueFree();
        }
        cellSprites.Clear();
        cells.Clear();
    }

    private void CreateCells()
    {
        for (int x = 0; x < options.NumCells; x++)
        {
            for (int y = 0; y < options.NumCells; y++)
            {
                Vector2I cellOrigin = new Vector2I(x, y) * options.CellSize;
                Cell cell = new(cellOrigin, options.CellSize, options.MapSize, options.Seed, options.VLayers);
                cells.Add(new(cell, Task.Run(() => cell.GetIMGTEX())));
            }
        }
        foreach (Tuple<Cell, Task<ImageTexture>> cell in cells)
        {
            Sprite2D sprite = new()
            {
                Texture = cell.Item2.Result,
                Position = cell.Item1.CellOrigin + Vector2I.One * (options.CellSize / 2),
                TextureFilter = TextureFilterEnum.Nearest,
            };
            cellSprites.Add(sprite);
            AddChild(sprite);
        }
    }
}
