using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Scriptableobjects/Item")]
public class ItemData : ScriptableObject //property count : 9
{
    public ItemType type;
    public new string name;

    [Tooltip("Required for all items")]
    public GameObject prefab;

    public Sprite icon;
    public int maxItemsPerSlot;

    [Space]
    [Tooltip("The amount of the consumable will effect")]
    public float healthConsumeAmount;
    public float energyConsumeAmount;

    [Space]
    [Tooltip("The amount of damage a tool can inflict")]
    public int damage = 1;

    [Tooltip("The amount of damage a tool can sustain")]
    public int durability;

    [Space]

    [TextArea(5, 10)]
    public string description;

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
