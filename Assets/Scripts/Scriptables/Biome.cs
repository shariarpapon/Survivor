using UnityEngine;

[CreateAssetMenu(fileName = "New Biome", menuName = "Scriptableobjects/Biome")]
public class Biome : ScriptableObject
{
    public BiomeType type;
    public Material material;
    public Color groundColor;
    [Range(0, 1)]
    public float temperatureThreshold;
    public Animal[] animals;
    public WorldFeature[] worldFeatures;
}
