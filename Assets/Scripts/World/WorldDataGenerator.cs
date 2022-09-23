using System.Collections.Generic;
using UnityEngine;

namespace Survivor.WorldManagement
{
    public class WorldDataGenerator
    {
        public static MapData GenerateWorldChunkMapData(string seed, Vector2 globalOffset, HeightMapInfo heightMapInfo, TemperatureMapInfo temperatureMapInfo, Biome[] biomes, World world)
        {
            #region Initialization
            const int noiseRange = 999999;
            System.Random prng = new System.Random(seed.GetHashCode());

            //noise offsets
            Vector2 noiseOffset = new Vector2(prng.Next(-noiseRange, noiseRange), prng.Next(-noiseRange, noiseRange));
            Vector2 globalNoiseOffset = globalOffset + noiseOffset;
            Vector2 temperatureNoiseOffset = globalOffset + (2 * noiseOffset);

            //clamp denominators
            heightMapInfo.globalScale = heightMapInfo.globalScale <= 0 ? 0.001f : heightMapInfo.globalScale;
            temperatureMapInfo.scale = temperatureMapInfo.scale <= 0 ? 0.001f : temperatureMapInfo.scale;

            float worldEdge = world.totalChunksPerAxis * WorldManager.VERTS_PER_CHUNK_EDGE;

            VertexData[,] vertexData = new VertexData[WorldManager.VERTS_PER_CHUNK_EDGE, WorldManager.VERTS_PER_CHUNK_EDGE];
            Dictionary<BiomeType, int> biomeVertices = new Dictionary<BiomeType, int>();
            #endregion

            for (int x = 0; x < WorldManager.VERTS_PER_CHUNK_EDGE; x++)
                for (int y = 0; y < WorldManager.VERTS_PER_CHUNK_EDGE; y++)
                {
                    float globalY = globalOffset.y + y;

                    #region Height/Temperature Values

                    float heightValue = CalculateHeightValue(new Vector2(x, y), globalNoiseOffset, heightMapInfo);

                    if (heightValue > world.maxHeight) world.maxHeight = heightValue;
                    else if (heightValue < world.minHeight) world.minHeight = heightValue;

                    float temperatureValue = CalculateTemperatureValue(new Vector2(x, y), globalY, worldEdge, temperatureNoiseOffset, temperatureMapInfo);

                    #endregion

                    #region Assign Vertex Biome

                    int biomeIndex = 0;
                    Biome biome = biomes[biomeIndex];

                    for (int i = 1; i < biomes.Length; i++)
                        if (temperatureValue >= biomes[i].temperatureThreshold)
                            biome = biomes[i];

                    if (biomeVertices.ContainsKey(biome.type)) biomeVertices[biome.type]++;
                    else biomeVertices.Add(biome.type, 1);

                    #endregion

                    vertexData[x, y] = new VertexData(heightValue, temperatureValue, biome.type);
                }

            #region Evaluate Dominant Biome

            BiomeType dominantBiome = BiomeType.Grassland;
            int maxBiomeVerts = int.MinValue;
            foreach (Biome b in biomes)
                if (biomeVertices.ContainsKey(b.type))
                    if (biomeVertices[b.type] > maxBiomeVerts)
                    {
                        dominantBiome = b.type;
                        maxBiomeVerts = biomeVertices[b.type];
                    }

            #endregion

            return new MapData(vertexData, globalOffset, dominantBiome);
        }

        public static float CalculateRadialFalloff(Vector2 vertexPosition, float radius, float sharpness, float shift)
        {
            Vector2 center = new Vector2(radius, radius);
            float dist = Vector2.Distance(center, vertexPosition);
            float t = dist / radius;

            //Apply sigmoid function
            float n = Mathf.Pow(GameUtility.E, -sharpness * (t - shift));
            float falloff = 1 / (1 + n);

            return falloff;
        }

        private static float CalculateHeightValue(Vector2 relativeCoord, Vector2 noiseOffset, HeightMapInfo info)
        {
            float noiseValue = 0;
            float amplitude = 1;
            float frequency = 1;

            for (int i = 0; i < info.octaves; i++)
            {
                float sampleX = (relativeCoord.x + noiseOffset.x) / info.globalScale * info.globalFrequency * frequency;
                float sampleY = (relativeCoord.y + noiseOffset.y) / info.globalScale * info.globalFrequency * frequency;
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);

                noiseValue += perlinValue * amplitude;
                amplitude *= info.persistence;
                frequency *= info.lacunarity;
            }

            return noiseValue * info.globalAmplitude;
        }

        private static float CalculateTemperatureValue(Vector2 relativeCoord, float globalY, float worldEdge, Vector2 noiseOffset, TemperatureMapInfo info)
        {
            float sx = (relativeCoord.x + noiseOffset.x) / info.scale;
            float sy = (relativeCoord.y + noiseOffset.y) / info.scale;
            float perlinValue = Mathf.PerlinNoise(sx * info.frequency, sy * info.frequency) * info.amplitude;
            float temperature = (globalY / worldEdge) + Mathf.Lerp(info.minOffset, info.maxOffset, perlinValue);
            return temperature;
        }
    }

    public class MapData
    {
        public readonly VertexData[,] vertexData;
        public readonly Vector2 chunkPosition;
        public readonly BiomeType dominantBiome;
        public Color[] colors;

        public MapData(VertexData[,] vertexData, Vector2 chunkPosition, BiomeType dominantBiome)
        {
            this.vertexData = vertexData;
            this.chunkPosition = chunkPosition;
            this.dominantBiome = dominantBiome;
        }

        public void UpdateVertexMeshInfo(Mesh mesh, Vector2 globalOffset)
        {
            for (int x = 0, i = 0; x < vertexData.GetLength(0); x++)
                for (int y = 0; y < vertexData.GetLength(1); y++, i++)
                    vertexData[x, y].SetVertexMeshInfo(mesh.vertices[i] + new Vector3(globalOffset.x, 0, globalOffset.y), mesh.normals[i]);
        }

        public BiomeType GetVertexBiomeType(int x, int y) => vertexData[x, y].biomeType;
        public void SetColors(Color[] colors) => this.colors = colors;
        public void SetVertexSpaceType(int x, int y, SpaceType spaceType) => vertexData[x, y].spaceValue = (int)spaceType;
        public SpaceType GetVertexSpaceType(int x, int y) => (SpaceType)vertexData[x, y].spaceValue;
    }

    public class VertexData
    {
        public float heightValue;
        public int spaceValue;
        public readonly float temperatureValue;
        public readonly BiomeType biomeType;

        public Vector3 position;
        public Vector3 normal;

        public VertexData(float heightValue, float temperatureValue, BiomeType biomeType)
        {
            this.heightValue = heightValue;
            this.temperatureValue = temperatureValue;
            this.biomeType = biomeType;
        }

        public void SetVertexMeshInfo(Vector3 position, Vector3 normal)
        {
            this.position = position;
            this.normal = normal;
        }
    }


}
