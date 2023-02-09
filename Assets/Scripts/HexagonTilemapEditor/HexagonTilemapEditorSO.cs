#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using Random = UnityEngine.Random;

namespace HexagonTilemapEditor
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Hexagon Tilemap Editor/Config", fileName = "config", order = 0)]
    public class HexagonTilemapEditorSO : SerializedScriptableObject
    {
        // SERIALIZED
        [Title("Hexagon Tilemap Editor")]
        [SerializeField] [InlineProperty(LabelWidth = 13)]
        private Vector2Int tilemapSize = new Vector2Int(1000, 1000);

        [FormerlySerializedAs("hunkSize")]
        [SerializeField] [InlineProperty(LabelWidth = 13)]
        private Vector2Int _hunkSize = new Vector2Int(100, 100);

        [SerializeField]
        private int hunkPopulatingSpeed = 50;

        [SerializeField]
        public int startXIndex = 1;

        [SerializeField]
        public int startYIndex = 1;

        [SerializeField] [InlineEditor] [Required]
        private TilesConfigurations tilesConfiguration;
  
        // CONSTANTS
        private const string HUNKS_SO_PATH = "Assets/ScriptableObjects/Hunks/";

        // PROPERTIES
        public Vector2Int TilemapSize => tilemapSize;
        public Vector2Int HunkSize => _hunkSize;
        public TilesConfigurations TilesConfiguration => tilesConfiguration;
        public int HunkPopulatingSpeed => hunkPopulatingSpeed;

        // PRIVATE METHODS
        [Title("As scriptable objects")]
        [Button("Generate Hunks")]
        private void GenerateHunksBtn()
        {
            EditorCoroutineUtility.StartCoroutine(GenerateHunks(), this);
        }

        private HunkSO GenerateHunk(int xIndex, int yIndex)
        {
            // Describes how lot of specifics tiles will be generate. E.g. 60% blue tiles
            HexTileSetup[] configurations = GenerateConfigurations();

            HunkSO hunkSo = CreateInstance<HunkSO>();

            int currentConfigIndex = 0;
            for (int i = 0; i < _hunkSize.y; i++)
            {
                // Index to get this tile on grid
                int globalTileIndexY = i + yIndex * _hunkSize.y + startYIndex;
                if (globalTileIndexY >= tilemapSize.y)
                    break;

                for (int j = 0; j < _hunkSize.x; j++)
                {
                    int globalTileIndexX = j + xIndex * _hunkSize.x + startXIndex;
                    if (globalTileIndexX >= tilemapSize.x)
                        break;

                    HexData hexData =
                        new HexData(
                            prefabId: configurations[currentConfigIndex].id,
                            coordinates: new Vector2Int(globalTileIndexX, globalTileIndexY));
                    hunkSo.AddHex(hexData);

                    currentConfigIndex++;
                }
            }

            string hunkName = $"Hunk_{hunkSo.MinCoords.x}_{hunkSo.MinCoords.y}" +
                              $"_{hunkSo.MaxCoords.x}_{hunkSo.MaxCoords.y}.asset";
            hunkSo.name = hunkName;
            AssetDatabase.CreateAsset(hunkSo, HUNKS_SO_PATH + hunkName);
            AssetDatabase.SaveAssets();

            return hunkSo;
        }

        private IEnumerator GenerateHunks()
        {
            Vector2Int hunksToGenerateCount = GetHunksCountToGenerate();
            yield return null;

            yield return new WaitForSeconds(0.5f);
            while (_isDeletingHunks)
            {
                // Waiting for deleting existing previous hunks
                yield return new WaitForSeconds(0.2f);
            }

            int hunksCount = hunksToGenerateCount.x * hunksToGenerateCount.y;
            EditorUtility.DisplayProgressBar(
                title: "Generating hunks",
                info: "Generating hunks 0,0",
                progress: (float)0 / hunksCount);

            HunksManagerSo hunksManagerSo = CreateInstance<HunksManagerSo>();

            for (int i = 0; i < hunksToGenerateCount.x; i++)
            {
                for (int j = 0; j < hunksToGenerateCount.y; j++)
                {
                    HunkSO hunkSo = GenerateHunk(i, j);
                    Debug.Log(
                        $"Added hunk {hunkSo.MinCoords.x};{hunkSo.MinCoords.y};{hunkSo.MaxCoords.x};{hunkSo.MaxCoords.y}");
                    string addresablePath = AssetDatabase.GetAssetPath(hunkSo);
                    hunksManagerSo.AddHunk(
                        new MinMaxRange(
                            hunkSo.MinCoords.x,
                            hunkSo.MinCoords.y,
                            hunkSo.MaxCoords.x,
                            hunkSo.MaxCoords.y),
                        addresablePath);

                    EditorUtility.DisplayProgressBar(
                        title: "Generating hunks",
                        info: $"Generating hunks {j},{i}",
                        progress: (float)((i + startXIndex) * (j + startYIndex)) / hunksCount);

                    yield return null;
                }
            }

            AssetDatabase.CreateAsset(hunksManagerSo,
                HUNKS_SO_PATH + "HunksManager.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        [Button("Clear hunks")]
        private void ClearHunksBtn()
        {
            EditorCoroutineUtility.StartCoroutine(ClearHunks(), this);
        }

        [Button("Set Data As Addressable")]
        private void SetDataAsAddressable_Btn(string label = "TilesData")
        {
            EditorCoroutineUtility.StartCoroutine(SetDataAsAddressable(label), this);
        }
    
        private IEnumerator SetDataAsAddressable(string label)
        {
            string[] files = Directory.GetFiles(HUNKS_SO_PATH);
            int filesCount = files.Length;
            EditorUtility.DisplayProgressBar(
                title: "Deleting hunks",
                info: "Deleting hunks",
                progress: 0f / filesCount);
            for (int i = 0; i < filesCount; i++)
            {
                AddressableHelper.CreateAssetEntry( 
                    label: label,
                    source: AssetDatabase.LoadAssetAtPath<SerializedScriptableObject>(files[i]));
                EditorUtility.DisplayProgressBar(
                    title: "Setting data as addressable",
                    info: $"File: {i + 1}",
                    progress: i / filesCount);
                Debug.Log($"Setting data as addressable: {i+1}/{filesCount}");
                yield return null;
            }

            _isDeletingHunks = false;
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
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
                    currentConfigurationIndex = Mathf.Clamp(
                        value: currentConfigurationIndex, 
                        min: 0,
                        max: tilesConfiguration.Configurations.Count - 1);
                    configurationMaxCount += GetConfigurationTilesCount();
                }

                tilesForInit[i] = tilesConfiguration.Configurations[currentConfigurationIndex];
            }

            int GetConfigurationTilesCount() => 
                tilesCount * tilesConfiguration.Configurations[currentConfigurationIndex].frequency / 100;

            // Randomize tiles order
            tilesForInit = tilesForInit
                .OrderBy(t => Random.value)
                .ToArray();

            return tilesForInit.ToArray();
        }
    }
}
#endif