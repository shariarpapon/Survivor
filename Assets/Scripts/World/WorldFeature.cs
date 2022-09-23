using UnityEngine;

namespace Survivor.WorldManagement
{
    [System.Serializable]
    public class WorldFeature
    {
        public GameObject prefab;
        [Range(0, 1)]
        public float spawnValue;
    }
}