using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Utils;

namespace HexagonTilemapEditor
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Level/Hunk", fileName = "HunkSO", order = 0)]
    public class HunkSO : SerializedScriptableObject
    {
        // SERIALIZED
        [SerializeField] 
        [DictionaryDrawerSettings(IsReadOnly = true, KeyLabel = "coords", ValueLabel = "hex", DisplayMode = DictionaryDisplayOptions.OneLine)]
        private Dictionary<Vector2Int, HexData> _hexes = new Dictionary<Vector2Int, HexData>();

        [SerializeField] [ReadOnly] private Vector2Int minCoords;
        [SerializeField] [ReadOnly] private Vector2Int maxCoords;

        // PRIVATE
        private bool inited = false;

        // PROPERTIES
        public Vector2Int MinCoords
        {
            get => minCoords;
            set => minCoords = value;
        }

        public Vector2Int MaxCoords
        {
            get => maxCoords;
            set => maxCoords = value;
        }

        public Dictionary<Vector2Int, HexData> Hexes => _hexes;
        public MinMaxRange MinMaxRange => new(minCoords.x, minCoords.y, maxCoords.x, maxCoords.y);

        // PUBLIC
        public void AddHex(HexData hex)
        {
            _hexes.Add(hex.coordinates, hex);
            UpdateRange(hex.coordinates);
        
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    
        // PRIVATE
        private void UpdateRange(Vector2Int newCoords)
        {
            if (!inited)
            {
                minCoords = newCoords;
                maxCoords = newCoords;

                inited = true;
            }

            minCoords.x = Mathf.Min(minCoords.x, newCoords.x);
            minCoords.y = Mathf.Min(minCoords.y, newCoords.y);
            maxCoords.x = Mathf.Max(maxCoords.x, newCoords.x);
            maxCoords.y = Mathf.Max(maxCoords.y, newCoords.y);
        }
    }
}