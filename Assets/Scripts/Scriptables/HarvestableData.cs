using UnityEngine;

[CreateAssetMenu(fileName ="New Harvestable", menuName ="Scriptableobjects/Harvesable Data")]
public class HarvestableData : ScriptableObject
{
    [Tooltip("At zero health the object will be harvested")]
    public int health;
    [Tooltip("Is there a tool required to harvest this object?")]
    public bool requiresTool;
    public ItemData harvestTool;
    [Space]
    public ItemDrop[] harvestDrops;
}
