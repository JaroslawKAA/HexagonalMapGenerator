#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Scriptable Objects/Hexagon Tilemap Editor/Config", fileName = "config", order = 0)]
public partial class HexagonTilemapEditorSO : SerializedScriptableObject
{
    [Title("Hexagon Tilemap Editor")]
    [SerializeField] [InlineProperty(LabelWidth = 13)]
    private Vector2Int tilemapSize = new Vector2Int(1000, 1000);

    [FormerlySerializedAs("hunkSize")]
    [SerializeField] [InlineProperty(LabelWidth = 13)]
    private Vector2Int _hunkSize = new Vector2Int(100, 100);

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

    private const string HUNKS_SO_PATH = "Assets/ScriptableObjects/Hunks/";

    [Title("As scriptable objects")]
    [Button("Generate Hunks")]
    public void GenerateHunksBtn()
    {
        EditorCoroutineUtility.StartCoroutine(GenerateHunks(), this);
    }

    private IEnumerator GenerateHunks()
    {
        Vector2Int hunksToGenerateCount = GetHunksCountToGenerate();
        yield return null;

        yield return new WaitForSeconds(0.5f);
        while (_isDeletingHunks)
        {
            // Waiting for deleting existing prewious hunks
            yield return new WaitForSeconds(0.2f);
        }
        
        int hunksCount = hunksToGenerateCount.x * hunksToGenerateCount.y;
        EditorUtility.DisplayProgressBar(
            title: "Generating hunks",
            info: "Generating hunks",
            progress: (float)0 / hunksCount);

        HunksManagerSo hunksManagerSo = CreateInstance<HunksManagerSo>();


        for (int i = 0; i < hunksToGenerateCount.x; i++)
        {
            for (int j = 0; j < hunksToGenerateCount.y; j++)
            {
                HunkSO hunkSo = GenerateHunk(i, j);
                hunksManagerSo.AddHunk(
                    new Vector4Int(
                        hunkSo.MinCoords.x,
                        hunkSo.MinCoords.y,
                        hunkSo.MaxCoords.x,
                        hunkSo.MaxCoords.y),
                    hunkSo);

                EditorUtility.DisplayProgressBar(
                    title: "Generating hunks",
                    info: "Generating hunks",
                    progress: (float)((i + 1) * (j + 1)) / hunksCount);

                yield return null;
            }
        }
        
        AssetDatabase.CreateAsset(hunksManagerSo,
            HUNKS_SO_PATH + "HunksManager.asset");
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    private HunkSO GenerateHunk(int xIndex, int yIndex)
    {
        // Describes how lot of specifics tiles will be generate.
        HexTileSetup[] configurations = GenerateConfigurations();

        HunkSO hunkSo = CreateInstance<HunkSO>();

        int currentConfigIndex = 0;
        for (int i = 0; i < _hunkSize.y; i++)
        {
            // Index to get this tile on grid
            int globalTileIndexY = i + yIndex * _hunkSize.y;
            if (globalTileIndexY >= tilemapSize.y) break;

            for (int j = 0; j < _hunkSize.x; j++)
            {
                int globalTileIndexX = j + xIndex * _hunkSize.x;
                if (globalTileIndexX >= tilemapSize.x) break;

                HunkSO.HexData hexData =
                    new HunkSO.HexData(
                        prefabId: configurations[currentConfigIndex].id,
                        coordinates: new Vector2Int(globalTileIndexX, globalTileIndexY));
                hunkSo.AddHex(hexData);

                currentConfigIndex++;
            }
        }
        
        AssetDatabase.CreateAsset(hunkSo,
            HUNKS_SO_PATH + $"Hunk_{hunkSo.MinCoords.x}_{hunkSo.MinCoords.y}" +
            $"_{hunkSo.MaxCoords.x}_{hunkSo.MaxCoords.y}.asset");

        return hunkSo;
    }

    [Button("Clear hunks")]
    private void ClearHunksBtn()
    {
        EditorCoroutineUtility.StartCoroutine(ClearHunks(), this);
    }
    
    private bool _isDeletingHunks = false;
    private IEnumerator ClearHunks()
    {
        _isDeletingHunks = true;

        string[] files = Directory.GetFiles(HUNKS_SO_PATH);
        int filesCount = files.Length;
        EditorUtility.DisplayProgressBar(
            title: "Deleting hunks",
            info: "Deleting hunks",
            progress: 0f / filesCount);
        for (int i = 0; i < filesCount; i++)
        {
            File.Delete(files[i]);
            EditorUtility.DisplayProgressBar(
                title: "Deleting hunks",
                info: "Deleting hunks",
                progress: i / filesCount);
            yield return null;
        }

        _isDeletingHunks = false;
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    private Vector2Int GetHunksCountToGenerate()
    {
        int xHunksCount = (tilemapSize.x / _hunkSize.x);
        if (tilemapSize.x % _hunkSize.x != 0) xHunksCount++;
        int yHunksCount = (tilemapSize.y / _hunkSize.y);
        if (tilemapSize.y % _hunkSize.y != 0) yHunksCount++;

        int hunksToGenerate = xHunksCount * yHunksCount;
        Debug.Log("All hunks to generate:" + hunksToGenerate);
        Debug.Log($"Hunks count to generate (XY): {xHunksCount}, {yHunksCount}");
        return new Vector2Int(xHunksCount, yHunksCount);
    }

    /// <summary>
    /// Generate configurations for each tile by tilesConfiguration setup.
    /// E. g. 60% blue tiles ana 40% green tiles
    /// </summary>
    /// <returns></returns>
    private HexTileSetup[] GenerateConfigurations()
    {
        int tilesCount = _hunkSize.x * _hunkSize.y;
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
        public int id;

        [Required]
        public Sprite sprite;

        [Range(0, 100)]
        public int frequency = 100;

        public bool interactable = false;
    }
}
#endif