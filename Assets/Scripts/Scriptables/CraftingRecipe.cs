using UnityEngine;

[CreateAssetMenu(fileName ="New Recipe", menuName = "Scriptableobjects/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public CraftingMaterial craftedMaterial;
    public CraftingMaterial[] craftingMaterials;
}
