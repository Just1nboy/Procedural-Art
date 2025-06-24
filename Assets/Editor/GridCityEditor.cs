using UnityEngine;
using UnityEditor;
using Demo;

[CustomEditor(typeof(GridCity))]
public class GridCityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();

        if (GUILayout.Button("Generate City"))
        {
            var city = (GridCity)target;
            Undo.RegisterCompleteObjectUndo(city.gameObject, "Regenerate City");
            city.Regenerate();
            EditorUtility.SetDirty(city);
        }

        if (GUILayout.Button("Clear City"))
        {
            var city = (GridCity)target;
            Undo.RegisterCompleteObjectUndo(city.gameObject, "Clear City");
            city.ClearCity();
            EditorUtility.SetDirty(city);
        }
    }
}
