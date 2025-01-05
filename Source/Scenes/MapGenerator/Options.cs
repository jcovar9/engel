using Godot;

[Tool]
public partial class Options : Node
{
    private uint _seed = 0;
    private int _vGridSize = 50;
    private int _vCellSize = 128;
    private FastNoiseLite.NoiseTypeEnum _noiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
    private FastNoiseLite.FractalTypeEnum _fractalType = FastNoiseLite.FractalTypeEnum.Fbm;
    private int _fractalOctaves = 5;
    private float _frequency = 35f;
    private bool _edgesInsteadBorders = false;
    [Export] public uint Seed { get => _seed; set { _seed = value; Reset(); } }
    [Export] public int VGridSize { get => _vGridSize; set { _vGridSize = value; Reset(); } }
    [Export] public int VCellSize { get => _vCellSize; set { _vCellSize = value; Reset(); } }
    [Export] public FastNoiseLite.NoiseTypeEnum NoiseType { get => _noiseType; set { _noiseType = value; Reset(); } }
    [Export] public FastNoiseLite.FractalTypeEnum FractalType { get => _fractalType; set { _fractalType = value; Reset(); } }
    [Export] public int FractalOctaves { get => _fractalOctaves; set { _fractalOctaves = value; Reset(); } }
    [Export] public float Frequency { get => _frequency; set { _frequency = value; Reset(); } }
    [Export] public bool EdgesInsteadBorders { get => _edgesInsteadBorders; set { _edgesInsteadBorders = value; Reset(); } }
    //[Export] public Shader shader;
    [Export] public WorldVisualizer worldVisualizer;
    public FastNoiseLite chunkVertexNoise;
    public RNG rng;

    private void Reset()
    {
        if (IsInsideTree())
        {
            NotifyPropertyListChanged();
            rng = new(_seed);
            chunkVertexNoise = new()
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
