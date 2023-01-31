#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using File = UnityEngine.Windows.File;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Scriptable Objects/Hexagon Tilemap Editor/Config", fileName = "config", order = 0)]
public class HexagonTilemapEditorSO : SerializedScriptableObject
{
    [Title("Hexagon Tilemap Editor")]
    [Title("Tilemap size", HorizontalLine = false, Bold = false)]
    [SerializeField] [HideLabel]
    private Vector2Int tilemapSize = new Vector2Int(1000, 1000);

    [Header("Hunk size")]
    [SerializeField] [HideLabel]
    private Vector2Int hunkSize = new Vector2Int(100, 100);

    [SerializeField]
    private int startXIndex = 1;

    [SerializeField]
    private int startYIndex = 1;

    [SerializeField]
    [TableList(AlwaysExpanded = true)]
    [ValidateInput("HasAnyElements", defaultMessage: "Configuration should contain one element at last.")]
    [ValidateInput("ValidateFrequency")]
    [InfoBox("Sum of frequency should be equal 100%./n" +
             "Last member will be calculated automatically.")]
    private List<HexTileSetup> tilesConfiguration = new() { new HexTileSetup() };

    [SerializeField]
    [Required]
    [AssetsOnly]
    private HexTile hexTilePrefab;

    [Header("Dependencies")]
    [SerializeField]
    [SceneObjectsOnly]
    [Required]
    private Tilemap tilemap;

    [SerializeField]
    [SceneObjectsOnly]
    [Required]
    private Grid grid;

    [Button("Generate Tiles")]
    [DisableIf("@tilemap == null")]
    private void GenerateTilesButton()
    {
        EditorCoroutineUtility.StartCoroutine(GenerateHunks(), this);
    }

    private IEnumerator GenerateHunks()
    {
        OnClearTilesButton();

        // Describes how big will be hunk part
        Vector2Int hunksToGenerateCount = GetHunksCountToGenerate();
        yield return null;

        yield return new WaitForSeconds(0.5f);
        while (isDeletingHunks)
        {
            Debug.Log("Waiting for deleting existing hunks.");
            yield return new WaitForSeconds(0.2f);
        }

        int hunksCount = hunksToGenerateCount.x * hunksToGenerateCount.y;
        EditorUtility.DisplayProgressBar("Generating hunks", "Generating hunks", (float)0 / hunksCount);

        for (int i = 0; i < hunksToGenerateCount.x; i++)
        {
            for (int j = 0; j < hunksToGenerateCount.y; j++)
            {
                GenerateHunk(i, j);
                
                EditorUtility.DisplayProgressBar(
                    "Generating hunks",
                    "Generating hunks",
                    (float)((i + 1) * (j + 1)) / hunksCount);
                
                yield return null;
            }
        }
        
        EditorUtility.ClearProgressBar();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private void GenerateHunk(int xIndex, int yIndex)
    {
        // Describes how lot of specifics tiles will be generate.
        HexTileSetup[] configurations = GenerateConfigurations();

        GameObject hunkGO = new GameObject(
            name: $"Hunk_{xIndex}_{yIndex}",
            components: typeof(Hunk));
        hunkGO.transform.parent = tilemap.transform;
        Hunk hunk = hunkGO.GetComponent<Hunk>();

        int currentConfigIndex = 0;
        for (int i = 0; i < hunkSize.x; i++)
        {
            int globalTileIndexY = i + yIndex * hunkSize.y;
            if (globalTileIndexY >= tilemapSize.y) break;

            for (int j = 0; j < hunkSize.x; j++)
            {
                int globalTileIndexX = j + xIndex * hunkSize.x;
                if (globalTileIndexX >= tilemapSize.x) break;
                HexTile hexTile = GenerateTile(globalTileIndexX, globalTileIndexY, configurations,
                    ref currentConfigIndex);
                hexTile.transform.parent = hunkGO.transform;

                hunk.RegisterTile(
                    key:new Vector2Int(globalTileIndexX + startXIndex, globalTileIndexY + startYIndex), 
                    hexTile: hexTile);
            }
        }

        PrefabUtility.SaveAsPrefabAssetAndConnect(
            hunkGO,
            $"Assets/Prefabs/Hunks/{hunkGO.name}.prefab",
            InteractionMode.AutomatedAction);
        Debug.Log($"Saved hunk: {hunkGO.name}");
    }

    private HexTile GenerateTile(
        int globalTileIndexX,
        int globalTileIndexY,
        HexTileSetup[] configurations,
        ref int currentConfigIndex)
    {
        HexTile hexTile = Instantiate(hexTilePrefab);
        hexTile.gameObject.name = $"HexTile{globalTileIndexX + startXIndex}_{globalTileIndexY + startYIndex}";
        hexTile.SpriteRenderer.sprite = configurations[currentConfigIndex].sprite;

        hexTile.Init(
            coordinates: new Vector2Int(globalTileIndexX + startXIndex, globalTileIndexY + startYIndex),
            color: configurations[currentConfigIndex].color,
            interactable: configurations[currentConfigIndex].interactable);

        hexTile.transform.position = grid.GetCellCenterWorld(new Vector3Int(
            globalTileIndexY,
            globalTileIndexX,
            0));

        currentConfigIndex++;
        return hexTile;
    }

    private Vector2Int GetHunksCountToGenerate()
    {
        int xHunksCount = (tilemapSize.x / hunkSize.x);
        if (tilemapSize.x % hunkSize.x != 0) xHunksCount++;
        int yHunksCount = (tilemapSize.y / hunkSize.y);
        if (tilemapSize.y % hunkSize.y != 0) yHunksCount++;

        int hunksToGenerate = xHunksCount * yHunksCount;
        Debug.Log("All hunks to generate:" + hunksToGenerate);
        Debug.Log($"Hunks count to generate (XY): {xHunksCount}, {yHunksCount}");
        return new Vector2Int(xHunksCount, yHunksCount);
    }

    [Button("Clear tiles")]
    void OnClearTilesButton()
    {
        EditorCoroutineUtility.StartCoroutine(ClearTiles(), this);
    }

    private bool isDeletingHunks = false;
    private IEnumerator ClearTiles()
    {
        isDeletingHunks = true;

        int hunksCount = tilemap.transform.childCount;
        EditorUtility.DisplayProgressBar("Deleting hunks", "Deleting hunks", 0f / hunksCount);
        for (int i = hunksCount - 1; i >= 0; i--)
        {
            // Remove prefab
            GameObject prefab =
                PrefabUtility.GetCorrespondingObjectFromSource(tilemap.transform.GetChild(i).gameObject);
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);
            if (File.Exists(prefabPath))
                File.Delete(prefabPath);
            if (File.Exists(prefabPath + ".meta"))
                File.Delete(prefabPath + ".meta");
            AssetDatabase.Refresh();

            // Destroy game object
            DestroyImmediate(tilemap.transform.GetChild(i).gameObject);

            EditorUtility.DisplayProgressBar("Deleting hunks", "Deleting hunks", (float)i / hunksCount);
            yield return null;
        }

        isDeletingHunks = false;
        EditorUtility.ClearProgressBar();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    /// <summary>
    /// Generate configurations for each tile by tilesConfiguration setup.
    /// E. g. 60% blue tiles ana 40% green tiles
    /// </summary>
    /// <returns></returns>
    private HexTileSetup[] GenerateConfigurations()
    {
        int tilesCount = hunkSize.x * hunkSize.y;
        HexTileSetup[] tilesForInit = new HexTileSetup[tilesCount];

        int currentConfigurationIndex = 0;
        int configurationMaxCount = GetConfigurationTilesCount();
        for (int i = 0; i < tilesForInit.Length; i++)
        {
            if (i >= configurationMaxCount)
            {
                currentConfigurationIndex++;
                currentConfigurationIndex = Mathf.Clamp(currentConfigurationIndex, 0, tilesConfiguration.Count - 1);
                configurationMaxCount += GetConfigurationTilesCount();
            }

            tilesForInit[i] = tilesConfiguration[currentConfigurationIndex];
        }

        int GetConfigurationTilesCount() => tilesCount * tilesConfiguration[currentConfigurationIndex].frequency / 100;

        // Randomize tiles order
        tilesForInit = tilesForInit
            .OrderBy(t => Random.value)
            .ToArray();

        return tilesForInit.ToArray();
    }

    #region Odin inspector validation

    // Odin validation method
    // ReSharper disable once UnusedMember.Local
    private bool ValidateFrequency()
    {
        if (!tilesConfiguration.Any())
            return false;

        tilesConfiguration.Last().frequency = 100 - tilesConfiguration
            .Take(tilesConfiguration.Count - 1)
            .Sum(t => t.frequency);
        return true;
    }

    // Odin validation method
    // ReSharper disable once UnusedMember.Local
    private bool HasAnyElements() => tilesConfiguration.Any();

    #endregion

    [Serializable]
    public class HexTileSetup
    {
        // @formatter:off
        public enum Color { Green, Blue, Yellow, Gray }
        // @formatter:on

        public Color color = Color.Gray;

        [Required]
        public Sprite sprite;

        [Range(0, 100)]
        public int frequency = 100;

        public bool interactable = false;
    }
}
#endif