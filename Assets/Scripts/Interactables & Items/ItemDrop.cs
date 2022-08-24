using UnityEngine;

[System.Serializable]
public struct ItemDrop
{
    public ItemData itemData;
    [Range(0, 1)]
    public float chance;
}
