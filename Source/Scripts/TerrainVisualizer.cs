using Godot;
using System;


public partial class TerrainVisualizer : Node2D
{
	[Export] int seed = 0;
	[Export] int voronoiTileSize = 10;
	[Export] int width = 128;
	[Export] int height = 128;
	[Export] Sprite2D sprite;
	RNG rng;
    int[] imageData;
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Image image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
		sprite.Texture = ImageTexture.CreateFromImage(image);
		
		imageData = new int[width * height];
		Init();
	}


	private void Init()
	{
		rng = new((uint)(seed));
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				Vector2I currentPos = new(x, y);
				Vector2I currentVoronoiChunk = GetVoronoiChunk(currentPos);
				float closestVoronoiValue = float.MaxValue;
				for(int a = -1; a < 2; a++)
				{
					for(int b = -1; b < 2; b++)
					{
						Vector2I voronoiChunk = currentVoronoiChunk + new Vector2I(a * voronoiTileSize, b * voronoiTileSize);
						Vector2I voronoiPos = voronoiChunk + GetVoronoiPos(voronoiChunk);
						float distance = currentPos.DistanceTo(voronoiPos);
						if(distance < closestVoronoiValue)
						{
							closestVoronoiValue = distance;
						}
					}
				}
				float ratio = closestVoronoiValue / voronoiTileSize;
				imageData[x * width + y] = ;


				Vector2I currentVoronoiPos = GetVoronoiPos(currentVoronoiChunk);


			}
		}

	}

	private Vector2I GetVoronoiChunk(Vector2I pos)
	{
		int x = Mathf.FloorToInt(pos.X / voronoiTileSize);
		int y = Mathf.FloorToInt(pos.Y / voronoiTileSize);
		return new Vector2I(x, y);
	}

	private Vector2I GetVoronoiPos(Vector2I voronoiChunk)
	{
		return rng.GetRand2DRange(voronoiChunk, voronoiTileSize, voronoiTileSize);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
