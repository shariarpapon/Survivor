using UnityEngine;
using UnityEditor;
using Survivor.Core;

public class ItemCreatorEditorWindow : EditorWindow
{
    private const string itemPath = @"Assets/Scriptable Objects/Items";
    private const string recipePath = @"Assets/Scriptable Objects/Recipes";

    private static ItemData itemToCreate;
    private static SerializedObject s_ItemToCreate;

    private static CraftingRecipe itemCraftRecipe;
    private static SerializedObject s_ItemCraftRecipe;

    private SerializedObject selectedObject;

    [MenuItem("Window/Survivor/Item and Recipe")]
    private static void Init()
    {
        ItemCreatorEditorWindow window = GetWindow<ItemCreatorEditorWindow>("Item & Dependency Creator");
        InitNewCreation();

        window.Show();
    }

    private static void InitNewCreation() 
    {
        itemToCreate = ScriptableObject.CreateInstance<ItemData>();
        itemToCreate.maxItemsPerSlot = 16;

        s_ItemToCreate = new SerializedObject(itemToCreate);

        itemCraftRecipe = ScriptableObject.CreateInstance<CraftingRecipe>();
        s_ItemCraftRecipe = new SerializedObject(itemCraftRecipe);

        s_ItemToCreate.ApplyModifiedProperties();
        s_ItemCraftRecipe.Update();
        s_ItemCraftRecipe.ApplyModifiedProperties();
        s_ItemCraftRecipe.Update();
    }

    private void OnGUI()
    {
        DrawButtons();

        if (selectedObject != null) DrawProperties();

        if (itemToCreate.type == ItemType.Tool) itemToCreate.maxItemsPerSlot = 1;

        s_ItemToCreate.ApplyModifiedProperties();
        s_ItemCraftRecipe.ApplyModifiedProperties();
    }

    private void DrawButtons()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Item", GUILayout.Height(35)))
        {
            if (selectedObject != s_ItemToCreate)
            {
                selectedObject = s_ItemToCreate;
            }
        }
        if (GUILayout.Button("Crafting Recipe", GUILayout.Height(35)))
        {
            if (selectedObject != s_ItemCraftRecipe)
            {
                selectedObject = s_ItemCraftRecipe;
            }
        }
        if (GUILayout.Button("Create Item", GUILayout.Height(35))) CreateItem();
        if(GUILayout.Button("Create Recipe", GUILayout.Height(35))) CreateRecipe();

        EditorGUILayout.EndHorizontal();

    }

    private void CreateItem()
    {
        UpdateProperties();
        try
        {
            AssetDatabase.CreateAsset((Object)itemToCreate, itemPath + $"/{itemToCreate.name}.asset");
            InitNewCreation();
            selectedObject = s_ItemToCreate;
            Debug.Log("<color=green> Item succesfully created</color>");
        }
        catch { Debug.Log("<color=red> Unable to create item</color>"); }
    }

    private void CreateRecipe() 
    {
        UpdateProperties();
        try
        {
            string path = recipePath + $"/{itemCraftRecipe.craftedMaterial.itemData.name} Recipe.asset";
            AssetDatabase.CreateAsset((Object)itemCraftRecipe, path);
            InitNewCreation();
            selectedObject = s_ItemCraftRecipe;

            CraftingManager manager = FindObjectOfType<CraftingManager>();
            if (manager != null) 
            {
                CraftingRecipe r = AssetDatabase.LoadAssetAtPath<CraftingRecipe>(path);
                if(manager.craftingRecipes.Contains(r) == false) manager.craftingRecipes.Add(r);
                Debug.Log("Crafting recipe added to list.");
            }

            Debug.Log("<color=green> Recipe succesfully created</color>");

        }
        catch { Debug.Log("<color=red> Unable to create recipe</color>"); }

    }

    private void UpdateProperties() 
    {
        if (itemCraftRecipe.craftedMaterial.itemData == null) itemCraftRecipe.craftedMaterial.itemData = itemToCreate;
        s_ItemToCreate.ApplyModifiedProperties();
        s_ItemCraftRecipe.ApplyModifiedProperties();
        s_ItemToCreate.Update();
        s_ItemCraftRecipe.Update();
    }

    private void DrawProperties() 
    {
        SerializedProperty prop = selectedObject.GetIterator();
        EditorGUILayout.PropertyField(prop, true);

        selectedObject.ApplyModifiedProperties();
        selectedObject.Update();
    }

}
    