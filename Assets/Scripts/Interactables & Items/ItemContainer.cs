using System.Collections;
using UnityEngine;

public class ItemContainer : MonoBehaviour, IInitializer
{
    public GameObject inventoryPanel;
    private ItemSlot[] slots;

    public virtual IEnumerator Init()
    {
        slots = new ItemSlot[inventoryPanel.transform.childCount];
        for (int i = 0; i < slots.Length; i++) slots[i] = inventoryPanel.transform.GetChild(i).GetComponent<ItemSlot>();
        yield return null;
    }

    public virtual bool AddItem(Item item) 
    {
        for (int i = 0; i < slots.Length; i++)
            if (slots[i].TryAdd(item))
             {
                TooltipManager.Instance.TimedPopup(null, $"{item.itemData.name} added to inventory.", 2.5f);
                return true;
            }

        TooltipManager.Instance.TimedPopup(null, $"Inventory is full.", 2.5f, TextOptions.RedContent);
        return false;
    }

    //[FIX] make it check the culmunative item count instead of just single slots
    public ItemSlot ContainsItem(ItemData item, int amount) 
    {
        foreach (ItemSlot slot in slots) 
            if (!slot.IsEmpty && slot.slotItem.itemData == item && slot.itemCount >= amount) 
                return slot;

        return null;
    }
}   

