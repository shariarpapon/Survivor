using UnityEngine;
using Survivor.WorldManagement;

public class ChunkObject : MonoBehaviour
{
    private Chunk assignedChunk;

    public void AssignChunk(Chunk chunk) 
    {
        assignedChunk = chunk;
    }

    private void OnDestroy() 
    {
        assignedChunk.destroyedChildren.Add(transform.name);
    }
}
