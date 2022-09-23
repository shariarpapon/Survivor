using System.Collections.Generic;
using UnityEngine;

namespace Survivor.WorldManagement
{
    public class World
    {
        public System.Random prng;
        public WorldSettings settings;
        public Dictionary<Vector2, Chunk> chunkDictionary;
        public Dictionary<BiomeType, Biome> biomeDictionary;
        public GameObject waterGameObject;
        public GameObject worldGameObject;
        public Gradient biomeGradient;

        public float minHeight;
        public float maxHeight;
        public bool isCreated;
        public int totalChunksPerAxis;

        private ChunkVisibilityUpdater chunkVisibilityUpdater;

        public World(WorldSettings settings)
        {
            this.settings = settings;
            chunkDictionary = new Dictionary<Vector2, Chunk>();
            biomeDictionary = new Dictionary<BiomeType, Biome>();
            worldGameObject = new GameObject($"world_{settings.seed}");
            EvaluateBiomeGradient();

            prng = new System.Random(settings.seed.GetHashCode());
            minHeight = float.MaxValue;
            maxHeight = float.MinValue;
            isCreated = false;

            if (settings.worldSize < WorldManager.CHUNK_DIMENSION) settings.worldSize = WorldManager.CHUNK_DIMENSION;
            settings.worldSize -= settings.worldSize % WorldManager.CHUNK_DIMENSION;
            totalChunksPerAxis = settings.worldSize / WorldManager.CHUNK_DIMENSION;
        }

        private void EvaluateBiomeGradient()
        {
            biomeGradient = new Gradient();
            GradientColorKey[] gradKeys = new GradientColorKey[settings.biomes.Length];
            for (int i = 0; i < settings.biomes.Length; i++)
            {
                Biome b = settings.biomes[i];
                gradKeys[i] = new GradientColorKey(b.groundColor, b.temperatureThreshold);
                biomeDictionary.Add(b.type, b);
            }
            biomeGradient.colorKeys = gradKeys;
        }

        public void UpdateChunkVisibility() 
        {
            if (!isCreated) return;
            chunkVisibilityUpdater.UpdateChunks();
        }

        public float GetTemperatureAtPosition(Vector3 position) 
        {
            Chunk chunk = GetClosestChunk(position);
            int localChunkX = (int)position.x % WorldManager.CHUNK_DIMENSION;
            int localChunkY = (int)position.z % WorldManager.CHUNK_DIMENSION;
            float envTemp = 1 - chunk.mapData.vertexData[localChunkX, localChunkY].temperatureValue;
            return envTemp;
        }

        public Vector2 GetVerifiedChunkCoord(Vector2 coord)
        {
            Vector2 chunkCoord = new Vector2(Mathf.FloorToInt(coord.x / WorldManager.CHUNK_DIMENSION), Mathf.FloorToInt(coord.y / WorldManager.CHUNK_DIMENSION));
            chunkCoord.x = Mathf.Clamp(chunkCoord.x, 0, totalChunksPerAxis - 1);
            chunkCoord.y = Mathf.Clamp(chunkCoord.y, 0, totalChunksPerAxis - 1);
            return chunkCoord;
        }

        public Vector2 GetVerifiedChunkCoord(Vector3 worldPos)
        {
            Vector2 chunkCoord = new Vector2(Mathf.RoundToInt(worldPos.x / WorldManager.CHUNK_DIMENSION), Mathf.RoundToInt(worldPos.z / WorldManager.CHUNK_DIMENSION));
            chunkCoord.x = Mathf.Clamp(chunkCoord.x, 0, totalChunksPerAxis - 1);
            chunkCoord.y = Mathf.Clamp(chunkCoord.y, 0, totalChunksPerAxis - 1);
            return chunkCoord;
        }

        public Color EvaluateVertexColor(int x, int y, float height, MapData mapData)
        {
            Color landColor = biomeGradient.Evaluate(mapData.vertexData[x, y].temperatureValue);
            float t = Mathf.Lerp(0, 1, height / settings.heightMapInfo.landFillThreshold);
            return Color.Lerp(settings.shoreColor, landColor, t);
        }

        public void FinalizeWorldGeneration(Transform worldViewer) 
        {
            chunkVisibilityUpdater = new ChunkVisibilityUpdater(this, worldViewer);
            isCreated = true;
        }

        public GameObject InstantiateInChunk(GameObject obj, Vector3 position, Quaternion rotataion)
            => GetClosestChunk(position).Instantiate(obj, position, rotataion);
        public Chunk GetClosestChunk(Vector3 worldPos) => chunkDictionary[GetVerifiedChunkCoord(worldPos)];
        public Chunk GetClosestChunk(Vector2 worldPos) => chunkDictionary[GetVerifiedChunkCoord(worldPos)];
        public static Vector2Int WorldToVertexCoord(Vector3 worldPos)
            => new Vector2Int(Mathf.RoundToInt(worldPos.x % WorldManager.VERTS_PER_CHUNK_EDGE), Mathf.RoundToInt(worldPos.z % WorldManager.VERTS_PER_CHUNK_EDGE));
    }
}
