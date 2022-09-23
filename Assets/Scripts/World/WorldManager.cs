using System.Collections;
using UnityEngine;

namespace Survivor.WorldManagement
{
    public class WorldManager : MonoBehaviour, IInitializer
    {
        #region Global Variables
        public const int CHUNK_DIMENSION = 16;
        public const int VERTS_PER_CHUNK_EDGE = CHUNK_DIMENSION + 1;
        public static WorldManager Instance;
        #endregion Global Variables

        [SerializeField] private WorldSettings overworldSettings;
        [SerializeField] private Transform worldViewer;
        private int[] ChunkMeshTriangles;

        public World overworld;

        private void Awake()
        {
            #region Singleton
            if (Instance == null) Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
            #endregion

            ChunkMeshTriangles = GetChunkMeshTriangles();
        }


        private void FixedUpdate() 
        {
            overworld?.UpdateChunkVisibility();
        }

        public IEnumerator Init()
        {
            Random.InitState(overworldSettings.GetHashCode().GetHashCode());                        

            //Start world generation

            overworld = new World(overworldSettings);
            yield return StartCoroutine(CreateWorld(overworld));
        }

        public IEnumerator CreateWorld(World world)
        {
            //create world
            Debug.Log("Generating land mesh...");
            yield return StartCoroutine(GenerateLand(world));

            Debug.Log("Generating water mesh...");
            yield return StartCoroutine(GenerateWater(world));

            Debug.Log("Generating natural features...");
            yield return StartCoroutine(GenerateNaturalFeatures(world));

            Debug.Log("Generating natural animals...");
            yield return StartCoroutine(GenerateAnimals());

            Debug.Log("Generating essential features...");
            yield return StartCoroutine(GenerateEssentialFeatures());

            Debug.Log("Generating foliage...");
            yield return StartCoroutine(GenerateFoliage(world));

            world.FinalizeWorldGeneration(worldViewer);
            Debug.Log("<color=green>World Generation Complete!</color>");
        }

        private IEnumerator GenerateLand(World world)
        {
            for (int localChunkX = 0; localChunkX < world.totalChunksPerAxis; localChunkX++)
            {
                for (int localChunkY = 0; localChunkY < world.totalChunksPerAxis; localChunkY++)
                {
                    Vector2 relativeChunkCoords = new Vector2(localChunkX, localChunkY);
                    Vector2 globalChunkCoords = relativeChunkCoords * CHUNK_DIMENSION;

                    MapData mapData = WorldDataGenerator.GenerateWorldChunkMapData(world.settings.seed, globalChunkCoords, 
                                                       world.settings.heightMapInfo, world.settings.temperatureMapInfo, world.settings.biomes, world);
                    Chunk chunk = new Chunk(relativeChunkCoords, globalChunkCoords, mapData);
                    world.chunkDictionary.Add(relativeChunkCoords, chunk);
                }
                yield return null;
            }

            //Evaluate post-generatation mapdata and generate mesh
            CreateMeshAndCollider(world);
        }

        private IEnumerator GenerateFoliage(World world)
        {
            yield return null;
        }

        private IEnumerator GenerateWater(World world)
        {
            world.waterGameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            world.waterGameObject.name = "Water_Mesh";
            world.waterGameObject.layer = LayerMask.NameToLayer("Water");

            float size = (overworldSettings.worldSize + 10) / 10;

            world.waterGameObject.transform.localScale = new Vector3(size, 1, size);
            world.waterGameObject.transform.position = new Vector3(overworldSettings.worldSize / 2, overworldSettings.waterLevel, overworldSettings.worldSize / 2);
            world.waterGameObject.GetComponent<MeshRenderer>().material = overworldSettings.waterMaterial;

            yield return null;
        }

        private IEnumerator GenerateNaturalFeatures(World world)
        {
            int length = VERTS_PER_CHUNK_EDGE - 1;
            for (int coordX = 0; coordX < world.totalChunksPerAxis; coordX++)
            {
                for (int coordY = 0; coordY < world.totalChunksPerAxis; coordY++)
                {
                    Chunk chunk = world.chunkDictionary[new Vector2(coordX, coordY)];

                    for (int x = 0; x < VERTS_PER_CHUNK_EDGE; x++)
                        for (int y = 0; y < VERTS_PER_CHUNK_EDGE; y++)
                        {
                            if (x == 0 || x == length || y == 0 || y == length) continue;

                            Vector3 position = chunk.mapData.vertexData[x, y].position;
                            Vector3 normal = chunk.mapData.vertexData[x, y].normal;

                            #region Check Spawn Pre-Conditions

                            float angle = Vector3.Angle(normal, Vector3.up);
                            if (angle >= world.settings.spawnSettings.maxFeatureSpawnAngle || position.y <= world.settings.waterLevel) continue;

                            if ((float)world.prng.NextDouble() >= world.settings.spawnSettings.naturalFeatureSpawnChance) continue;

                            #endregion

                            Biome biome = world.biomeDictionary[chunk.mapData.GetVertexBiomeType(x, y)];
                            WorldFeature feature = GetWorldFeature((float)world.prng.NextDouble(), biome.worldFeatures);

                            if (feature != null)
                            {
                                float randPosOffsetX = Random.Range(-world.settings.spawnSettings.maxPositionOffset, world.settings.spawnSettings.maxPositionOffset);
                                float randPosOffsetY = Random.Range(-world.settings.spawnSettings.maxPositionOffset, world.settings.spawnSettings.maxPositionOffset);

                                Vector3 pos = new Vector3(x + chunk.globalPosition.x + randPosOffsetX, position.y, y + chunk.globalPosition.y + randPosOffsetY);
                                GameObject inst = chunk.Instantiate(feature.prefab, pos, Quaternion.identity);

                                inst.name = "Feature_" + pos;
                                inst.transform.up = normal;
                                inst.transform.Rotate(new Vector3(0, Random.Range(0, 360), 0), Space.Self);

                                chunk.mapData.SetVertexSpaceType(x, y, SpaceType.Occupied);
                            }
                        }
                }
                yield return null;
            }
        }

        private IEnumerator GenerateAnimals()
        {
            yield return null;
        }

        private IEnumerator GenerateEssentialFeatures()
        {
            yield return null; //650, -300 mc elijah
        }

        private void CreateMeshAndCollider(World world) 
        {
            for (int coordX = 0; coordX < world.totalChunksPerAxis; coordX++)
                for (int coordY = 0; coordY < world.totalChunksPerAxis; coordY++)
                {
                    Chunk chunk = world.chunkDictionary[new Vector2(coordX, coordY)];
                    EvaluatePostGenerationData(chunk.mapData, world.settings.heightMapInfo, world);
                    Mesh chunkMesh = GenerateChunkMesh(GenerateMeshData(chunk.mapData, world));
                    chunk.mapData.SetColors(chunkMesh.colors);
                    chunk.chunkGameObject.AddComponent<MeshFilter>().mesh = chunkMesh;
                    chunk.chunkGameObject.AddComponent<MeshCollider>().sharedMesh = chunkMesh;
                    chunk.chunkGameObject.AddComponent<MeshRenderer>().material = world.biomeDictionary[chunk.mapData.dominantBiome].material;
                    chunk.chunkGameObject.transform.SetParent(world.worldGameObject.transform);
                    chunk.mapData.UpdateVertexMeshInfo(chunkMesh, chunk.globalPosition);
                }
        }

        private Mesh GenerateChunkMesh(ChunkMeshData meshData)
        {
            Mesh mesh = new Mesh { vertices = meshData.vertices, triangles = ChunkMeshTriangles, colors = meshData.colors };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }

        private ChunkMeshData GenerateMeshData(MapData mapData, World world)
        {
            //NOTE: A worldsize of 4x4 grid will have vertices 5x5
            Color[] colors = new Color[VERTS_PER_CHUNK_EDGE * VERTS_PER_CHUNK_EDGE];
            Vector3[] verts = new Vector3[VERTS_PER_CHUNK_EDGE * VERTS_PER_CHUNK_EDGE];

            //Set vertex height and color
            for (int i = 0, x = 0; x < VERTS_PER_CHUNK_EDGE; x++)
                for (int y = 0; y < VERTS_PER_CHUNK_EDGE; y++, i++)
                {
                    float minNoiseHeight = mapData.vertexData[x, y].heightValue;
                    float maxInfluenceHeight = minNoiseHeight * world.settings.landAltitude;
                    float height = Mathf.Lerp(minNoiseHeight, maxInfluenceHeight, world.settings.heightMapInfo.heightMultiplier);

                    verts[i] = new Vector3(x, height, y);
                    colors[i] = world.EvaluateVertexColor(x, y, height, mapData);
                    mapData.SetVertexSpaceType(x, y, height <= world.settings.waterLevel ? SpaceType.Water : SpaceType.Land);
                }

            return new ChunkMeshData(verts, colors);
        }

        private void EvaluatePostGenerationData(MapData mapData, HeightMapInfo heightMapInfo, World world)
        { 
            float radius = world.totalChunksPerAxis * VERTS_PER_CHUNK_EDGE / 2;
            for (int x = 0; x < mapData.vertexData.GetLength(0); x++)
                for (int y = 0; y < mapData.vertexData.GetLength(1); y++)
                {
                    float heightValue = Mathf.InverseLerp(world.minHeight, world.maxHeight, mapData.vertexData[x, y].heightValue) * 2 - 1;

                    if (heightMapInfo.applyFalloff)
                        heightValue -= WorldDataGenerator.CalculateRadialFalloff(new Vector2(mapData.chunkPosition.x + x, mapData.chunkPosition.y + y), radius, heightMapInfo.falloffSharpness, heightMapInfo.falloffShift);

                    mapData.vertexData[x, y].heightValue = heightValue;
                }
        }

        private WorldFeature GetWorldFeature(float decider, WorldFeature[] features)
        {
            for (int i = 0; i < features.Length; i++)
            {
                WorldFeature feature = features[i];
                if (decider <= feature.spawnValue) return feature;
            }
            return null;
        }

        private int[] GetChunkMeshTriangles()
        {
            int[] tris = new int[CHUNK_DIMENSION * CHUNK_DIMENSION * 6];
            int t = 0, v = 0;
            for (int x = 0; x < CHUNK_DIMENSION; x++)
            {
                for (int y = 0; y < CHUNK_DIMENSION; y++)
                {
                    tris[t] = v + 1;
                    tris[t + 1] = v + CHUNK_DIMENSION + 1;
                    tris[t + 2] = v + 0;
                    tris[t + 3] = v + CHUNK_DIMENSION + 2;
                    tris[t + 4] = v + CHUNK_DIMENSION + 1;
                    tris[t + 5] = v + 1;
                    t += 6;
                    v++;
                }
                v++;
            }
            return tris;
        }
    }
}





