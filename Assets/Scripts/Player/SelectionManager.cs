using ScriptableObjectArchitecture;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using HexagonTilemapEditor;
using UnityEditor;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    // SERIALIZED
    [Title("Dependencies")]
    [SerializeField] [Required]
    private Camera _camera;

    [SerializeField] [Required]
    private RealtimeMapGenerator _realtimeMapGenerator;

    [SerializeField] [Required]
    private Grid _grid;

    [SerializeField] [Required]
    private HexTileGameEvent onTileSelected;

    [SerializeField] [ReadOnly]
    private HexTile _selectedHex;

    [SerializeField] [ReadOnly]
    private HexTile _secondSelectedHex;

    // PRIVATE
    private Vector3 _mousePosition;
    private Vector3Int _tileUnderMouseCords;
    private HexTile _selectedTemp;

    private Pathfinding _pathfinding;
    private List<PathNode> _pathToSecondSelected = new();

    private List<PathNode> _selectedNeighbors = new();
    private List<PathNode> _secondSelectedNeighbors = new();

    // EVENT

    private void Start()
    {
        _pathfinding = new Pathfinding(_realtimeMapGenerator);
    }

    private void Update()
    {
        // Deselect all during moving camera and remove path
        HandleCancelSelection();

        HandleSelection();
    }

    private void DebugNeighbors()
    {
        foreach (PathNode node in _selectedNeighbors)
            Gizmos.DrawWireSphere(node.HexTile.transform.position, .25f);
        foreach (PathNode node in _secondSelectedNeighbors)
            Gizmos.DrawWireSphere(node.HexTile.transform.position, .25f);
    }

    private void HandleCancelSelection()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ClearPathDebug();
            _selectedHex = null;
            _secondSelectedHex = null;
            _pathToSecondSelected.Clear();
        }
    }

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ClearPathDebug();

            if (!_selectedHex)
            {
                _selectedHex = TrySelect();

                onTileSelected.Raise(_selectedHex);

                // Deselect second selection if selected is null
                if (_selectedHex == null)
                {
                    _secondSelectedHex = null;
                }
                else
                    _selectedNeighbors = GetNeighbors(_selectedHex.PathNode, HexTile.OnScreenTiles);
            }
            else
            {
                _secondSelectedHex = TrySelect();

                // Deselect after clicking the same tile
                if (_secondSelectedHex == _selectedHex) _secondSelectedHex = null;

                onTileSelected.Raise(_secondSelectedHex);

                // Deselect also first selection is selected is null
                if (_secondSelectedHex == null)
                    _selectedHex = null;
                else
                {
                    _secondSelectedNeighbors =
                        GetNeighbors(_secondSelectedHex.PathNode, HexTile.OnScreenTiles);

                    // If selected yellow tiles (first and last)
                    if (_secondSelectedHex.ID == 2 && _selectedHex.ID == 2)
                    {
                        // Find path
                        _pathToSecondSelected = _pathfinding
                            .FindPathOnScreen(_selectedHex.Coordinates, _secondSelectedHex.Coordinates);
                        DebugPath();
                    }
                }
            }
        }
    }


    // PRIVATE
    void DebugPath() => _pathToSecondSelected.ForEach(p => p.HexTile.DebugPath());
    void ClearPathDebug() => _pathToSecondSelected.ForEach(p => p.HexTile.ClearPathDebug());

    private HexTile TrySelect()
    {
        _mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        _mousePosition = new Vector3(_mousePosition.x, _mousePosition.y, 0);
        _tileUnderMouseCords = _grid.WorldToCell(_mousePosition);
        _selectedTemp = _realtimeMapGenerator.GetTile(new Vector2Int(_tileUnderMouseCords.y, _tileUnderMouseCords.x));
        return _selectedTemp != null && _selectedTemp.Interactable ? _selectedTemp : null;
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
                result.Add(hexes[pathNode.HexTile.Coordinates + diff].PathNode);
        // Is even column
        else
            foreach (Vector2Int diff in EVEN_COLS_NEIGHBOR_DIFFERENCES)
                result.Add(hexes[pathNode.HexTile.Coordinates + diff].PathNode);


        return result;
    }

    private void OnDrawGizmos()
    {
        DebugNeighbors();

        if (!EditorApplication.isPlaying
            || _pathToSecondSelected.Count == 0) return;

        Gizmos.color = Color.green;
        Vector3 firstPos = _pathToSecondSelected[0].HexTile.transform.position;
        for (int i = 1; i < _pathToSecondSelected.Count; i++)
        {
            Vector3 secondPos = _pathToSecondSelected[i].HexTile.transform.position;
            Gizmos.DrawLine(firstPos, secondPos);
            firstPos = secondPos;
        }
    }
}