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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="minMaxCoordinates"></param>
    /// <returns>Addresable reference</returns>
    public string GetHunk(MinMaxRange minMaxCoordinates) => hunks[minMaxCoordinates];

    public void RemoveHunk(MinMaxRange minMaxCoords)
    {
        if (hunks.ContainsKey(minMaxCoords))
        {
            hunks.Remove(minMaxCoords);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        else
        {
            Debug.LogError("Key wasn't found");
        }
    }

    public List<string> TryGetCurrentSet(Vector2Int hunkPoint)
    {
        MinMaxRange hunkRange = hunks.Keys.FirstOrDefault(h => IsPointInRange(h, hunkPoint));

        var result = new List<string>();
        result.AddRange(TryGetNeighbourPaths(hunkPoint));

        string centerHunk = TryGetHunk(hunkPoint);
        if (centerHunk != String.Empty) result.Add(centerHunk);

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
        MinMaxRange hunkRange = hunks.Keys.FirstOrDefault(h => IsPointInRange(h, hunkPoint));
        if (hunkRange.IsDefault) return String.Empty;
        return hunks[hunkRange];
    }

    private List<string> TryGetNeighbourPaths(Vector2Int hunkPoint)
    {
        List<string> result = new List<string>();

        MinMaxRange hunkRange = hunks.Keys.FirstOrDefault(h => IsPointInRange(h, hunkPoint));
        if (!hunkRange.IsDefault)
        {
            string hunkPathTemp = String.Empty;
            Vector2Int neighbourHunkPoint = new Vector2Int();

            // --- Left hunk ---
            // Estimated point in left hunk
            neighbourHunkPoint = new Vector2Int(hunkRange.minX - 1, hunkRange.minY);
            TryGetNeighbourHunkPath(neighbourHunkPoint, ref hunkPathTemp, ref result);

            // --- Right hunk ---
            // Estimated point in right hunk
            neighbourHunkPoint = new Vector2Int(hunkRange.maxX + 1, hunkRange.minY);
            TryGetNeighbourHunkPath(neighbourHunkPoint, ref hunkPathTemp, ref result);

            // --- Top hunk ---
            // Estimated point in top hunk
            neighbourHunkPoint = new Vector2Int(hunkRange.minX, hunkRange.maxY + 1);
            TryGetNeighbourHunkPath(neighbourHunkPoint, ref hunkPathTemp, ref result);

            // --- Down hunk ---
            // Estimated point in down hunk
            neighbourHunkPoint = new Vector2Int(hunkRange.minX, hunkRange.minY - 1);
            TryGetNeighbourHunkPath(neighbourHunkPoint, ref hunkPathTemp, ref result);


            // --- Left Top hunk ---
            // Estimated point in Left Top hunk
            neighbourHunkPoint = new Vector2Int(hunkRange.minX - 1, hunkRange.maxY + 1);
            TryGetNeighbourHunkPath(neighbourHunkPoint, ref hunkPathTemp, ref result);

            // --- Left Down hunk ---
            // Estimated point in Left Down hunk
            neighbourHunkPoint = new Vector2Int(hunkRange.minX - 1, hunkRange.minY - 1);
            TryGetNeighbourHunkPath(neighbourHunkPoint, ref hunkPathTemp, ref result);

            // --- Right Top hunk ---
            // Estimated point in Right Top hunk
            neighbourHunkPoint = new Vector2Int(hunkRange.maxX + 1, hunkRange.maxY + 1);
            TryGetNeighbourHunkPath(neighbourHunkPoint, ref hunkPathTemp, ref result);


            // --- Right Down hunk ---
            // Estimated point in Right Top hunk
            neighbourHunkPoint = new Vector2Int(hunkRange.maxX + 1, hunkRange.minY - 1);
            TryGetNeighbourHunkPath(neighbourHunkPoint, ref hunkPathTemp, ref result);
        }

        return result;
    }

    private bool IsPointInRange(MinMaxRange minMaxRange, Vector2Int hunkPoint) =>
        minMaxRange.minX <= hunkPoint.x
        && minMaxRange.minY <= hunkPoint.y
        && minMaxRange.maxX >= hunkPoint.x
        && minMaxRange.maxY >= hunkPoint.y;

    private void TryGetNeighbourHunkPath(
        Vector2Int neighbourPoint,
        ref string hunkTemp,
        ref List<string> result)
    {
        // Estimated point
        Vector2Int pointTemp = neighbourPoint;
        hunkTemp = TryGetHunk(pointTemp);
        // If hunk exist
        if (hunkTemp != String.Empty)
            result.Add(hunkTemp);
    }
    
    private MinMaxRange GetHunkRange(Vector2Int hunkPoint)
    {
        return new MinMaxRange(
            minX: (hunkPoint.x / _mapConfig.HunkSize.x * _mapConfig.HunkSize.x) + _mapConfig.startXIndex,
            minY: ((hunkPoint.y / _mapConfig.HunkSize.y) * _mapConfig.HunkSize.y) + _mapConfig.startYIndex,
            maxX: (hunkPoint.x / _mapConfig.HunkSize.x * _mapConfig.HunkSize.x) + _mapConfig.HunkSize.x,
            maxY: (hunkPoint.y / _mapConfig.HunkSize.y * _mapConfig.HunkSize.y) + _mapConfig.HunkSize.y);
    }
}