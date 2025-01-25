using Godot;

[Tool]
public partial class Options : Node
{
    private uint _seed = 0;
    [Export] public uint Seed { get => _seed; set { _seed = value; Reset(); } }
    private int _cellSize = 200;
    [Export] public int CellSize { get => _cellSize; set { _cellSize = value; Reset(); } }
    private int _numCells = 32;
    [Export] public int NumCells { get => _numCells; set { _numCells = value; Reset(); } }
    private int _vLayers = 5;
    [Export] public int VLayers { get => _vLayers; set { _vLayers = value; Reset(); } }
    [Export] public WorldVisualizer worldVisualizer;
    public int MapSize = 6400;

    public override void _Ready()
    {
        Reset();
    }

    private void Reset()
    {
        if (IsInsideTree())
        {
            NotifyPropertyListChanged();
            worldVisualizer.Init();
        }
    }
}
