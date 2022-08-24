using UnityEngine;

[System.Serializable]
public struct SpawnSettings 
{
    [Header("Natural Features")]
    [Range(0, 360)]
    public float maxFeatureSpawnAngle;
    [Range(0, 1)]
    public float naturalFeatureSpawnChance;
    [Range(0, 1)]
    public float maxPositionOffset;
}