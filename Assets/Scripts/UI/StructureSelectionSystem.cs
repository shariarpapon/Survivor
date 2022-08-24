using UnityEngine;

public class StructureSelectionSystem : MonoBehaviour
{
    public SelectableStructure[] structures;
    public GameObject[] frontPanels;
    public Transform selectionBorder;

    private int currentSelectionIndex = 0;
    private int previousSelectionIndex;

    private void OnEnable() { foreach (GameObject g in frontPanels) g.SetActive(false); }
    private void OnDisable() { foreach (GameObject g in frontPanels) g.SetActive(true); }

    private void Start() 
    {
        Select(currentSelectionIndex);
    }

    private void Update()
    {
        currentSelectionIndex = Mathf.Clamp((int)-Input.mouseScrollDelta.y + currentSelectionIndex, 0, structures.Length - 1);
        if(previousSelectionIndex != currentSelectionIndex) Select(currentSelectionIndex);
    }


    public void Select(int index) 
    {
        selectionBorder.transform.position = structures[index].iconTransform.position;
        previousSelectionIndex = index;
        BuildingManager.Instance.SetStructurePreview(structures[index].structureRecipe);
    }

    [System.Serializable]
    public struct SelectableStructure
    {
        public Transform iconTransform;
        public CraftingRecipe structureRecipe;
    }
}
