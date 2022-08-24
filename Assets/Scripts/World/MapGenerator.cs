using System.Collections.Generic;
using UnityEngine;

public class MapGenerator
{
    public static MapData GenerateMapData(int size, int seed, Vector2 globalOffset, HeightMapInfo heightMapInfo, TemperatureMapInfo temperatureMapInfo, Biome[] biomes, int mode)
    {
        #region Initialization

        System.Random prng = new System.Random(seed);
        Vector2 noiseOffset = new Vector2(prng.Next(-999999, 999999), prng.Next(-999999, 999999));        

        //clamp denominators
        heightMapInfo.globalScale = heightMapInfo.globalScale <= 0 ? 0.001f : heightMapInfo.globalScale;
        temperatureMapInfo.scale = temperatureMapInfo.scale <= 0 ? 0.001f : temperatureMapInfo.scale;

        //noise offsets
        Vector2 globalNoiseOffset = globalOffset + noiseOffset; //TEST
        Vector2 temperatureNoiseOffset = globalOffset + (2 * noiseOffset); //TEST

        float worldEdge = WorldManager.TotalChunksPerAxis * WorldManager.VERTS_PER_CHUNK_EDGE;

        VertexData[,] vertexData = new VertexData[size, size];
        Dictionary<BiomeType, int> biomeVertices = new Dictionary<BiomeType, int>();

        #endregion

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float globalY = globalOffset.y + y;

                #region Height/Temperature Values

                float heightValue = 0;
                if (mode == 1)
                    heightValue = CalculateHeightValueMode1(new Vector2(x, y), globalNoiseOffset, heightMapInfo);
                else if(mode == 2)
                    heightValue = CalculateHeightValueMode2(new Vector2(x, y), globalNoiseOffset, heightMapInfo);

                if (heightValue > WorldManager.MaxWorldHeight) WorldManager.MaxWorldHeight = heightValue;
                else if (heightValue < WorldManager.MinWorldHeight) WorldManager.MinWorldHeight = heightValue;

                float temperature = CalculateTemperatureValueMode1(new Vector2(x, y), globalY, worldEdge, temperatureNoiseOffset, temperatureMapInfo);
                
                #endregion

                #region Assign Vertex Biome

                int biomeIndex = 0; 
                Biome biome = biomes[biomeIndex]; 

                for (int i = 1; i < biomes.Length; i++)
                    if (temperature >= biomes[i].temperatureThreshold)
                        biome = biomes[i];

                if (biomeVertices.ContainsKey(biome.type)) biomeVertices[biome.type]++;
                else biomeVertices.Add(biome.type, 1);

                #endregion

                vertexData[x, y] = new VertexData(heightValue, temperature, biome.type);
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

    public static void EvaluatePostGenerationData(MapData mapData, HeightMapInfo heightMapInfo) 
    {
        float radius = WorldManager.TotalChunksPerAxis * WorldManager.VERTS_PER_CHUNK_EDGE / 2;
        for (int x = 0; x < mapData.vertexData.GetLength(0); x++)
            for (int y = 0; y < mapData.vertexData.GetLength(1); y++)
            {
                float heightValue = Mathf.InverseLerp(WorldManager.MinWorldHeight, WorldManager.MaxWorldHeight, mapData.vertexData[x, y].heightValue) * 2 - 1;
                
                if(heightMapInfo.applyFalloff)
                    heightValue -= CalculateRadialFalloff(new Vector2(mapData.chunkPosition.x + x, mapData.chunkPosition.y + y), radius, heightMapInfo.falloffSharpness, heightMapInfo.falloffShift);
                
                mapData.vertexData[x, y].heightValue = heightValue;
            }
    }

    private static float CalculateRadialFalloff(Vector2 vertexPosition, float radius, float sharpness, float shift)
    {
        Vector2 center = new Vector2(radius, radius);
        float dist = Vector2.Distance(center, vertexPosition);
        float t = dist / radius;

        //Apply sigmoid function
        float n = Mathf.Pow(GameUtility.E, -sharpness * (t - shift));
        float falloff = 1 / (1 + n);

        return falloff;
    }

    private static float CalculateHeightValueMode1(Vector2 relativeCoord, Vector2 globalNoiseOffset, HeightMapInfo info) 
    {
        float sampleX = (relativeCoord.x + globalNoiseOffset.x) / info.globalScale * info.globalFrequency;
        float sampleY = (relativeCoord.y + globalNoiseOffset.y) / info.globalScale * info.globalFrequency;
        float noiseValue = Mathf.PerlinNoise(sampleX, sampleY);
        return (noiseValue * 2 - 1) * info.globalAmplitude;
    }

    private static float CalculateHeightValueMode2(Vector2 relativeCoord, Vector2 globalNoiseOFfset, HeightMapInfo info) 
    {
        float noiseValue = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < info.octaves; i++) 
        {
            float sampleX = (relativeCoord.x + globalNoiseOFfset.x) / info.globalScale * info.globalFrequency * frequency;
            float sampleY = (relativeCoord.y + globalNoiseOFfset.y) / info.globalScale * info.globalFrequency * frequency;
            float perlinValue = Mathf.PerlinNoise(sampleX ,sampleY);

            noiseValue += perlinValue * amplitude;
            amplitude *= info.persistence;
            frequency *= info.lacunarity;
        }

        return (noiseValue) * info.globalAmplitude;
    }

    private static float CalculateTemperatureValueMode1(Vector2 relativeCoord, float globalY, float worldEdge, Vector2 temperatureNoiseOffset, TemperatureMapInfo info) 
    {
        float sx = (relativeCoord.x + temperatureNoiseOffset.x) / info.scale;
        float sy = (relativeCoord.y + temperatureNoiseOffset.y) / info.scale;
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

    public BiomeType GetVertexBiomeType(int x, int y) 
    {
        return vertexData[x, y].biomeType;
    }

    public void SetColors(Color[] colors) 
    {
        this.colors = colors;
    }

    public void SetVertexSpaceType(int x, int y, SpaceType spaceType) 
    {
        vertexData[x, y].spaceValue = (int)spaceType;
    }

    public SpaceType GetVertexSpaceType(int x, int y) 
    {
        return (SpaceType)vertexData[x, y].spaceValue;
    }

    public void UpdateVertexMeshInfo(Mesh mesh, Vector2 globalOffset) 
    {
        for (int x = 0, i = 0; x < vertexData.GetLength(0); x++)
            for (int y = 0; y < vertexData.GetLength(1); y++, i++)
                vertexData[x, y].SetVertexMeshInfo(mesh.vertices[i] + new Vector3(globalOffset.x, 0, globalOffset.y), mesh.normals[i]);
    }
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
