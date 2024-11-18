using Godot;

public class WaterChunk
{
    public Vector2I origin;
    public int size;
    public Vector2 voronoiPoint;

    public WaterChunk(Vector2I _origin, int _size, Vector2 _voronoiPoint){
        origin = _origin;
        size = _size;
        voronoiPoint = _voronoiPoint;
    }
}