using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Level/Hunks Manager", fileName = "HunksManagerSo", order = 0)]
public class HunksManagerSo : SerializedScriptableObject
{
    // SERIALIZED
    [SerializeField]
    private Dictionary<MinMaxRange, string> hunks = new Dictionary<MinMaxRange, string>();

    [SerializeField] [Required]
    private HexagonTilemapEditorSO _mapConfig;

    // PUBLIC

    public void AddHunk(MinMaxRange minMaxCoords, string addresablePath)
    {
        hunks.Add(minMaxCoords, addresablePath);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    public List<string> TryGetCurrentSet(Vector2Int hunkPoint)
    {
        var result = new List<string>();

        string centerHunk = TryGetHunk(hunkPoint);
        if (centerHunk != String.Empty) result.Add(centerHunk);

        result.AddRange(TryGetNeighbourPaths(hunkPoint));

        return result;
    }


    // PRIVATE

    /// <summary>
    /// Try get hunk path or return String.Empty
    /// </summary>
    /// <param name="hunkPoint">Point witch hunk should contains.</param>
    /// <returns>Hunk Path</returns>
    private string TryGetHunk(Vector2Int hunkPoint)
    {
        MinMaxRange hunkRange = GetHunkRange(hunkPoint);
        if (hunks.ContainsKey(hunkRange))
            return hunks[hunkRange];

        return String.Empty;
    }

    private List<string> TryGetNeighbourPaths(Vector2Int hunkPoint)
    {
        List<string> result = new List<string>();

        MinMaxRange hunkRange = GetHunkRange(hunkPoint);
        if (!hunkRange.IsDefault)
        {
            Vector2Int neighbourHunkPoint = new Vector2Int();

            // --- Left hunk ---
            // Estimated point in left hunk
            neighbourHunkPoint = new Vector2Int(hunkRange.minX - 1, hunkRange.minY);
            TryGetNeighbourHunkPath(neighbourHunkPoint, ref result);

            // --- Right hunk ---
            // Estimated point in right hunk
            neighbourHunkPoint = new Vector2Int(hunkRange.maxX + 1, hunkRange.minY);
            TryGetNeighbourHunkPath(neighbourHunkPoint, ref result);

            // --- Top hunk ---
            // Estimated point in top hunk
            neighbourHunkPoint = new Vector2Int(hunkRange.minX, hunkRange.maxY + 1);
            TryGetNeighbourHunkPath(neighbourHunkPoint, ref result);

            // --- Down hunk ---
            // Estimated point in down hunk
            neighbourHunkPoint = new Vector2Int(hunkRange.minX, hunkRange.minY - 1);
            TryGetNeighbourHunkPath(neighbourHunkPoint, ref result);


            // --- Left Top hunk ---
            // Estimated point in Left Top hunk
            neighbourHunkPoint = new Vector2Int(hunkRange.minX - 1, hunkRange.maxY + 1);
            TryGetNeighbourHunkPath(neighbourHunkPoint, ref result);

            // --- Left Down hunk ---
            // Estimated point in Left Down hunk
            neighbourHunkPoint = new Vector2Int(hunkRange.minX - 1, hunkRange.minY - 1);
            TryGetNeighbourHunkPath(neighbourHunkPoint, ref result);

            // --- Right Top hunk ---
            // Estimated point in Right Top hunk
            neighbourHunkPoint = new Vector2Int(hunkRange.maxX + 1, hunkRange.maxY + 1);
            TryGetNeighbourHunkPath(neighbourHunkPoint, ref result);


            // --- Right Down hunk ---
            // Estimated point in Right Top hunk
            neighbourHunkPoint = new Vector2Int(hunkRange.maxX + 1, hunkRange.minY - 1);
            TryGetNeighbourHunkPath(neighbourHunkPoint, ref result);
        }

        return result;
    }

    private void TryGetNeighbourHunkPath(
        Vector2Int neighbourPoint,
        ref List<string> result)
    {
        // Estimated point
        string hunkTemp = TryGetHunk(neighbourPoint);
        // If hunk exist
        if (hunkTemp != String.Empty)
            result.Add(hunkTemp);
    }

    
    int _cachedMinX;
    int _cachedMinY;
    int _minxModulo;
    int _minYModulo;
    
    /// <summary>
    /// Fast calculation of hunk range
    /// </summary>
    /// <param name="hunkPoint"></param>
    /// <returns></returns>
    private MinMaxRange GetHunkRange(Vector2Int hunkPoint)
    {
        _cachedMinX = 0;
        _cachedMinY = 0;

        _minxModulo = hunkPoint.x % _mapConfig.HunkSize.x;
        _minYModulo = hunkPoint.y % _mapConfig.HunkSize.y;

        if (hunkPoint.x > 0)
            _cachedMinX = hunkPoint.x - _minxModulo;
        else if (hunkPoint.x < 0)
            _cachedMinX = hunkPoint.x - -_minxModulo;

        if (hunkPoint.y > 0)
            _cachedMinY = hunkPoint.y - _minYModulo;
        else if (hunkPoint.y < 0)
            _cachedMinY = hunkPoint.y - -_minYModulo;

        return new MinMaxRange(
            minX: _minxModulo != 0
                ? _cachedMinX + _mapConfig.startXIndex
                : _cachedMinX - _mapConfig.HunkSize.x + _mapConfig.startXIndex,
            minY: _minYModulo != 0
                ? _cachedMinY + _mapConfig.startYIndex
                : _cachedMinY - _mapConfig.HunkSize.y + _mapConfig.startYIndex,
            maxX: _minxModulo != 0 ? _cachedMinX + _mapConfig.HunkSize.x : _cachedMinX,
            maxY: _minYModulo != 0 ? _cachedMinY + _mapConfig.HunkSize.y : _cachedMinY);
    }
}