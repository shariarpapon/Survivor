using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Survivor.Core;

public class WorldManager : MonoBehaviour, IInitializer
{
    #region Global Variables
    public const int CHUNK_DIMENSION = 16;
    public const int MAX_CHUNKS_VISIBLE_EACH_DIRECTION = 16;
    public const float MAX_VIEW_DISTANCE = 100;
    public const int VERTS_PER_CHUNK_EDGE = CHUNK_DIMENSION + 1;
    public static int TotalChunksPerAxis;
    public static bool WorldCreated = false;
    public static float MinWorldHeight = float.MaxValue;
    public static float MaxWorldHeight = float.MinValue;

    public static System.Random prng;
    public static Dictionary<Vector2, Chunk> chunkDictionary;
    public static Dictionary<BiomeType, Biome> biomeDictionary;
    public static GameObject waterGameObject;
    public static WorldManager Instance;

    #endregion Global Variables

    #region Public Variables

    [Header("References")]
    public Transform viewer;
    public Material waterMaterial;
    public ComputeShader worldCompute;

    [Header("World Settings")]
    public ChunkUpdateMode chunkUpdateMode;
    public int worldSize;
    [Range(1, 2)]
    public int mapGenerationMode;
    public string prngSeed;
    public float landAltitude = 2;
    public float waterLevel = 0;
    public float underwaterLandCutoff = -1.5f;
    public float colorNoiseScale;
    public Color shore;
    public TemperatureMapInfo temperatureMapInfo;
    public HeightMapInfo heightMapInfo;
    public SpawnSettings spawnSettings;
    public Biome[] biomes;

    #endregion Public Variables

    #region Private Variables
    private Gradient BiomeGradient;
    private System.Action ChunkUpdateMethod;

    private const int VISIBLE_CHUNKS_PER_AXIS = MAX_CHUNKS_VISIBLE_EACH_DIRECTION * 2;
    private const int VISIBLE_CHUNK_COUNT = VISIBLE_CHUNKS_PER_AXIS * VISIBLE_CHUNKS_PER_AXIS;
    private int[] ChunkMeshTriangles;
    #endregion Private Variables

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

        //Validate worldsize
        if (worldSize < CHUNK_DIMENSION) worldSize = CHUNK_DIMENSION;
        worldSize -= worldSize % CHUNK_DIMENSION;
        TotalChunksPerAxis = worldSize / CHUNK_DIMENSION;
        WorldCreated = false;

        MinWorldHeight = float.MaxValue;
        MaxWorldHeight = float.MinValue;
    }

    public IEnumerator Init()
    {
        //LOAD DATA
        if (GameManager.LoadDataFromExistingSave)
        {
            prngSeed = DataHandler.ReadData("prngSeed");
        }

        //Init random states / offsets
        prng = new System.Random(prngSeed.GetHashCode());
        Random.InitState(prngSeed.GetHashCode().GetHashCode());

        //Init varaiables
        chunkDictionary = new Dictionary<Vector2, Chunk>();
        biomeDictionary = new Dictionary<BiomeType, Biome>();
        activeChunks = new Queue<Vector2>();
        inactiveChunks = new Queue<Vector2>();
        lastActiveChunks = new Queue<Vector2>();
        GPU_ChunkDataArray = new GPU_ChunkData[VISIBLE_CHUNK_COUNT];
        BiomeGradient = new Gradient();
        ChunkMeshTriangles = GetChunkMeshTriangles();
        isChunkThreadRunning = false;

        //Assign biome gradient color keys
        GradientColorKey[] gradKeys = new GradientColorKey[biomes.Length];
        for (int i = 0; i < biomes.Length; i++) 
        {
            Biome  b = biomes[i];
            gradKeys[i] = new GradientColorKey(b.groundColor, b.temperatureThreshold);
            biomeDictionary.Add(b.type, b);
        }
        BiomeGradient.colorKeys = gradKeys;

        //Init compute shader values and chunk update method
        worldCompute.SetFloat("maxChunkViewDist", MAX_VIEW_DISTANCE); 
        worldCompute.SetFloat("chunkSize", CHUNK_DIMENSION);
        worldCompute.SetFloat("chunkExt", CHUNK_DIMENSION / 2);
        worldCompute.SetInt("chunksVisibleEachDirection", MAX_CHUNKS_VISIBLE_EACH_DIRECTION);
        worldCompute.SetInt("visibleChunkPerAxis", VISIBLE_CHUNKS_PER_AXIS);

        switch (chunkUpdateMode)
        {
            case ChunkUpdateMode.CPUThreading: ChunkUpdateMethod = new System.Action(UpdateChunk_CPU); break;
            case ChunkUpdateMode.GPUThreading: ChunkUpdateMethod = new System.Action(UpdateChunk_GPU); break;
        }

        //Start world generation
       yield return StartCoroutine(InitWorldGeneration());
    }

    private void FixedUpdate()
    {
        UpdateChunks();
    }

    private void UpdateChunks()
    {
        if (WorldCreated) ChunkUpdateMethod.Invoke();
    }

    private IEnumerator InitWorldGeneration()
    {
        Debug.Log("Generating land mesh...");
        yield return StartCoroutine(GenerateLand());

        Debug.Log("Generating water mesh...");
        yield return StartCoroutine(GenerateWater());

        Debug.Log("Generating natural features...");
        yield return StartCoroutine(GenerateNaturalFeatures());

        //TEST
        //Debug.Log("Generating npcs...");
        //yield return StartCoroutine(GenerateAnimals());

        Debug.Log("Generating essential features...");
        yield return StartCoroutine(GenerateEssentialFeatures());

        Debug.Log("Generating foliage...");
        yield return StartCoroutine(GenerateFoliage());

        WorldCreated = true;
        Debug.Log("<color=green>World Generation Complete!</color>");
    }

    private IEnumerator GenerateLand()
    {
        //Generate map data
        for (int coordX = 0; coordX < TotalChunksPerAxis; coordX++)
        {
            for (int coordY = 0; coordY < TotalChunksPerAxis; coordY++)
            {
                Vector2 relativeChunkCoords = new Vector2(coordX, coordY);
                Vector2 globalChunkCoords = relativeChunkCoords * CHUNK_DIMENSION;

                MapData mapData = MapGenerator.GenerateMapData(VERTS_PER_CHUNK_EDGE, prngSeed.GetHashCode(), globalChunkCoords, heightMapInfo, temperatureMapInfo, biomes, mapGenerationMode);
                Chunk chunk = new Chunk(relativeChunkCoords, globalChunkCoords, mapData);
                chunkDictionary.Add(relativeChunkCoords, chunk);
            }
            yield return null;
        }

        //Post world generation evaluation
        for (int coordX = 0; coordX < TotalChunksPerAxis; coordX++)
        {
            for (int coordY = 0; coordY < TotalChunksPerAxis; coordY++)
            {
                Chunk chunk = chunkDictionary[new Vector2(coordX, coordY)];
                MapGenerator.EvaluatePostGenerationData(chunk.mapData, heightMapInfo);

                Mesh chunkMesh = CreateChunkMesh(GenerateMeshData(chunk.mapData));
                chunk.mapData.SetColors(chunkMesh.colors);
                chunk.chunkGameObject.AddComponent<MeshFilter>().mesh = chunkMesh;
                chunk.chunkGameObject.AddComponent<MeshCollider>().sharedMesh = chunkMesh;
                chunk.chunkGameObject.AddComponent<MeshRenderer>().material = biomeDictionary[chunk.mapData.dominantBiome].material;
                chunk.chunkGameObject.transform.SetParent(transform);

                chunk.mapData.UpdateVertexMeshInfo(chunkMesh, chunk.globalPosition);
            }
            yield return null;
        }
    }

    //TEST
    private IEnumerator GenerateFoliage() 
    {
        if (generateGrass == true) 
            for (int chunkX = 0; chunkX < TotalChunksPerAxis; chunkX++)
                for (int chunkY = 0; chunkY < TotalChunksPerAxis; chunkY++)
                {
                    Chunk chunk = chunkDictionary[new Vector2(chunkX, chunkY)];
                    Texture2D grassVisibilityMap = new Texture2D(VERTS_PER_CHUNK_EDGE, VERTS_PER_CHUNK_EDGE);
                    bool genGrass = true;

                    //Vertex loops------------------------------------------------------------
                    for (int vertX = 0; vertX < VERTS_PER_CHUNK_EDGE; vertX++)
                        for (int vertY = 0; vertY < VERTS_PER_CHUNK_EDGE; vertY++)
                        {
                            VertexData vd = chunk.mapData.vertexData[vertX, vertY];

                            if (vd.biomeType != BiomeType.Grassland || vd.spaceValue != (int)SpaceType.Land) 
                            {
                                if(vd.spaceValue == (int)SpaceType.Water) genGrass = false;
                                grassVisibilityMap.SetPixel(vertX, vertY, Color.black);
                                continue; 
                            }

                            //Calculate grass noise
                            float heightNoise = vd.heightValue;
                            float sample_x = (vd.position.x / grassMapInfo.scale) * grassMapInfo.frequency;
                            float sample_y = (vd.position.z / grassMapInfo.scale) * grassMapInfo.frequency;
                            float noiseValue = Mathf.PerlinNoise(sample_x, sample_y) * grassMapInfo.amplitude;
                            grassVisibilityMap.SetPixel(vertX, vertY, Color.Lerp(Color.black, Color.white, noiseValue));
                        }

                    if (genGrass)
                    {
                        MeshRenderer mr = chunk.chunkGameObject.GetComponent<MeshRenderer>();
                        Material mainMat = mr.material;
                        Material[] mats = new Material[]
                        {
                        mainMat,
                        grassMaterial
                        };
                        mr.materials = mats;
                    }
                }
        

        yield return null;
    }

    private IEnumerator GenerateWater()
    {
        waterGameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterGameObject.name = "Water_Mesh";
        waterGameObject.layer = LayerMask.NameToLayer("Water");

        float size = (worldSize + 10) / 10;

        waterGameObject.transform.localScale = new Vector3(size, 1, size);
        waterGameObject.transform.position = new Vector3(worldSize / 2, waterLevel, worldSize / 2);
        waterGameObject.GetComponent<MeshRenderer>().material = waterMaterial;

        yield return null;
    }

    private IEnumerator GenerateNaturalFeatures()
    {
        int length = VERTS_PER_CHUNK_EDGE - 1;

        for (int coordX = 0; coordX < TotalChunksPerAxis; coordX++)
        {
            for (int coordY = 0; coordY < TotalChunksPerAxis; coordY++)
            {
                Chunk chunk = chunkDictionary[new Vector2(coordX, coordY)];

                for (int x = 0; x < VERTS_PER_CHUNK_EDGE; x++)
                    for (int y = 0; y < VERTS_PER_CHUNK_EDGE; y++)
                    {
                        if (x == 0 || x == length || y == 0 || y == length) continue;

                        Vector3 position = chunk.mapData.vertexData[x, y].position;
                        Vector3 normal = chunk.mapData.vertexData[x, y].normal;

                        #region Check Spawn Pre-Conditions

                        float angle = Vector3.Angle(normal, Vector3.up);
                        if (angle >= spawnSettings.maxFeatureSpawnAngle || position.y <= waterLevel) continue;

                        if ((float)prng.NextDouble() >= spawnSettings.naturalFeatureSpawnChance) continue;

                        #endregion

                        Biome biome = biomeDictionary[chunk.mapData.GetVertexBiomeType(x, y)];
                        WorldFeature feature = GetWorldFeature((float)prng.NextDouble(), biome.worldFeatures);

                        if (feature != null)
                        {
                            float randPosOffsetX = Random.Range(-spawnSettings.maxPositionOffset, spawnSettings.maxPositionOffset);
                            float randPosOffsetY = Random.Range(-spawnSettings.maxPositionOffset, spawnSettings.maxPositionOffset);

                            Vector3 pos = new Vector3(x + chunk.globalPosition.x + randPosOffsetX, position.y, y + chunk.globalPosition.y + randPosOffsetY);
                            GameObject inst = InstantiateChunkObject(feature.prefab, pos, Quaternion.identity, chunk);

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
        //Generate at most 1 animal per chunk.
        //[TODO] Make it so we can spawn animals from a range of 0 - maxHerdSize per chunk.

        for (int coordX = 0; coordX < TotalChunksPerAxis; coordX++)
            for (int coordY = 0; coordY < TotalChunksPerAxis; coordY++)
            {
                Chunk chunk = chunkDictionary[new Vector2(coordX, coordY)];
                Biome biome = biomeDictionary[chunk.mapData.dominantBiome];

                float decider = (float)prng.NextDouble();
                Animal animal = GetAnimal(decider, biome.animals);

                if (animal != null)
                {
                    Vector3 pos = new Vector3(chunk.globalPosition.x, landAltitude, chunk.globalPosition.y);
                    InstantiateChunkObject(animal.prefab, pos, GetRandomYRotation(), chunk);
                }
            }
        yield return null;
    }

    private IEnumerator GenerateEssentialFeatures() 
    {
        yield return null;
    }

    private Mesh CreateChunkMesh(ChunkMeshData meshData)
    {
        Mesh mesh = new Mesh { vertices = meshData.vertices, triangles = ChunkMeshTriangles, colors = meshData.colors };
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        return mesh;
    }

    private ChunkMeshData GenerateMeshData(MapData mapData)
    {
        //NOTE: A worldsize of 4x4 grid will have vertices 5x5
        Color[] colors = new Color[VERTS_PER_CHUNK_EDGE * VERTS_PER_CHUNK_EDGE];
        Vector3[] verts = new Vector3[VERTS_PER_CHUNK_EDGE * VERTS_PER_CHUNK_EDGE];

        //Set vertex height and color
        for (int i = 0, x = 0; x < VERTS_PER_CHUNK_EDGE; x++)
            for (int y = 0; y < VERTS_PER_CHUNK_EDGE; y++, i++)
            {
                float minNoiseHeight = mapData.vertexData[x, y].heightValue;
                float maxInfluenceHeight = minNoiseHeight * landAltitude;
                float height = Mathf.Lerp(minNoiseHeight, maxInfluenceHeight, heightMapInfo.heightMultiplier);

                verts[i] = new Vector3(x, height, y);
                colors[i] = GetVertexColor(x, y, height, mapData);
                mapData.SetVertexSpaceType(x, y, height <= waterLevel ? SpaceType.Water : SpaceType.Land);
            }

        return new ChunkMeshData(verts, colors);
    }

    private Color GetVertexColor(int x, int y, float height, MapData mapData)
    {
        Color landColor = BiomeGradient.Evaluate(mapData.vertexData[x, y].temperatureValue);
        float t = Mathf.Lerp(0, 1, height / heightMapInfo.landFillThreshold);
        return Color.Lerp(shore, landColor, t);
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

    public GameObject InstantiateChunkObject(GameObject obj, Vector3 pos, Quaternion rot, Chunk chunk) 
    {
        GameObject inst = Instantiate(obj, pos, rot, chunk.chunkGameObject.transform);
        inst.AddComponent<ChunkObject>().AssignChunk(chunk);
        return inst;
    }

    public GameObject InstantiateChunkObject(GameObject obj, Vector3 pos, Quaternion rot)
    {
        Chunk chunk = GetClosestChunk(pos);
        GameObject inst = Instantiate(obj, pos, rot, chunk.chunkGameObject.transform);
        inst.AddComponent<ChunkObject>().AssignChunk(chunk);
        return inst;
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

    private Animal GetAnimal(float decider, Animal[] npcs) 
    {
        for (int i = 0; i < npcs.Length; i++)
        {
            Animal npc = npcs[i];
            if (decider <= npc.spawnThreshold) return npc;
        }
        return null;
    }

    private Quaternion GetRandomYRotation() 
    {
        return Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);
    }

    public static Vector2 GetVerifiedChunkCoord(Vector2 coord)
    {
        Vector2 chunkCoord = new Vector2(Mathf.FloorToInt(coord.x / CHUNK_DIMENSION), Mathf.FloorToInt(coord.y / CHUNK_DIMENSION));
        chunkCoord.x = Mathf.Clamp(chunkCoord.x, 0, TotalChunksPerAxis - 1);
        chunkCoord.y = Mathf.Clamp(chunkCoord.y, 0, TotalChunksPerAxis - 1);
        return chunkCoord;
    }

    public static Vector2 GetVerifiedChunkCoord(Vector3 worldPos) 
    {
        Vector2 chunkCoord = new Vector2(Mathf.RoundToInt(worldPos.x / CHUNK_DIMENSION), Mathf.RoundToInt(worldPos.z / CHUNK_DIMENSION));
        chunkCoord.x = Mathf.Clamp(chunkCoord.x, 0, TotalChunksPerAxis - 1);
        chunkCoord.y = Mathf.Clamp(chunkCoord.y, 0, TotalChunksPerAxis - 1);
        return chunkCoord;
    }

    public static Chunk GetClosestChunk(Vector3 worldPos)
    {
        return chunkDictionary[GetVerifiedChunkCoord(worldPos)];
    }

    public static Chunk GetClosestChunk(Vector2 worldPos) 
    {
        return chunkDictionary[GetVerifiedChunkCoord(worldPos)];
    }

    public static Vector2Int WorldToVertexCoord(Vector3 worldPos) 
    {
        return new Vector2Int(Mathf.RoundToInt(worldPos.x % VERTS_PER_CHUNK_EDGE), Mathf.RoundToInt(worldPos.z % VERTS_PER_CHUNK_EDGE));
    }

    #region CPU Chunk Update

    private bool isChunkThreadRunning = false;
    private Queue<Vector2> activeChunks;
    private Queue<Vector2> inactiveChunks;
    private Queue<Vector2> lastActiveChunks;

    private void UpdateChunk_CPU()
    {
        if (activeChunks.Count > 0) while (activeChunks.Count > 0) chunkDictionary[activeChunks.Dequeue()].SetVisible(true);
        if (inactiveChunks.Count > 0) while (inactiveChunks.Count > 0) chunkDictionary[inactiveChunks.Dequeue()].SetVisible(false);

        if (isChunkThreadRunning == false)
        {
            isChunkThreadRunning = true;
            Vector2 chunkCoord = new Vector2(Mathf.FloorToInt(viewer.position.x / CHUNK_DIMENSION), Mathf.FloorToInt(viewer.position.z / CHUNK_DIMENSION));
            CPU_ChunkData data = new CPU_ChunkData(MAX_CHUNKS_VISIBLE_EACH_DIRECTION, viewer.position, chunkCoord);
            ThreadStart threadStart = new ThreadStart(delegate { UpdateChunkUpdateCPUThread(data); });
            new Thread(threadStart).Start();
        }
    }

    private void UpdateChunkUpdateCPUThread(CPU_ChunkData data)
    {
        lock (chunkDictionary) lock (activeChunks) lock (inactiveChunks)
                {
                    while (lastActiveChunks.Count > 0)
                    {
                        Vector2 coord = lastActiveChunks.Dequeue();
                        Chunk chunk = chunkDictionary[coord];
                        if (!chunk.IsChunkVisible(data.viewerPos))
                            inactiveChunks.Enqueue(coord);
                    }

                    for (int x = -data.chunksVisible; x < data.chunksVisible; x++)
                        for (int y = -data.chunksVisible; y < data.chunksVisible; y++)
                        {
                            Vector2 targetChunkCoord = new Vector2(data.viewerChunkCoord.x + x, data.viewerChunkCoord.y + y);
                            if (chunkDictionary.ContainsKey(targetChunkCoord))
                            {
                                Chunk chunk = chunkDictionary[targetChunkCoord];
                                if (chunk.IsChunkVisible(data.viewerPos))
                                {
                                    if (chunk.isVisible == false) activeChunks.Enqueue(targetChunkCoord);
                                    lastActiveChunks.Enqueue(targetChunkCoord);
                                }
                            }
                        }
                }
        isChunkThreadRunning = false;
    }
    #endregion

    #region GPU Chunk Update
    private const int CHUNK_UPDATE_THREAD_COUNT = 16;
    private const int CHUNK_UPDATE_THREADGROUPS = VISIBLE_CHUNKS_PER_AXIS / CHUNK_UPDATE_THREAD_COUNT;
    private const int GPU_CHUNKDATA_BYTESIZE = sizeof(float) * 2 + sizeof(int);
    private GPU_ChunkData[] GPU_ChunkDataArray;

    private void UpdateChunk_GPU()
    {
        ComputeBuffer dataBuffer = new ComputeBuffer(VISIBLE_CHUNK_COUNT, GPU_CHUNKDATA_BYTESIZE);
        dataBuffer.SetData(GPU_ChunkDataArray);

        worldCompute.SetBuffer(0, "ChunkDataArray", dataBuffer);
        worldCompute.SetVector("viewerPosition", viewer.position);

        //Check the previously visible chunks with new viewPoint
        worldCompute.Dispatch(0, CHUNK_UPDATE_THREADGROUPS, CHUNK_UPDATE_THREADGROUPS, 1);
        dataBuffer.GetData(GPU_ChunkDataArray);
        CheckGPUChunkData();

        //Check visible chunks with updated chunk coords
        Vector2 viewerChunkCoord = GetVerifiedChunkCoord(viewer.position);
        worldCompute.SetInt("viewerChunkCoordX", (int)viewerChunkCoord.x);
        worldCompute.SetInt("viewerChunkCoordY", (int)viewerChunkCoord.y);

        worldCompute.Dispatch(0, CHUNK_UPDATE_THREADGROUPS, CHUNK_UPDATE_THREADGROUPS, 1);
        dataBuffer.GetData(GPU_ChunkDataArray);
        dataBuffer.Release();

        CheckGPUChunkData();
    }

    private void CheckGPUChunkData()
    {
        foreach (GPU_ChunkData data in GPU_ChunkDataArray)
        {
            if (chunkDictionary.ContainsKey(data.coord))
            {
                if (data.setActive == 1)
                    chunkDictionary[data.coord].SetVisible(true);
                else chunkDictionary[data.coord].SetVisible(false);
            }
        }
    }
    #endregion

    private struct CPU_ChunkData
    {
        public int chunksVisible;
        public Vector3 viewerPos;
        public Vector2 viewerChunkCoord;

        public CPU_ChunkData(int chunksVisible, Vector3 viewerPos, Vector2 viewerChunkCoord)
        {
            this.chunksVisible = chunksVisible;
            this.viewerPos = viewerPos;
            this.viewerChunkCoord = viewerChunkCoord;
        }
    }

    private struct GPU_ChunkData
    {
        public Vector2 coord;
        public int setActive;
    }

    private struct ChunkMeshData
    {
        public readonly Vector3[] vertices;
        public readonly Color[] colors;

        public ChunkMeshData(Vector3[] vertices, Color[] colors)
        {
            this.vertices = vertices;
            this.colors = colors;
        }
    }

    //TEST
    [System.Serializable]
    public struct GrassMapInfo
    {
        public float scale;
        public float frequency;
        public float amplitude;
        [Range(0, 1)]
        public float grassSpawnThreshold;
    }

    //TEST
    [Space]
    public bool generateGrass = false;
    public GrassMapInfo grassMapInfo;
    public Material grassMaterial;

    [System.Serializable]
    public enum ChunkUpdateMode
    {
        CPUThreading,
        GPUThreading
    }
}





