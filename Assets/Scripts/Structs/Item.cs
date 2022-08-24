[System.Serializable]
public struct Item
{
    public ItemData itemData;
    public ItemStats stats;

    public Item(ItemData itemData)
    {
        this.itemData = itemData;
        stats = new ItemStats(itemData.durability, itemData.damage);
    }

    public static Item Null { get { return new Item(); } }

    public void DamageTool(float damage) 
    {
        stats.durability -= damage;
    }

    public static bool operator ==(Item a, Item b) => a.itemData == b.itemData && a.stats == b.stats;
    public static bool operator !=(Item a, Item b) => a.itemData != b.itemData || a.stats != b.stats;
    public override int GetHashCode() => base.GetHashCode();
    public override bool Equals(object obj) => base.Equals(obj);
}

[System.Serializable]
public struct ItemStats
{
    public float durability;
    public float attackDamage;
    private readonly float maxDurability;

    public ItemStats(float durability, float attackDamage)
    {
        this.durability = durability;
        this.attackDamage = attackDamage;
        maxDurability = durability;
    }

    public float DurabilityPercentage 
    {
        get { return durability  / maxDurability; }
    }

    public static ItemStats Null { get { return new ItemStats(float.NaN, float.NaN); } }

    public static bool operator ==(ItemStats a, ItemStats b) { return a.attackDamage == b.attackDamage && a.durability == b.durability; }
    public static bool operator !=(ItemStats a, ItemStats b) { return a.attackDamage != b.attackDamage || a.durability != b.durability; }
    public override int GetHashCode() { return base.GetHashCode(); }
    public override bool Equals(object obj) { return base.Equals(obj); }
}
