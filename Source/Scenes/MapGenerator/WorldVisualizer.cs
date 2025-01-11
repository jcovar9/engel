using System.Collections.Generic;
using Godot;

[Tool]
public partial class WorldVisualizer : Node2D
{
    [Export] public Options options;

    //private float[,] chunkVertexHeights = new float[100,100];
    //private HashSet<Vector2I> oceanTiles = new();
    private readonly List<Sprite2D> cellSprites = new();

    public void Init()
    {
        FreePreviousSprites();
        CreateCells();
    }

    private void FreePreviousSprites()
    {
        foreach (Sprite2D sprite in cellSprites)
        {
            sprite.QueueFree();
        }
        cellSprites.Clear();
    }

    private void CreateCells()
    {
        int numCells = options.NumCells;
        int offset = numCells / 2 * options.CellSize;
        for (int x = 0; x < numCells; x++)
        {
            for (int y = 0; y < numCells; y++)
            {
                int xPos = x * options.CellSize - offset;
                int yPos = y * options.CellSize - offset;
                Sprite2D sprite = new()
                {
                    Texture = new Cell(options, new(xPos, yPos)).GetIMGTEX(),
                    Position = new(xPos + options.CellSize / 2, yPos + options.CellSize / 2),
                    TextureFilter = TextureFilterEnum.Nearest,
                };
                cellSprites.Add(sprite);
                AddChild(sprite);
            }
        }
    }
}
