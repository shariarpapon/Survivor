using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST_DEBUGGING : MonoBehaviour
{
    public bool debug = false;

    public GameObject grassPrefab;
    public MeshFilter groundMeshFilter;
    public MeshFilter grassMeshFilter;
    public Material grassMat;

    public int density = 1;
    public int size = 10;
    public float grassBladeScale;
    public Quaternion[,] rotations;

    private void Awake()
    {
        debug = false;
        if (!debug) return;
        rotations = new Quaternion[(size+1) * density, (size+1) * density];
        Random.InitState(System.DateTime.Now.Millisecond.GetHashCode());
        for (int x = 0; x <= size * density; x++)
            for (int y = 0; y <= size * density; y++)
            {
                rotations[x, y] = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }
    }

    public void Update()
    {
        if (!debug) return;
        Execute();
    }

    private void Execute() 
    {
        Mesh grassMesh = grassMeshFilter.sharedMesh;
        Vector3 ext = new Vector3(size /2 , 0, size /2 );

        for (int x = 0; x <= size * density; x++)
            for (int y = 0; y <= size * density; y++) 
            {
                Vector3 pos = (new Vector3(x, 0, y) * (1.0f/density)) - ext + groundMeshFilter.transform.position;
                Quaternion rot = rotations[x, y];
                Vector3 scl = new Vector3(grassBladeScale, 1, grassBladeScale);
                Matrix4x4 matrix = Matrix4x4.TRS(pos, rot, scl);
                Graphics.DrawMesh(grassMesh, matrix, grassMat, 1);

            }
    }
}
