using System.Collections.Generic;
using HexagonTilemapEditor;
using UnityEngine;

public class Pathfinding
{
    private readonly RealtimeMapGenerator _realtimeMapGenerator;

    private const int MOVE_STRAIGHT_COST = 10;

    private List<PathNode> _openList;
    private List<PathNode> _closedList;


    public Pathfinding(RealtimeMapGenerator realtimeMapGenerator)
    {
        _realtimeMapGenerator = realtimeMapGenerator;
    }

    public List<PathNode> FindPathOnScreen(Vector2Int start, Vector2Int end)
    {
        PathNode startNode = _realtimeMapGenerator.GetTile(start).PathNode;
        PathNode endNode = _realtimeMapGenerator.GetTile(end).PathNode;

        _openList = new() { startNode };
        _closedList = new();

        Dictionary<Vector2Int, HexTile> hexTiles = HexTile.OnScreenTiles;
        foreach (HexTile tile in hexTiles.Values)
        {
            tile.PathNode.gCost = int.MaxValue;
            tile.PathNode.CalculateFCost();
            tile.PathNode.cameFromNode = null;
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateHeuristicDistanceCost(start, end);
        startNode.CalculateFCost();

        while (_openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(_openList);
            if (currentNode == endNode)
            {
                //Reached final node
                return CalculatePath(endNode);
            }

            _openList.Remove(currentNode);
            _closedList.Add(currentNode);

            foreach (PathNode neighborNode in GetNeighbors(currentNode, hexTiles))
            {
                if (_closedList.Contains(neighborNode)) continue;

                if(neighborNode.HexTile.PathNode == endNode)
                {
                    endNode.cameFromNode = currentNode;
                    endNode.hCost = 0;
                    endNode.gCost = 0;
                    endNode.CalculateFCost();
                    return CalculatePath(endNode);
                }
                
                if (!neighborNode.HexTile.IsWalkable)
                {
                    _closedList.Add(neighborNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateHeuristicDistanceCost(
                    currentNode.HexTile.Coordinates, neighborNode.HexTile.Coordinates);
                if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.cameFromNode = currentNode;
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = CalculateHeuristicDistanceCost(
                        neighborNode.HexTile.Coordinates,
                        endNode.HexTile.Coordinates);
                    neighborNode.CalculateFCost();

                    if (!_openList.Contains(neighborNode))
                        _openList.Add(neighborNode);
                }
            }
        }

        // Out of nodes on the openList
        return new List<PathNode>();
    }

    private readonly Vector2Int[] EVEN_COLS_NEIGHBOR_DIFFERENCES = new[]
    {
        new Vector2Int(1, 1), new Vector2Int(1, 0),
        new Vector2Int(0, -1), new Vector2Int(-1, 0),
        new Vector2Int(-1, 1), new Vector2Int(0, 1),
    };

    private readonly Vector2Int[] ODD_COLS_NEIGHBOR_DIFFERENCES = new[]
    {
        new Vector2Int(1, 0), new Vector2Int(1, -1),
        new Vector2Int(0, -1), new Vector2Int(-1, -1),
        new Vector2Int(-1, 0), new Vector2Int(0, 1),
    };

    private List<PathNode> GetNeighbors(PathNode pathNode, Dictionary<Vector2Int, HexTile> hexes)
    {
        List<PathNode> result = new List<PathNode>();

        // Is odd column
        if (pathNode.HexTile.Coordinates.x % 2 == 0)
            foreach (Vector2Int diff in ODD_COLS_NEIGHBOR_DIFFERENCES)
            {
                if (hexes.ContainsKey(pathNode.HexTile.Coordinates + diff))
                    result.Add(hexes[pathNode.HexTile.Coordinates + diff].PathNode);
            }
        // Is even column
        else
            foreach (Vector2Int diff in EVEN_COLS_NEIGHBOR_DIFFERENCES)
            {
                if (hexes.ContainsKey(pathNode.HexTile.Coordinates + diff))
                    result.Add(hexes[pathNode.HexTile.Coordinates + diff].PathNode);
            }


        return result;
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }

        path.Reverse();
        return path;
    }

    private int CalculateHeuristicDistanceCost(Vector2Int a, Vector2Int b) =>
        Mathf.RoundToInt(MOVE_STRAIGHT_COST *
                         Vector3.Distance(
                             _realtimeMapGenerator.Grid.CellToWorld((Vector3Int)a),
                             _realtimeMapGenerator.Grid.CellToWorld((Vector3Int)b)));

    private PathNode GetLowestFCostNode(List<PathNode> pathNodes)
    {
        PathNode lowesFCostNode = pathNodes[0];
        for (int i = 1; i < pathNodes.Count; i++)
        {
            if (pathNodes[i].fCost < lowesFCostNode.fCost)
                lowesFCostNode = pathNodes[i];
        }

        return lowesFCostNode;
    }
}