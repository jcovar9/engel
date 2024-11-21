using Godot;

public class WaterChunk
{
    public Vector2I origin;
    public int size;
    public Vector2I voronoiPoint;
    public int[] heightData;

    public WaterChunk(Vector2I _origin, int _size, Vector2I _voronoiPoint){
        origin = _origin;
        size = _size;
        voronoiPoint = _voronoiPoint;
        heightData = new int[_size * _size];
    }
}