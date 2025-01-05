using System;
using System.Collections.Generic;
using Godot;

public class ChunkVertex
{
    public Vector2I position;
    public float height;
    public HashSet<Vector2I> voronoiOrigins;
    public List<ChunkVertex> connections = new();
    public bool has3Connections = false;
    public bool connectionsHave3Connections = false;
    public bool isLocalMinimum = false;
    public bool isLocalMaximum = false;
    public bool hasLake = false;
    public Vector2 flowDirection = Vector2.Zero;
    public ChunkVertex(Vector2I _position, float _height, HashSet<Vector2I> _voronoiOrigins)
    {
        position = _position;
        height = _height;
        voronoiOrigins = _voronoiOrigins;
    }

    public void SetupConnections(){
        if(connections.Count == 3)
        {
            has3Connections = true;
            Span<bool> connectionsIsHigher = stackalloc bool[3];
            connectionsHave3Connections = true;
            int numLower = 0;
            int numHigher = 0;
            for(int i = 0; i < connections.Count; i++)
            {
                ChunkVertex connectedVertex = connections[i];
                if(connectedVertex.height < height)
                {
                    connectionsIsHigher[i] = false;
                    numLower++;
                }
                else
                {
                    connectionsIsHigher[i] = true;
                    numHigher++;
                }
                if(connectedVertex.connections.Count != 3)
                {
                    connectionsHave3Connections = false;
                }
            }
            if(numLower == 3)
            {
                isLocalMaximum = true;
                hasLake = true;
            }
            else if(numHigher == 3)
            {
                isLocalMinimum = true;
                hasLake = true;
            }
            else
            {
                for(int i = 0; i < 3; i++)
                {
                    if(!connectionsIsHigher[i])
                    {
                        flowDirection += ((Vector2)(connections[i].position - position)).Normalized();
                    }
                }
                flowDirection = flowDirection.Normalized();
            }
        }
    }
}