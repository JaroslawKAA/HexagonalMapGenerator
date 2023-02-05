using System.Collections.Generic;
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
        public Vector2Int Coordinates => coordinates;
        public string[] Stats => new[]
        {
            $"Prefab Id: {id}",
            $"Coordinates: ({coordinates.x},{coordinates.y})",
            $"Interactable: {interactable}",
            $"Sprite: {SpriteRenderer.sprite.name}"
        };
        public PathNode PathNode { get; private set; }
        public static Dictionary<Vector2Int, HexTile> OnScreenTiles => _onScreenTiles;
        public bool IsWalkable { get; private set; }

        // PRIVATE
        [ShowInInspector]
        private static Dictionary<Vector2Int, HexTile> _onScreenTiles = new();

        // PUBLIC
        public void Init(int id, Vector2Int coordinates, bool interactable, bool isWalkable,
            bool debugCoords, bool debugPathFinding, RealtimeMapGenerator realtimeMapGenerator)
        {
            this.id = id;
            this.coordinates = coordinates;
            this.interactable = interactable;
            this.IsWalkable = isWalkable;

            this.PathNode = new PathNode(realtimeMapGenerator, this);

            if (debugCoords)
            {
                _debugCanvas.SetActive(true);

                _coordsText.enabled = debugCoords;
                _coordsText.text = $"({coordinates.x},{coordinates.y})";
            }
        }

        public void DebugPath()
        {
            _debugCanvas.SetActive(true);

            _fText.enabled = true;
            _gText.enabled = true;
            _hText.enabled = true;

            _fText.text = PathNode.fCost.ToString();
            _gText.text = PathNode.gCost.ToString();
            _hText.text = PathNode.hCost.ToString();
        }
        
        public void ClearPathDebug()
        {
            _debugCanvas.SetActive(false);

            _fText.enabled = false;
            _gText.enabled = false;
            _hText.enabled = false;
        }

        // EVENT
        private void OnBecameVisible()
        {
            _onScreenTiles[this.Coordinates] = this;
        }

        private void OnBecameInvisible()
        {
            _onScreenTiles.Remove(this.Coordinates);
        }
    }
}