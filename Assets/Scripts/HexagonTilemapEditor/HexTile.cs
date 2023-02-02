using Sirenix.OdinInspector;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    [Title("Hex Tile")]
    [SerializeField] private Vector2Int coordinates;
    [SerializeField] private bool interactable;
    [SerializeField] private int id;

    [SerializeField] [Required]
    private SpriteRenderer spriteRenderer;

    public SpriteRenderer SpriteRenderer => spriteRenderer;

    public void Init(
        int id,
        Vector2Int coordinates, 
        bool interactable)
    {
        this.id = id;
        this.coordinates = coordinates;
        this.interactable = interactable;
    }
}
