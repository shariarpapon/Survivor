public class ToolSlot : ItemSlot
{
    public override bool TryAdd(Item item)
    {
        if (!item.itemData || item.itemData.type != ItemType.Tool) return false;

        if (base.TryAdd(item)) 
        {
            EquipmentManager.Instance.EquipTool();
            return true; 
        }
        else return false;
    }

    protected override void OnSlotModified()
    {
        base.OnSlotModified();
        if (IsEmpty) EquipmentManager.Instance.UnequipTool();
    }

    public void UpdateDurabilityBar() 
    {
        if (IsEmpty) return;

        durabilityBar.fillAmount = slotItem.stats.DurabilityPercentage;
    }

}
