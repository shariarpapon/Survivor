using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BiomeFeatureSorterEditorWindow : EditorWindow
{
    private static WorldFeatureArrayWrapper worldFeatureArrayWrapper;
    private static BiomeFeatureSorterEditorWindow myWindow;
    private static GUIStyle blackLabelFont;
    private static bool foldoutSortingBar = true;

    private Biome biome;
    private Biome assignedBiome = null;
    private readonly Dictionary<int, Color> colorSet = new Dictionary<int, Color>()
    {
        { 0, new Color(115f / 255, 181f / 255, 235f / 255)},
        { 1, new Color(167f / 255, 235f / 255, 115f / 255)},
        { 2, new Color(235f / 255, 159f / 255, 115f /255)},
        { 3, new Color(117f / 255, 115f / 255, 235f / 255)},
        { 4, new Color(235f / 255, 227f / 255, 115f / 255)},
        { 5, new Color(235f / 255, 115f / 255, 115f / 255)}
    };

    #region Sizing Variables
    const float barHeight = 400;
    const float barWidth = 100;
    const float barPositionX = 5;
    const float sliderWidth = 250;
    const float defaultBarHeight = 20;

    float initGap = 10;
    float minBarHeight = defaultBarHeight;
    Vector2 scrollPosition = default;
    #endregion

    [MenuItem("Window/Survivor/Biome Feature Sorter")]
    private static void OpenWindow() 
    {
        myWindow = GetWindow<BiomeFeatureSorterEditorWindow>("Biome Feature Sorter");
        myWindow.minSize = new Vector2(500, 600);

        blackLabelFont = new GUIStyle();
        blackLabelFont.normal.textColor = Color.black;
    }

    private void OnGUI()
    {
        initGap = 10;
        biome = (Biome)EditorGUILayout.ObjectField("Biome", biome, typeof(Biome), false, GUILayout.Height(20));
        initGap += 20;

        if (biome != null)
        {
            if (assignedBiome != biome)
            {
                assignedBiome = biome;
                worldFeatureArrayWrapper = CreateInstance<WorldFeatureArrayWrapper>();
                worldFeatureArrayWrapper.SetArray(biome.worldFeatures);
            } 
        }
        else 
        {
            assignedBiome = null;
            worldFeatureArrayWrapper = null;
        }

        if (worldFeatureArrayWrapper != null) 
        { 
            DrawBiomeFeatureEditor();
            return;
        }
    }

    private void DrawBiomeFeatureEditor() 
    {
        GUILayout.BeginVertical();
        if (GUILayout.Button("Sort", GUILayout.Height(20))) worldFeatureArrayWrapper.features.Sort();
        else if (GUILayout.Button("Confirm Changes", GUILayout.Height(20))) worldFeatureArrayWrapper.CopyDataToBiome(biome);
        GUILayout.EndVertical();

        Editor featureEditor = Editor.CreateEditor(worldFeatureArrayWrapper);
        EditorUtility.SetDirty(featureEditor);
        featureEditor.DrawDefaultInspector();

        foldoutSortingBar = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutSortingBar, "Sorting Bar");
        if (foldoutSortingBar) DrawSortingBar();
        EditorGUILayout.EndFoldoutHeaderGroup();

    }

    private void DrawSortingBar() 
    {
        var features = worldFeatureArrayWrapper.features;
        GUILayout.Space(3);

        GUILayout.BeginVertical();
        if (GUILayout.Button("Default Scale", GUILayout.Height(20))) minBarHeight = defaultBarHeight;
        else if (GUILayout.Button("Scale +", GUILayout.Height(20))) minBarHeight += 2.5f;
        else if (GUILayout.Button("Scale -", GUILayout.Height(20))) minBarHeight -= 2.5f;
        minBarHeight = Mathf.Clamp(minBarHeight, 0.1f, float.PositiveInfinity);
        GUILayout.EndVertical();

        float yPosition = initGap;
        float maxBarHeight = barHeight + (features.Count * minBarHeight);
        float prevValue = 0;
        
        Rect workArea = GUILayoutUtility.GetRect(500, 10000, 50, 10000);
        scrollPosition = GUI.BeginScrollView(workArea, scrollPosition, new Rect(0, yPosition - 5, 0, maxBarHeight));
        GUILayout.BeginArea(new Rect(0, 0, 1000, maxBarHeight));

        EditorGUI.DrawRect(new Rect(barPositionX, yPosition, barWidth, maxBarHeight), new Color(224f / 255, 220f / 255, 215f / 255));

        for (int i = 0; i < features.Count; i++)
        {
            WorldFeatureArrayWrapper.WorldFeatureStruct feature = features[i];

            float height = (feature.spawnValue - prevValue) * barHeight + minBarHeight;
            prevValue = feature.spawnValue;

            Rect rect = new Rect(barPositionX, yPosition, barWidth, height);
            Color color = colorSet[i % colorSet.Count];

            EditorGUI.DrawRect(rect, color);
            GUI.Label(new Rect(barPositionX + barWidth * 0.4f, yPosition, barWidth, 15), $"{feature.spawnValue}", blackLabelFont);
            GUI.Label(new Rect(barPositionX + barWidth + 5, yPosition, 500, 15), feature.prefab.name);

            yPosition += height;
            features[i] = feature;
        }

        GUILayout.EndArea();
        GUI.EndScrollView();

        worldFeatureArrayWrapper.features = features;
    }

    private class WorldFeatureArrayWrapper : ScriptableObject
    {
        public List<WorldFeatureStruct> features;

        public void SetArray(WorldFeature[] f)
        {
            features = new List<WorldFeatureStruct>();
            for (int i = 0; i < f.Length; i++)
                features.Add(new WorldFeatureStruct(f[i].prefab, f[i].spawnValue, f[i].spawnValue));

            features.Sort();
        }

        public void CopyDataToBiome(Biome targetBiome) 
        {
            if (targetBiome == null || features == null)
            {
                Debug.Log("Unable to copy data.");
                return;
            }

            EditorUtility.SetDirty(targetBiome);
            features.Sort();
            targetBiome.worldFeatures = new WorldFeature[features.Count];
            for (int i = 0; i < features.Count; i++)
                targetBiome.worldFeatures[i] = new WorldFeature() {
                    prefab = features[i].prefab,
                    spawnValue = features[i].spawnValue
                };

            Debug.Log("Data succesfully copied.");
        }

        [Serializable]
        public struct WorldFeatureStruct  : IComparable
        {
            public GameObject prefab;
            [Range(0, 1)]
            public float spawnValue;
            public float prevSpawnValue;

            public WorldFeatureStruct(GameObject p, float s, float psv)
            {
                prefab = p;
                spawnValue = s;
                prevSpawnValue = psv;
            }

            public int CompareTo(object obj)
            {
                if (obj is WorldFeatureStruct c)
                    return this.spawnValue.CompareTo(c.spawnValue);
                throw new NotImplementedException();
            }
        }
    }


}
