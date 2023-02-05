using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using System;
using HexagonTilemapEditor;

public class Hunk : SerializedMonoBehaviour
{
    [ShowInInspector] [ReadOnly] [DictionaryDrawerSettings]
    private Dictionary<Vector2Int, HexTile> hexagonalTiles = new Dictionary<Vector2Int, HexTile>();

    
    // PROPERTIES
    public string AddresablePath { get; set; }
    public AsyncOperationHandle<HunkSO> AddresableOperation { get; set; }
    public IEnumerator PopulatingCoroutine { get; set; }
    public Action onDisabled { get; set; }
    public MinMaxRange MinMaxRange { get; set; }

    // PRIVATE

    private bool _isDestroying = false;

    public Dictionary<Vector2Int, HexTile> HexagonalTiles => hexagonalTiles;

    public void RegisterTile(Vector2Int key, HexTile hexTile)
    {
        if (!_isDestroying)
            hexagonalTiles[key] = hexTile;
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