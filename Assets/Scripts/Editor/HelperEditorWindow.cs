using UnityEditor;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class HelperEditorWindow : EditorWindow
{
    [MenuItem("Window/Survivor/Helper Utility")]
    public static void OpenWindow() => GetWindow<HelperEditorWindow>("Helper Utility").Show();

    private Object  fontAsset;

    private void OnGUI()
    {

        fontAsset = EditorGUILayout.ObjectField("Font Asset", fontAsset, typeof(TMP_FontAsset), true);

        var texts = FindObjectsOfType<TextMeshProUGUI>();

        if (GUILayout.Button("Assign to All")) 
        {
            TMP_FontAsset font = (TMP_FontAsset)fontAsset;
            if (fontAsset)
                foreach (var tmp in texts)
                {
                    EditorUtility.SetDirty(tmp);
                    tmp.font = font;
                }
        }

    }

}
