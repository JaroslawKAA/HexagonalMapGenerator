using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System;
using UnityEditor;

[CreateAssetMenu(menuName = "Scriptable Objects/Level/Hunk", fileName = "HunkSO", order = 0)]
public class HunkSO : SerializedScriptableObject
{
    [SerializeField] 
    [DictionaryDrawerSettings(IsReadOnly = true, KeyLabel = "coords", ValueLabel = "hex", DisplayMode = DictionaryDisplayOptions.OneLine)]
    private Dictionary<Vector2Int, HexData> _hexes = new Dictionary<Vector2Int, HexData>();

    [SerializeField] [ReadOnly]
    private Vector2Int minCoords;

    [SerializeField] [ReadOnly]
    private Vector2Int maxCoords;
    
    [SerializeField] [ReadOnly] 
    private bool inited = false;

    public Vector2Int MinCoords => minCoords;
    public Vector2Int MaxCoords => maxCoords;

    public void AddHex(HexData hex)
    {
        _hexes.Add(hex.coordinates, hex);
        UpdateRange(hex.coordinates);
        
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
    
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
    
    [Serializable]
    [HideReferenceObjectPicker]
    public class HexData
    {
        [ReadOnly]
        public int prefabId;
        
        [ReadOnly]
        public Vector2Int coordinates;

        public HexData(int prefabId, Vector2Int coordinates)
        {
            this.prefabId = prefabId;
            this.coordinates = coordinates;
        }
    }
}