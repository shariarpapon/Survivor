using UnityEngine;
using System.Collections.Generic;

namespace Survivor.WorldManagement
{

    public class Chunk
    {
        public Vector2 globalPosition;
        public Vector2 relativePosition;
        public bool isVisible;

        public GameObject chunkGameObject;
        public MapData mapData;

        public Bounds bound;

        public List<string> destroyedChildren;
        public Dictionary<string, Vector2> neighboringChunks;

        public Chunk(Vector2 relativePosition, Vector2 globalPosition, MapData mapData)
        {
            this.mapData = mapData;
            this.relativePosition = relativePosition;
            this.globalPosition = globalPosition;

            SetBounds();
            CreateChunk();
            SetNeighbors();
        }

        private void CreateChunk()
        {
            chunkGameObject = new GameObject("Chunk_" + globalPosition.ToString());
            chunkGameObject.transform.tag = "Ground";
            chunkGameObject.layer = LayerMask.NameToLayer("Ground");
            chunkGameObject.transform.position = new Vector3(globalPosition.x, 0, globalPosition.y);
            destroyedChildren = new List<string>();
            SetVisible(false);
        }

        private void SetNeighbors()
        {
            neighboringChunks = new Dictionary<string, Vector2>();
            neighboringChunks.Add("top", new Vector2(relativePosition.x, relativePosition.y + 1));
            neighboringChunks.Add("bot", new Vector2(relativePosition.x, relativePosition.y - 1));
            neighboringChunks.Add("right", new Vector2(relativePosition.x + 1, relativePosition.y));
            neighboringChunks.Add("left", new Vector2(relativePosition.x - 1, relativePosition.y));
            neighboringChunks.Add("topRight", new Vector2(relativePosition.x + 1, relativePosition.y + 1));
            neighboringChunks.Add("topLeft", new Vector2(relativePosition.x - 1, relativePosition.y + 1));
            neighboringChunks.Add("botRight", new Vector2(relativePosition.x + 1, relativePosition.y - 1));
            neighboringChunks.Add("botLeft", new Vector2(relativePosition.x - 1, relativePosition.y - 1));
        }

        private void SetBounds()
        {
            bound = new Bounds
            {
                center = new Vector3(relativePosition.x * WorldManager.CHUNK_DIMENSION + WorldManager.CHUNK_DIMENSION / 2, 0, relativePosition.y * WorldManager.CHUNK_DIMENSION + WorldManager.CHUNK_DIMENSION / 2),
                size = Vector3.one * WorldManager.CHUNK_DIMENSION
            };
        }

        public void SetVisible(bool visible)
        {
            isVisible = visible;
            chunkGameObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return chunkGameObject.activeSelf;
        }

        public void UpdateVertexColor(int i, Color color)
        {
            chunkGameObject.GetComponent<MeshFilter>().mesh.colors[i] = color;
            mapData.colors[i] = color;
        }

        public void UpdateChunkColors(Color[] colors)
        {
            chunkGameObject.GetComponent<MeshFilter>().mesh.SetColors(colors);
            mapData.SetColors(colors);
        }

        public GameObject Instantiate(GameObject obj, Vector3 pos, Quaternion rot)
        {
            GameObject inst =  GameObject.Instantiate(obj, pos, rot, chunkGameObject.transform);
            inst.AddComponent<ChunkObject>().AssignChunk(this);
            return inst;
        }
    }

}