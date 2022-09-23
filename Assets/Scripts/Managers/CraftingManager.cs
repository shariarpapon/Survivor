using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Survivor.Core
{
    public class CraftingManager : MonoBehaviour, IInitializer
    {
        public static CraftingManager Instance { get; private set; }

        public List<CraftingRecipe> craftingRecipes = new List<CraftingRecipe>();
        private ItemContainer playerInventory;

        [HideInInspector] public Interactor player;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public IEnumerator Init()
        {
            player = FindObjectOfType<PlayerController>().GetComponent<Interactor>();
            playerInventory = player.GetComponent<ItemContainer>();

            yield return null;
        }

        public void CraftItem(CraftingRecipe recipe)
        {
            var slots = GetMaterialSlots(recipe);
            if (slots != null)
            {
                foreach (CraftingMaterial mat in recipe.craftingMaterials)
                    slots[mat].RemoveAmount(mat.amount, false);

                bool isInventoryFull = false;

                for (int i = 0; i < recipe.craftedMaterial.amount; i++)
                {
                    Item craftedItem = new Item(recipe.craftedMaterial.itemData);

                    if (isInventoryFull == false)
                        isInventoryFull = !playerInventory.AddItem(craftedItem);

                    if (isInventoryFull == true)
                        player.DropToGround(craftedItem);
                }
            }
            else TooltipManager.Instance.TimedPopup(null, "Unable to craft item.", 2.5f, TextOptions.RedContent);
        }

        public bool TryRemoveStructureCraftMaterials(CraftingRecipe recipe)
        {
            Dictionary<CraftingMaterial, ItemSlot> matSlots = GetMaterialSlots(recipe);
            if (matSlots != null)
            {
                foreach (CraftingMaterial mat in recipe.craftingMaterials)
                    matSlots[mat].RemoveAmount(mat.amount, false);
                return true;
            }
            else
            {
                TooltipManager.Instance.TimedPopup(null, "Unable to craft.", 2.5f, TextOptions.RedContent);
                return false;
            }
        }

        private Dictionary<CraftingMaterial, ItemSlot> GetMaterialSlots(CraftingRecipe recipe)
        {
            Dictionary<CraftingMaterial, ItemSlot> validSlots = new Dictionary<CraftingMaterial, ItemSlot>();
            for (int i = 0; i < recipe.craftingMaterials.Length; i++)
            {
                CraftingMaterial mat = recipe.craftingMaterials[i];
                ItemSlot slot = playerInventory.ContainsItem(mat.itemData, mat.amount);
                if (slot != null) validSlots.Add(mat, slot);
                else return null;
            }
            return validSlots;
        }

        public bool IsCraftable(CraftingRecipe recipe)
        {
            foreach (CraftingMaterial mat in recipe.craftingMaterials)
                if (!player.inventory.ContainsItem(mat.itemData, mat.amount)) return false;

            return true;
        }
    }
}

