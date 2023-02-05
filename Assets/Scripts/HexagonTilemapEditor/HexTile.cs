using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace HexagonTilemapEditor
{
    public class HexTile : MonoBehaviour
    {
        // SERIALIZED
        [Title("Hex Tile")]
        [SerializeField] private Vector2Int coordinates;

        [SerializeField] private bool interactable;
        [SerializeField] private int id;

        [Title("Dependencies")]
        [SerializeField] [Required]
        private SpriteRenderer spriteRenderer;

        [SerializeField] [Required]
        private GameObject _debugCanvas;

        [SerializeField] [Required]
        private TMP_Text _coordsText;

        [SerializeField] [Required]
        private TMP_Text _fText;

        [SerializeField] [Required]
        private TMP_Text _gText;

        [SerializeField] [Required]
        private TMP_Text _hText;


        // PROPERTIES
        public int ID => id;
        public SpriteRenderer SpriteRenderer => spriteRenderer;
        public bool Interactable => interactable;
        public string[] Stats => new[]
        {
            $"Prefab Id: {id}",
            $"Coordinates: ({coordinates.x},{coordinates.y})",
            $"Interactable: {interactable}",
            $"Sprite: {SpriteRenderer.sprite.name}"
        };

        // PUBLIC
        public void Init(int id, Vector2Int coordinates, bool interactable,
            bool debugCoords, bool debugPathFinding)
        {
            this.id = id;
            this.coordinates = coordinates;
            this.interactable = interactable;

            if (debugCoords)
            {
                _debugCanvas.SetActive(true);

                _coordsText.enabled = debugCoords;
                _coordsText.text = $"({coordinates.x},{coordinates.y})";
            }

            if (debugPathFinding)
            {
                _debugCanvas.SetActive(true);

                _fText.enabled = debugPathFinding;
                _gText.enabled = debugPathFinding;
                _hText.enabled = debugPathFinding;
            }
        }
    }
}