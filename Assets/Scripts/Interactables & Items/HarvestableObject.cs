public class HarvestableObject : OnHitInteractable 
{
    public HarvestableData data;
    private int health;

    private void Start() //No need for to IInitilizer
    {
        health = data.health;
    }
    
    public override bool Hit(Interactor interactor)
    {
        if (data.requiresTool) 
        {
            if (EquipmentManager.Instance.EquipedTool == data.harvestTool) 
            { 
                Damage(data.harvestTool.damage);
                EquipmentManager.Instance.DamageTool(1);
            }
            else return false;
        }
        else 
        {
            if (data.harvestTool == null || EquipmentManager.Instance.toolSlot.slotItem == Item.Null)
            {
                Damage(Interactor.defaultDamage);
            }
            else if (EquipmentManager.Instance.EquipedTool == data.harvestTool)
            {
                Damage(data.harvestTool.damage);
                EquipmentManager.Instance.DamageTool(1);
            }
            else return false;
        }

        if (health <= 0) Harvest(interactor);
        return true;
    }

    private void Damage(int amount) 
    {
        health -= amount;
    }

    private void Harvest(Interactor interactor)
    {
        for (int i = 0; i < data.harvestDrops.Length; i++) 
        {
            ItemDrop drop = data.harvestDrops[i];
            if (WorldManager.prng.NextDouble() <= drop.chance) interactor.DropToGround(new Item(drop.itemData));
        }
        Destroy(gameObject);
    }
}
