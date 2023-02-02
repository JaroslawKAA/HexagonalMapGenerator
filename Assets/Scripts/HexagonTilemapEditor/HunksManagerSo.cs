using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Level/Hunks Manager", fileName = "HunksManagerSo", order = 0)]
public class HunksManagerSo : SerializedScriptableObject
{
    [SerializeField]
    private Dictionary<Vector4Int, HunkSO> hunks = new Dictionary<Vector4Int, HunkSO>();

    public void AddHunk(Vector4Int minMaxCoords, HunkSO hunkSo)
    {
        hunks.Add(minMaxCoords, hunkSo);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    public HunkSO GetHunk(Vector4Int minMaxCoordinates)
    {
        return hunks[minMaxCoordinates];
    }

    public void RemoveHunk(Vector4Int minMaxCoords)
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
}
