using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    [Title("Depend")]
    [SerializeField] [Required]
    private RealtimeMapGenerator _realtimeMapGenerator;

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private List<PathNode> _openList;
    private List<PathNode> _closedList;


    private List<PathNode> FindPath(Vector2Int start, Vector2Int end)
    {
        PathNode startNode = _realtimeMapGenerator.GetTile(start).PathNode;
        PathNode endNode = _realtimeMapGenerator.GetTile(end).PathNode;

        _openList = new() { startNode };
        _closedList = new();

        Hunk currentHunk = _realtimeMapGenerator.CurrentHunk;
        for (int i = 0; i < currentHunk.Height; i++)
        {
            for (int j = 0; j < currentHunk.Width; j++)
            {
                PathNode pathNode = currentHunk.GetTile(new Vector2Int(j, i)).PathNode;
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(start, end);
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

            foreach (PathNode neighborNode in GetNeighbors(currentNode, currentHunk))
            {
                if (_closedList.Contains(neighborNode)) continue;

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(
                    currentNode.HexTile.Coordinates, neighborNode.HexTile.Coordinates);
                if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.cameFromNode = currentNode;
                }

            }
        }
    }

    private readonly Vector2Int[] EVEN_COLS_NEIGHBOR_DIFFERENCES = new[]
    {
        new Vector2Int(1, 0), new Vector2Int(1, -1),
        new Vector2Int(0, -1), new Vector2Int(-1, -1),
        new Vector2Int(-1, 0), new Vector2Int(0, 1),
    };

    private readonly Vector2Int[] ODD_COLS_NEIGHBOR_DIFFERENCES = new[]
    {
        new Vector2Int(1, 1), new Vector2Int(1, 0),
        new Vector2Int(0, -1), new Vector2Int(-1, 0),
        new Vector2Int(-1, 1), new Vector2Int(0, 1),
    };

    private List<PathNode> GetNeighbors(PathNode pathNode, Hunk currentHunk)
    {
        List<PathNode> result = new List<PathNode>();

        // Is odd column
        if (pathNode.HexTile.Coordinates.x % 2 == 0)
            foreach (Vector2Int diff in ODD_COLS_NEIGHBOR_DIFFERENCES)
                result.Add(currentHunk.GetTile(pathNode.HexTile.Coordinates + diff).PathNode);
        // Is even column
        else
            foreach (Vector2Int diff in EVEN_COLS_NEIGHBOR_DIFFERENCES)
                result.Add(currentHunk.GetTile(pathNode.HexTile.Coordinates + diff).PathNode);

        return result;
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        return null;
    }

    private int CalculateDistanceCost(Vector2Int a, Vector2Int b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST + Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

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