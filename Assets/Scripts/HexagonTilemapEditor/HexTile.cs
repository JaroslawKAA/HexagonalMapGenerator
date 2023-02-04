using Sirenix.OdinInspector;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    [Title("Hex Tile")]
    [SerializeField] private Vector2Int coordinates;
    [SerializeField] private bool interactable;
    [SerializeField] private int id;

    [Title("Dependencies")]
    [SerializeField] [Required]
    private SpriteRenderer spriteRenderer;
    
    [SerializeField] [Required]
    private PolygonCollider2D _collider2D;
    
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public PolygonCollider2D Collider2D => _collider2D;


    public void Init(
        int id,
        Vector2Int coordinates, 
        bool interactable)
    {
        this.id = id;
        this.coordinates = coordinates;
        this.interactable = interactable;
        if (!interactable)
            _collider2D.enabled = false;
    }
}
