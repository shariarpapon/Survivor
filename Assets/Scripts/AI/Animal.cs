using UnityEngine;

[System.Serializable]
public class Animal
{
    public GameObject prefab;
    public int maxHerdSize;
    [Range(0, 1)]
    public float spawnThreshold;
    [Range(0, 1)]
    public float landRequirement;
}

