using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Tilemaps;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MapGenerator : MonoBehaviour
{
    [Title("Dependencies")]
    [SerializeField] [Required]
    private HunksManagerSo _hunksManager;

    [SerializeField] [Required]
    private Grid _grid;

    [SerializeField] [Required]
    private HexesPool _hexesPool;

    [SerializeField] [Required]
    private HexagonTilemapEditorSO _mapConfig;

    [SerializeField]
    private List<Hunk> _hunks = new();

    [SerializeField]
    private List<string> _hunksPaths = new();

    private List<string> _hunksPathsToRemove = new();

    [SerializeField]
    private Camera _camera;

    private Action<string> onPathAdded;
    private Action<string> onPathRemoved;


    private void TryAddPath(string path)
    {
        if (_hunksPaths.All(p => p != path))
        {
            _hunksPaths.Add(path);
            onPathAdded?.Invoke(path);
            Debug.Log($"On path Added: {path}");
        }
    }

    private void RemovePath(string path)
    {
        _hunksPaths.Remove(_hunksPaths[_hunksPaths.IndexOf(path)]);
        onPathRemoved?.Invoke(path);

        Debug.Log($"On path Removed: {path}");
    }

    // Update is called once per frame
    private void Start()
    {
        _camera = Camera.main;

        onPathAdded += path =>
        {
            Addressables.LoadAssetAsync<HunkSO>(path).Completed += SpawnHunk;
        };

        onPathRemoved += path =>
        {
            Hunk toDestroy = _hunks.First(h => path == h.AddresablePath);
            Addressables.Release(toDestroy.AddresableOperation);
            _hunks.Remove(toDestroy);

            StartCoroutine(KickHexesToPool(toDestroy.HexagonalTiles.Values.ToArray()));
            
            toDestroy.CustomDestroy();
        };

        InvokeRepeating(nameof(UpdateAssetsToLoad), 0f, .5f);
    }

    private IEnumerator KickHexesToPool(HexTile[] hexTiles)
    {
        int workConter = 0;
        foreach (HexTile tile in hexTiles)
        {
            tile.gameObject.SetActive(false);
            workConter++;
            if (workConter > _mapConfig.HunkPopulatingSpeed)
            {
                workConter = 0;
                yield return new WaitForEndOfFrame();
            }
        }
    }


    private Vector2Int _currentHunkPoint;

    void UpdateAssetsToLoad()
    {
        Vector3Int cameraCell = _grid.WorldToCell(_camera.transform.position);
        _currentHunkPoint = new Vector2Int(cameraCell.y, cameraCell.x);

        List<string> newPaths = _hunksManager.TryGetCurrentSet(_currentHunkPoint);

        // Remove not returned paths
        _hunksPathsToRemove = _hunksPaths.Except(newPaths).ToList();
        newPaths.ForEach(TryAddPath);
        _hunksPathsToRemove.ForEach(RemovePath);
    }

    private void SpawnHunk(AsyncOperationHandle<HunkSO> asyncOperation)
    {
        GameObject hunkGO = new GameObject(
            name: $"Hunk_{asyncOperation.Result.MinCoords.x}_{asyncOperation.Result.MinCoords.y}" +
                  $"_{asyncOperation.Result.MaxCoords.x}_{asyncOperation.Result.MaxCoords.y}",
            components: typeof(Hunk));
        hunkGO.transform.parent = transform;

        Hunk hunk = hunkGO.GetComponent<Hunk>();
        hunk.AddresablePath = AssetDatabase.GetAssetPath(asyncOperation.Result);
        hunk.AddresableOperation = asyncOperation;
        hunk.PopulatingCoroutine = PopulateHunk(asyncOperation.Result, hunkGO);
        StartCoroutine(hunk.PopulatingCoroutine);

        Debug.Log($"Spawned: {hunkGO.name}");

        _hunks.Add(hunk);
    }

    private IEnumerator PopulateHunk(HunkSO hunkSO, GameObject hunkGO)
    {
        // Stop generating tiles if hunk is disabling
        bool hunkDisabled = false;
        Hunk hunk = hunkGO.GetComponent<Hunk>();
        hunk.onDisabled += () => hunkDisabled = true;

        // Count how lot of object populated per frame
        int packageCounter = 0;

        for (int i = hunkSO.MinCoords.y; i <= hunkSO.MaxCoords.y; i++)
        {
            if (hunkDisabled) break;

            for (int j = hunkSO.MinCoords.x; j <= hunkSO.MaxCoords.x; j++)
            {
                if (hunkDisabled) break;

                HexTile hexTile = GenerateTile(hunkSO.Hexes[new Vector2Int(j, i)]);

                hunkGO.GetComponent<Hunk>().RegisterTile(
                    key: new Vector2Int(j, i),
                    hexTile: hexTile);

                packageCounter++;
                if (packageCounter > _mapConfig.HunkPopulatingSpeed)
                {
                    packageCounter = 0;
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        StopCoroutine(hunk.PopulatingCoroutine);
    }

    private HexTile GenerateTile(HunkSO.HexData hexData)
    {
        HexTile hexTile =_hexesPool.GetPulledObject();

        var config = _mapConfig.TilesConfiguration
            .First(c => c.id == hexData.prefabId);
        hexTile.SpriteRenderer.sprite = config.sprite;

        hexTile.Init(
            id: config.id,
            coordinates: new Vector2Int(hexData.coordinates.x, hexData.coordinates.y),
            interactable: config.interactable);

        hexTile.transform.position = _grid.GetCellCenterWorld(
            new Vector3Int(
                hexData.coordinates.y,
                hexData.coordinates.x,
                0));

        return hexTile;
    }

    private Vector2Int GetHunksCountToGenerate()
    {
        int xHunksCount = (_mapConfig.TilemapSize.x / _mapConfig.HunkSize.x);
        if (_mapConfig.TilemapSize.x % _mapConfig.HunkSize.x != 0) xHunksCount++;
        int yHunksCount = (_mapConfig.TilemapSize.y / _mapConfig.HunkSize.y);
        if (_mapConfig.TilemapSize.y % _mapConfig.HunkSize.y != 0) yHunksCount++;

        int hunksToGenerate = xHunksCount * yHunksCount;
        Debug.Log("All hunks to generate:" + hunksToGenerate);
        Debug.Log($"Hunks count to generate (XY): {xHunksCount}, {yHunksCount}");
        return new Vector2Int(xHunksCount, yHunksCount);
    }
}