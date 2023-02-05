using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using System;
using HexagonTilemapEditor;

public class Hunk : SerializedMonoBehaviour
{
    // SERIALIZED
    [ShowInInspector] [ReadOnly] [DictionaryDrawerSettings]
    private Dictionary<Vector2Int, HexTile> hexagonalTiles = new Dictionary<Vector2Int, HexTile>();
    
    // PRIVATE

    private bool _isDestroying = false;


    // PROPERTIES
    public string AddresablePath { get; set; }
    public AsyncOperationHandle<HunkSO> AddresableOperation { get; set; }
    public IEnumerator PopulatingCoroutine { get; set; }
    public Action onDisabled { get; set; }
    public MinMaxRange MinMaxRange { get; set; }
    public Dictionary<Vector2Int, HexTile> HexagonalTiles => hexagonalTiles;
    public int Height { get; private set; } = 0;
    public int Width { get; private set; } = 0;
    
    public void RegisterTile(Vector2Int key, HexTile hexTile)
    {
        if (!_isDestroying)
        {
            hexagonalTiles[key] = hexTile;
            Height = Mathf.Max(Height, key.y + 1);
            Width = Mathf.Max(Width, key.x + 1);
        }
    }

    public void CustomDestroy()
    {
        _isDestroying = true;
        gameObject.name = gameObject.name + "TO_DESTROY";
        AddresablePath = "";
        
        Destroy(gameObject);
    }

    public HexTile GetTile(Vector2Int coords)
    {
        return hexagonalTiles[coords];
    }

    private void OnDisable()
    {
        onDisabled?.Invoke();
    }
}