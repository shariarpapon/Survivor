using UnityEngine;

namespace Survivor.WorldManagement
{
    [CreateAssetMenu(fileName = "World Generation Settings", menuName = "Scriptableobjects/World Generation Settings")]
    public class WorldSettings : ScriptableObject
    {
        public string seed;
        [Tooltip("Rounds down to the nearest multiple of the chunk dimension")]
        public int worldSize;
        public float landAltitude = 2;
        public float waterLevel = 0;
        public float colorNoiseScale;
        public Material waterMaterial;
        public ComputeShader chunkVisbilityComputer;
        public Color shoreColor;
        public TemperatureMapInfo temperatureMapInfo;
        public HeightMapInfo heightMapInfo;
        public SpawnSettings spawnSettings;
        public Biome[] biomes;
    }

}
