using Godot;

[Tool]
public partial class Options : Node
{
    private uint _seed = 0;
    public int MapSize = 6400;
    private int _cellSize = 320;
    private int _numCells = 1;
    private FastNoiseLite.NoiseTypeEnum _noiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
    private FastNoiseLite.FractalTypeEnum _fractalType = FastNoiseLite.FractalTypeEnum.Fbm;
    private int _fractalOctaves = 7;
    private float _frequency = 35f;
    [Export] public uint Seed { get => _seed; set { _seed = value; Reset(); } }
    [Export] public int CellSize { get => _cellSize; set { _cellSize = value; Reset(); } }
    [Export] public int NumCells { get => _numCells; set { _numCells = value; Reset(); } }
    // [Export] public FastNoiseLite.NoiseTypeEnum NoiseType { get => _noiseType; set { _noiseType = value; Reset(); } }
    // [Export] public FastNoiseLite.FractalTypeEnum FractalType { get => _fractalType; set { _fractalType = value; Reset(); } }
    [Export] public int FractalOctaves { get => _fractalOctaves; set { _fractalOctaves = value; Reset(); } }
    [Export] public float Frequency { get => _frequency; set { _frequency = value; Reset(); } }
    [Export] public WorldVisualizer worldVisualizer;
    public FastNoiseLite noise;
    public RNG rng;

    private void Reset()
    {
        if (IsInsideTree())
        {
            NotifyPropertyListChanged();
            rng = new(_seed);
            noise = new()
            {
                Seed = (int)_seed,
                Frequency = _frequency / 100000f,
                NoiseType = _noiseType,
                FractalType = _fractalType,
                FractalOctaves = _fractalOctaves,
            };
            worldVisualizer.Init();
        }
    }

    public override void _Ready()
    {
        Reset();
    }
}
