using UnityEngine;
using UnityEditor;

public class MenuItemEditor : Editor
{
    [MenuItem("Assets/Verify ItemData")]
    private static void VerifyItemData()
    {
        ItemManager manager = FindObjectOfType<ItemManager>();
        EditorUtility.SetDirty(manager);

        if (manager == null)
        {
            Debug.Log("<color=#ff8080>Item Manager does not exist in the current scene</color>");
            return;
        }
        else
        {
            foreach (Object obj in Selection.objects)
            {
                if (obj.GetType() == typeof(ItemData))
                {
                    ItemData item = (ItemData)obj;
                    if (manager.itemList.Contains(item)) Debug.Log($"<color=#80aaff>{item.name} already exists in the list.</color>");
                    else
                    {
                        manager.itemList.Add(item);
                        Debug.Log($"<color=#ccff99>{item.name} succesfully added to the list!</color>");
                    }
                }
                else Debug.Log($"<color=#ffd480>{obj.name} is not of type ItemData</color>");
            }
        }
    }

    [MenuItem("Assets/Verify Recipe")]
    private static void VerifyRecipeData()
    {
        CraftingManager manager = FindObjectOfType<CraftingManager>();
        EditorUtility.SetDirty(manager);

        if (manager == null)
        {
            Debug.Log("<color=#ff8080>Crafting Manager does not exist in the current scene</color>");
            return;
        }
        else
        {
            foreach (Object obj in Selection.objects)
            {
                if (obj.GetType() == typeof(CraftingRecipe))
                {
                    CraftingRecipe recipe = (CraftingRecipe)obj;
                    if (manager.craftingRecipes.Contains(recipe)) Debug.Log($"<color=#80aaff>{recipe.name} already exists in the list.</color>");
                    else
                    {
                        manager.craftingRecipes.Add(recipe);
                        Debug.Log($"<color=#ccff99>{recipe.name} succesfully added to the list!</color>");
                    }
                }
                else Debug.Log($"<color=#ffd480>{obj.name} is not of type CraftingRecipe</color>");
            }
        }
    }
}
