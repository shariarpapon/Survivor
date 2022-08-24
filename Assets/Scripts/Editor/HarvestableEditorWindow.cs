using UnityEditor;
using UnityEngine;

public class HarvestableEditorWindow : EditorWindow
{
    private const string assetPath = "Assets/Scriptable Objects/Harvestable";
    private HarvestableData harvestableData;
    private SerializedObject s_harvestableData;
    private string assetName;

    private HarvestableObject targetHO;

    private void OnEnable() 
    {
        Init();
    }

    private void Init() 
    {
        InitNewCreation();
    }

    private void InitNewCreation() 
    {
        harvestableData = CreateInstance<HarvestableData>();
        s_harvestableData = new SerializedObject(harvestableData);
        targetHO = null;
    }

    public void OnGUI() 
    {
        DrawFields();
    }

    public void DrawFields() 
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Name");
        assetName = EditorGUILayout.TextField(assetName);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(s_harvestableData.GetIterator(), true);
        s_harvestableData.ApplyModifiedProperties();
        s_harvestableData.Update();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create", GUILayout.Height(30))) Create();
        if (GUILayout.Button("Create & Assign", GUILayout.Height(30))) { Create(); Assign(); }
        if (GUILayout.Button("Reset", GUILayout.Height(30))) InitNewCreation();
        EditorGUILayout.EndHorizontal();
    }


    private void Create() 
    {
        s_harvestableData.ApplyModifiedProperties();
        s_harvestableData.Update();

        try
        {
            AssetDatabase.CreateAsset((Object)harvestableData, assetPath + $"/{assetName} HD.asset");
            Debug.Log("<color=green>Asset succesfully created</color>");
        }
        catch { Debug.LogError("Unable to create asset"); }
    }

    private void Assign() 
    {
        if (!string.IsNullOrEmpty(assetName))
        {
            targetHO = Selection.activeGameObject.GetComponent<HarvestableObject>();
            if (targetHO != null) targetHO.data = harvestableData;
        }
    }
}
