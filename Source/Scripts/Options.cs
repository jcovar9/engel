using Godot;

[Tool]
public partial class Options : Node
{
    private uint _seed = 0;
    private int _mapSize = 1;
    private int _chunkSize = 128;
    private int _viewSize = 800;
    private FastNoiseLite.NoiseTypeEnum _noiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
    private FastNoiseLite.FractalTypeEnum _fractalType = FastNoiseLite.FractalTypeEnum.None;
    private int _fractalOctaves = 1;
    private float _frequency = 0.01f;
    [Export] public uint Seed { get => _seed; set { _seed = value; Reset(); } }
    [Export] public int MapSize { get => _mapSize; set { _mapSize = value; Reset(); } }
    [Export] public int ChunkSize { get => _chunkSize; set { _chunkSize = value; Reset(); } }
    [Export] public int ViewSize { get => _viewSize; set { _viewSize = value; Reset(); } }
    [Export] public FastNoiseLite.NoiseTypeEnum NoiseType { get => _noiseType; set { _noiseType = value; Reset(); } }
    [Export] public FastNoiseLite.FractalTypeEnum FractalType { get => _fractalType; set { _fractalType = value; Reset(); } }
    [Export] public int FractalOctaves { get => _fractalOctaves; set { _fractalOctaves = value; Reset(); } }
    [Export] public float Frequency { get => _frequency; set { _frequency = value; Reset(); } }
    [Export] public Shader shader;
    [Export] public ChunkMap chunkMap;
    public FastNoiseLite chunkVertexNoise;
    public RNG rng;

    private void Reset()
    {
        if(IsInsideTree())
        {
            NotifyPropertyListChanged();
            rng = new(_seed);
            chunkVertexNoise = new()
            {
                Seed = (int)_seed,
                Frequency = _frequency,
                NoiseType = _noiseType,
                FractalType = _fractalType,
                FractalOctaves = _fractalOctaves,
            };
            chunkMap.Init();
        }
    }

    public override void _Ready()
    {
        Reset();
    }
}
