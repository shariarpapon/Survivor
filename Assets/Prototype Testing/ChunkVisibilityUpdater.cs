using UnityEngine;

namespace Survivor.WorldManagement
{
    public class ChunkVisibilityUpdater
    {
        #region  CONSTS
        public const int MAX_CHUNKS_VISIBLE_EACH_DIRECTION = 16;
        public const float MAX_VIEW_DISTANCE = 100;
        public const int VISIBLE_CHUNKS_PER_AXIS = MAX_CHUNKS_VISIBLE_EACH_DIRECTION * 2;
        public const int VISIBLE_CHUNK_COUNT = VISIBLE_CHUNKS_PER_AXIS * VISIBLE_CHUNKS_PER_AXIS;

        private const int CHUNK_UPDATE_THREAD_COUNT = 16;
        private const int CHUNK_UPDATE_THREADGROUPS = VISIBLE_CHUNKS_PER_AXIS / CHUNK_UPDATE_THREAD_COUNT;
        private const int GPU_CHUNKDATA_BYTESIZE = sizeof(float) * 2 + sizeof(int);
        #endregion

        private ComputeShader chunkUpdateComputer;
        private Transform viewer;
        private World world;
        private GPU_ChunkData[] GPU_ChunkDataArray;

        public ChunkVisibilityUpdater(World world, Transform viewer)
        {
            this.world = world;
            this.viewer = viewer;
            this.chunkUpdateComputer = world.settings.chunkVisbilityComputer;
            InitlizeComputerVariables();
        }

        private void InitlizeComputerVariables() 
        {
            chunkUpdateComputer.SetFloat("maxChunkViewDist", MAX_VIEW_DISTANCE);
            chunkUpdateComputer.SetFloat("chunkSize", WorldManager.CHUNK_DIMENSION);
            chunkUpdateComputer.SetFloat("chunkExt", WorldManager.CHUNK_DIMENSION / 2);
            chunkUpdateComputer.SetInt("chunksVisibleEachDirection", MAX_CHUNKS_VISIBLE_EACH_DIRECTION);
            chunkUpdateComputer.SetInt("visibleChunkPerAxis", VISIBLE_CHUNKS_PER_AXIS);
            GPU_ChunkDataArray = new GPU_ChunkData[VISIBLE_CHUNK_COUNT];
        }

        //Update logics
        public void UpdateChunks()
        {
            ComputeBuffer dataBuffer = new ComputeBuffer(VISIBLE_CHUNK_COUNT, GPU_CHUNKDATA_BYTESIZE);
            dataBuffer.SetData(GPU_ChunkDataArray);

            chunkUpdateComputer.SetBuffer(0, "ChunkDataArray", dataBuffer);
            chunkUpdateComputer.SetVector("viewerPosition", viewer.position);

            //Check the previously visible chunks with new viewPoint
            chunkUpdateComputer.Dispatch(0, CHUNK_UPDATE_THREADGROUPS, CHUNK_UPDATE_THREADGROUPS, 1);
            dataBuffer.GetData(GPU_ChunkDataArray);
            CheckGPUChunkData();

            //Check visible chunks with updated chunk coords
            Vector2 viewerChunkCoord = world.GetVerifiedChunkCoord(viewer.position);
            chunkUpdateComputer.SetInt("viewerChunkCoordX", (int)viewerChunkCoord.x);
            chunkUpdateComputer.SetInt("viewerChunkCoordY", (int)viewerChunkCoord.y);

            chunkUpdateComputer.Dispatch(0, CHUNK_UPDATE_THREADGROUPS, CHUNK_UPDATE_THREADGROUPS, 1);
            dataBuffer.GetData(GPU_ChunkDataArray);
            dataBuffer.Dispose();

            CheckGPUChunkData();
        }

        private void CheckGPUChunkData()
        {
            foreach (GPU_ChunkData data in GPU_ChunkDataArray)
            {
                if (world.chunkDictionary.ContainsKey(data.coord))
                {
                    if (data.setActive == 1)
                        world.chunkDictionary[data.coord].SetVisible(true);
                    else world.chunkDictionary[data.coord].SetVisible(false);
                }
            }
        }

        private struct GPU_ChunkData
        {
            public Vector2 coord;
            public int setActive;
        }
    }

}
