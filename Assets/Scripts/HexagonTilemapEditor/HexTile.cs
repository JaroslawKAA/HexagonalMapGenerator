using Sirenix.OdinInspector;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    [Title("Hex Tile")]
    [SerializeField] private Vector2Int coordinates;
    [SerializeField] private HexagonTilemapEditorSO.HexTileSetup.Color color;
    [SerializeField] private bool interactable;

    [SerializeField] [Required]
    private SpriteRenderer spriteRenderer;

    public SpriteRenderer SpriteRenderer => spriteRenderer;

    public void Init(
        Vector2Int coordinates, 
        HexagonTilemapEditorSO.HexTileSetup.Color color,
        bool interactable)
    {
        this.coordinates = coordinates;
        this.color = color;
        this.interactable = interactable;
    }
}
