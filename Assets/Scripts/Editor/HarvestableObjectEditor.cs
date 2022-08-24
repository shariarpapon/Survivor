using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HarvestableObject))]
public class HarvestableObjectEditor : Editor
{
    public static HarvestableObject targetHO;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        if (GUILayout.Button("Create New", GUILayout.Height(30))) 
        {
            targetHO = target as HarvestableObject;
            EditorWindow window = EditorWindow.GetWindow<HarvestableEditorWindow>("Harvestable Creator");
        }
    }
}
