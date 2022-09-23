using UnityEngine;

namespace Survivor.WorldManagement
{
    public struct ChunkMeshData
    {
        public readonly Vector3[] vertices;
        public readonly Color[] colors;

        public ChunkMeshData(Vector3[] vertices, Color[] colors)
        {
            this.vertices = vertices;
            this.colors = colors;
        }
    }
}