using System;
using HexagonTilemapEditor;
using UnityEngine;

[Serializable]
public class PathNode
{
    [SerializeField]
    private RealtimeMapGenerator _realtimeMapGenerator;

    [SerializeField]
    private HexTile _hexTile;

    public int gCost;
    public int hCost;
    public int fCost;

    public PathNode cameFromNode;

    public HexTile HexTile => _hexTile;

    public PathNode(RealtimeMapGenerator realtimeMapGenerator, HexTile hexTile)
    {
        _realtimeMapGenerator = realtimeMapGenerator;
        _hexTile = hexTile;
    }
    
    public override string ToString() => $"{_hexTile.Coordinates.x},{_hexTile.Coordinates.y}";

    public void CalculateFCost() => fCost = gCost + hCost;
}
