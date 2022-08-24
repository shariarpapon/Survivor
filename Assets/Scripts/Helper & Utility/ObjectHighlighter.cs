using UnityEngine;

public class ObjectHighlighter : MonoBehaviour
{
    public static ObjectHighlighter Instance { get; private set; }

    public Material outliner;
    private GameObject currentTarget;

    private void Awake() 
    {
        if (Instance == null) Instance = this;
    }

    public void HighlightObject(GameObject obj) 
    {
        UnhighlightObject(currentTarget);
        currentTarget = obj;

        MeshRenderer rend = obj.GetComponent<MeshRenderer>();
        if (rend == null) return;

        Material[] mats = new Material[]
        {
            rend.materials[0],
            outliner
        };
        rend.materials = mats;
    }

    public void UnhighlightObject(GameObject obj) 
    {
        if (obj == null && currentTarget == null) return;

        MeshRenderer rend = currentTarget.GetComponent<MeshRenderer>();
        if (rend == null) return;

        Material[] mats = new Material[] { rend.materials[0] };
        rend.materials = mats;
    }
}
