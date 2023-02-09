using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HexagonTilemapEditor
{
    /// <summary>
    /// Spawning hex tiles based on current level data.
    /// Level data is read partly with Addressables.
    /// </summary>
    public class RuntimeMapLoader : MonoBehaviour
    {
        // SERIALIZED
        [Title("Config")]
        [SerializeField]
        private bool _debugCoords = true;

        [Title("Dependencies")]
        [SerializeField] [Required]
        private HunksManagerSo _hunksManager;

        [SerializeField] [Required]
        private Grid _grid;

        [SerializeField] [Required]
        private HexesPool _hexesPool;

        [SerializeField] [Required]
        private HexagonTilemapEditorSO _mapConfig;

        // PRIVATE
        private readonly List<Hunk> _visibleHunks = new();
        private readonly List<string> _visibleHunksPaths = new();
        private List<string> _hunksPathsToRemove = new();

        private UnityEngine.Camera _camera;

        // Events
        private event Action<string> onPathAdded;
        private event Action<string> onPathRemoved;

        // PROPERTIES
        public Grid Grid => _grid;

        // PUBLIC
        public HexTile GetTile(Vector2Int tileCoords)
        {
            Hunk hunk = _visibleHunks.FirstOrDefault(h => h.MinMaxRange.IsPointInRange(tileCoords)); // TODO Refactor
            if (!hunk) return default;
            return hunk.GetTile(tileCoords);
        }

        // EVENT
        private void Start()
        {
            _camera = UnityEngine.Camera.main;

            onPathAdded += path => { Addressables.LoadAssetAsync<HunkSO>(path).Completed += LoadHunk; };

            onPathRemoved += path =>
            {
                Hunk toDestroy = _visibleHunks.First(h => path == h.AddressablePath);
                Addressables.Release(toDestroy.AddressableOperation);
                _visibleHunks.Remove(toDestroy);

                StartCoroutine(KickHexesToPool(toDestroy.HexagonalTiles.Values.ToArray()));

                toDestroy.CustomDestroy();
            };

            InvokeRepeating(nameof(UpdateAssetsToLoad), 0f, 1f);
        }

        // PRIVATE
        private void RemovePath(string path)
        {
            _visibleHunksPaths.Remove(_visibleHunksPaths[_visibleHunksPaths.IndexOf(path)]);
            onPathRemoved?.Invoke(path);
        
            Debug.Log($"On path Removed: {path}");
        }

        /// <summary>
        /// Add path to visible hunks list.
        /// </summary>
        /// <param name="path"></param>
        private void TryAddPath(string path)
        {
            if (_visibleHunksPaths.All(p => p != path))
            {
                _visibleHunksPaths.Add(path);
                onPathAdded?.Invoke(path);
                Debug.Log($"On path Added: {path}");
            }
        }


        private Vector2Int _currentHunkPoint;

        /// <summary>
        /// Get current visible hunk paths.
        /// </summary>
        private void UpdateAssetsToLoad()
        {
            Vector3Int cameraCell = _grid.WorldToCell(_camera.transform.position);
            _currentHunkPoint = new Vector2Int(cameraCell.y, cameraCell.x);

            List<string> newAddressablePaths = _hunksManager.TryGetCurrentSet(_currentHunkPoint);

            // Remove not returned paths
            _hunksPathsToRemove = _visibleHunksPaths.Except(newAddressablePaths).ToList();
            newAddressablePaths.ForEach(TryAddPath);
            _hunksPathsToRemove.ForEach(RemovePath);
        }

        private IEnumerator KickHexesToPool(HexTile[] hexTiles)
        {
            int workCounter = 0;
            foreach (HexTile tile in hexTiles)
            {
                tile.gameObject.SetActive(false);
                workCounter++;
                if (workCounter > _mapConfig.HunkPopulatingSpeed)
                {
                    workCounter = 0;
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        private void LoadHunk(AsyncOperationHandle<HunkSO> asyncOperation)
        {
            GameObject hunkGO = new GameObject(
                name: $"Hunk_{asyncOperation.Result.MinCoords.x}_{asyncOperation.Result.MinCoords.y}" +
                      $"_{asyncOperation.Result.MaxCoords.x}_{asyncOperation.Result.MaxCoords.y}",
                components: typeof(Hunk));
            hunkGO.transform.parent = transform;

            Hunk hunk = hunkGO.GetComponent<Hunk>();
            hunk.AddressablePath = AssetDatabase.GetAssetPath(asyncOperation.Result);
            hunk.AddressableOperation = asyncOperation;
            hunk.MinMaxRange = asyncOperation.Result.MinMaxRange;
            hunk.PopulatingCoroutine = PopulateHunk(asyncOperation.Result, hunkGO);
            StartCoroutine(hunk.PopulatingCoroutine);

            Debug.Log($"Spawned: {hunkGO.name}");

            _visibleHunks.Add(hunk);
        }

        /// <summary>
        /// Fill hunk by tiles.
        /// </summary>
        /// <param name="hunkSO"></param>
        /// <param name="hunkGO"></param>
        /// <returns></returns>
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

                    HexTile hexTile = LoadTile(hunkSO.Hexes[new Vector2Int(j, i)]);

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

        private HexTile LoadTile(HexData hexData)
        {
            HexTile hexTile =  _hexesPool.GetPulledObject().GetComponent<HexTile>();
        

            var config = _mapConfig.TilesConfiguration.Configurations
                .First(c => c.id == hexData.prefabId);
            hexTile.SpriteRenderer.sprite = config.sprite;

            hexTile.Init(
                id: config.id,
                coordinates: new Vector2Int(hexData.coordinates.x, hexData.coordinates.y),
                interactable: config.interactable,
                isWalkable: config.walkable,
                debugCoords: _debugCoords,
                this);

            hexTile.transform.position = _grid.GetCellCenterWorld(
                new Vector3Int(
                    hexData.coordinates.y,
                    hexData.coordinates.x,
                    0));

            return hexTile;
        }
    }
}