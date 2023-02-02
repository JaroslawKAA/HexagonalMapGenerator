using System.Collections;
using System.IO;
using Sirenix.OdinInspector;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public partial class HexagonTilemapEditorSO
{
    [Title("As prefabs (depreciated)")]
    [Button("Generate Tiles (Depreciated)")]
    [DisableIf("@tilemap == null || grid == null")]
    private void GenerateTilesButton_Depreciated()
    {
        EditorCoroutineUtility.StartCoroutine(GenerateHunks_Depreciated(), this);
    }

    private IEnumerator GenerateHunks_Depreciated()
    {
        OnClearTilesButton_Depreciated();

        // Describes how big will be hunk part
        Vector2Int hunksToGenerateCount = GetHunksCountToGenerate();
        yield return null;

        yield return new WaitForSeconds(0.5f);
        while (isDeletingHunks_depreciated)
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
                GenerateHunk_Depreciated(i, j);
                
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
    
    private void GenerateHunk_Depreciated(int xIndex, int yIndex)
    {
        // Describes how lot of specifics tiles will be generate.
        HexTileSetup[] configurations = GenerateConfigurations();

        GameObject hunkGO = new GameObject(
            name: $"Hunk_{xIndex}_{yIndex}",
            components: typeof(Hunk));
        hunkGO.transform.parent = tilemap.transform;
        Hunk hunk = hunkGO.GetComponent<Hunk>();

        int currentConfigIndex = 0;
        for (int i = 0; i < _hunkSize.y; i++)
        {
            int globalTileIndexY = i + yIndex * _hunkSize.y;
            if (globalTileIndexY >= tilemapSize.y) break;

            for (int j = 0; j < _hunkSize.x; j++)
            {
                int globalTileIndexX = j + xIndex * _hunkSize.x;
                if (globalTileIndexX >= tilemapSize.x) break;
                HexTile hexTile = GenerateTile_Depreciated(globalTileIndexX, globalTileIndexY, configurations,
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
    
    private HexTile GenerateTile_Depreciated(
        int globalTileIndexX,
        int globalTileIndexY,
        HexTileSetup[] configurations,
        ref int currentConfigIndex)
    {
        HexTile hexTile = Instantiate(hexTilePrefab);
        hexTile.gameObject.name = $"HexTile{globalTileIndexX + startXIndex}_{globalTileIndexY + startYIndex}";
        hexTile.SpriteRenderer.sprite = configurations[currentConfigIndex].sprite;

        hexTile.Init(
            id: configurations[currentConfigIndex].id,
            coordinates: new Vector2Int(globalTileIndexX + startXIndex, globalTileIndexY + startYIndex),
            interactable: configurations[currentConfigIndex].interactable);

        hexTile.transform.position = grid.GetCellCenterWorld(new Vector3Int(
            globalTileIndexY,
            globalTileIndexX,
            0));

        currentConfigIndex++;
        return hexTile;
    }
    
    [Button("Clear tiles (Depreciated)")]
    [DisableIf("@tilemap == null || grid == null")]
    void OnClearTilesButton_Depreciated()
    {
        EditorCoroutineUtility.StartCoroutine(ClearTiles_Depreciated(), this);
    }

    private bool isDeletingHunks_depreciated = false;
    private IEnumerator ClearTiles_Depreciated()
    {
        isDeletingHunks_depreciated = true;

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

        isDeletingHunks_depreciated = false;
        EditorUtility.ClearProgressBar();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}
