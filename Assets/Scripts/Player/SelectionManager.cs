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

    // PRIVATE
    private Vector3 _mousePosition;
    private Vector3Int _tileUnderMouseCords;
    private HexTile _selectedTemp;

    // EVENT
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            _mousePosition = new Vector3(_mousePosition.x, _mousePosition.y, 0);
            _tileUnderMouseCords = _grid.WorldToCell(_mousePosition);
            _selectedTemp = _realtimeMapGenerator.GetTile(new Vector2Int(_tileUnderMouseCords.y, _tileUnderMouseCords.x));
            if(_selectedTemp) Debug.Log($"Selected temp: id {_selectedTemp.ID}");
            _selectedHex = _selectedTemp != null && _selectedTemp.Interactable ? _selectedTemp : null;
            Debug.Log($"Selected: {_selectedHex != null}");
        }
    }
}