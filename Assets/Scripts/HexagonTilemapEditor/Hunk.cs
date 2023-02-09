using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

namespace HexagonTilemapEditor
{
    public class Hunk : SerializedMonoBehaviour
    {
        // SERIALIZED
        [ShowInInspector] [SerializeField] [ReadOnly]
        private Dictionary<Vector2Int, HexTile> hexagonalTiles = new Dictionary<Vector2Int, HexTile>();

        // PRIVATE
        private bool _isDestroying = false;

        // PROPERTIES
        public string AddressablePath { get; set; }
        public AsyncOperationHandle<HunkSO> AddressableOperation { get; set; }
        public IEnumerator PopulatingCoroutine { get; set; }
        public MinMaxRange MinMaxRange { get; set; }
        public Dictionary<Vector2Int, HexTile> HexagonalTiles => hexagonalTiles;
        public Action onDisabled { get; set; }

        // PUBLIC
        public void RegisterTile(Vector2Int key, HexTile hexTile)
        {
            if (!_isDestroying) hexagonalTiles[key] = hexTile;
        }

        public void CustomDestroy()
        {
            _isDestroying = true;
            gameObject.name = gameObject.name + "TO_DESTROY";
            AddressablePath = "";

            Destroy(gameObject);
        }

        public HexTile GetTile(Vector2Int coords) => hexagonalTiles[coords];

        // EVENT
        private void OnDisable() => onDisabled?.Invoke();
    }
}