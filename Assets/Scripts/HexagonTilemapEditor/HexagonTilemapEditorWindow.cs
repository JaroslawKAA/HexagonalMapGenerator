#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;

public class HexagonTilemapEditorWindow : OdinEditorWindow
{
    [MenuItem("Tools/Hexagon Tilemap Editor/Editor")]
    public static void ShowWindow()
    {
        GetWindow<HexagonTilemapEditorWindow>()
            .position = GUIHelper.GetEditorWindowRect().AlignCenter(300, 600);
    }

    protected override object GetTarget()
    {
        return AssetDatabase.LoadAssetAtPath<HexagonTilemapEditor.HexagonTilemapEditorSO>("Assets/ScriptableObjects/HexagonTilemapEditor/config.asset");
    }
}
#endif