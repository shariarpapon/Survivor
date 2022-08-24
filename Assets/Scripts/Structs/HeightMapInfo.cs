using UnityEngine;

[System.Serializable]
public struct HeightMapInfo
{
    [Range(0, 1)]
    public float heightMultiplier;

    public float globalScale;
    public float globalFrequency;
    public float globalAmplitude;
    [Range(0, 1)]
    public float landFillThreshold;

    [Space]
    public bool applyFalloff;
    public float falloffSharpness;
    [Range(0, 1)]
    public float falloffShift;

    [Space]
    public int octaves;
    public float lacunarity;
    public float persistence;   
}
