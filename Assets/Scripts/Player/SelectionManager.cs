using HexagonTilemapEditor;
using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;
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

    // EVENT
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!_selectedHex)
            {
                _selectedHex = TrySelect();
            
                onTileSelected.Raise(_selectedHex);

                // Deselect also second selection is selected is null
                if (_selectedHex == null) _secondSelectedHex = null;
            }
            else
            {
                _secondSelectedHex = TrySelect();
                
                // Deselect after clicking the same tile
                if (_secondSelectedHex == _selectedHex) _secondSelectedHex = null;

                onTileSelected.Raise(_secondSelectedHex);

                // Deselect also first selection is selected is null
                if (_secondSelectedHex == null) _selectedHex = null;
            }
        }
    }

    // PRIVATE
    private HexTile TrySelect()
    {
        _mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        _mousePosition = new Vector3(_mousePosition.x, _mousePosition.y, 0);
        _tileUnderMouseCords = _grid.WorldToCell(_mousePosition);
        _selectedTemp = _realtimeMapGenerator.GetTile(new Vector2Int(_tileUnderMouseCords.y, _tileUnderMouseCords.x));
        return _selectedTemp != null && _selectedTemp.Interactable ? _selectedTemp : null;
    }
}