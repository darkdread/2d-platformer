using UnityEditor;
using UnityEngine;

public class SelectedObjectsWindow : EditorWindow {
    [MenuItem("My Game/Selected Objects")]
    public static void ShowWindow() {
        GetWindow<SelectedObjectsWindow>(false, "Selected Objects", true);
    }

    private void OnGUI() {
        EditorGUILayout.IntField("Selected Object(s)", Selection.objects.Length);
    }

    private void OnSelectionChange() {
        OnGUI();
    }
}