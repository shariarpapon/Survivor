using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }
    public static bool InBuildMode = false;

    public static event System.Action OnStructureBuildModeEnter;
    public static event System.Action OnItemBuildModeEnter;
    public static event System.Action OnBuildModeExit;

    [SerializeField] private float buildRange = 5;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask groundLayer;

    [HideInInspector] public GameObject structure;
    private System.Action buildMethod;
    private ItemSlot buildItemSlot;
    private GameObject preview;
    private Interactor playerInteractor;
    private CraftingRecipe currentStructureRecipe;
    private bool canBuild;

    private void Awake() 
    {
        if (Instance == null) Instance = this;
        else return;

        playerInteractor = FindObjectOfType<Interactor>();
    }

    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Escape) && InBuildMode) ExitBuildMode();
        if (Input.GetKeyDown(KeyCode.B)) 
        {
            if (InBuildMode) ExitBuildMode();
            else EnterStructureBuildMode();
        }

        if (InBuildMode && preview != null) 
        { 
            PreviewBuild();
            if (Input.GetMouseButtonDown(0)) buildMethod?.Invoke();
            if (Input.GetKeyDown(KeyCode.Q)) preview.transform.eulerAngles += new Vector3(0, 45, 0);
            if (Input.GetKeyDown(KeyCode.E)) preview.transform.eulerAngles += new Vector3(0, -45, 0);
        }
    }

    private void PreviewBuild() 
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));

        if (Physics.Raycast(ray, out RaycastHit hit, 300, groundLayer))
        {
            preview.transform.position = hit.point;

            //Rotation
            Vector3 axis = Vector3.Cross(preview.transform.up, hit.normal).normalized;
            float angle = Vector3.SignedAngle(preview.transform.up, hit.normal, axis);
            preview.transform.Rotate(axis, angle);
            //preview.transform.up = hit.normal;
            canBuild = CanBuild(hit);
        }
    }

    public void EnterStructureBuildMode()
    {
        ExitBuildMode();
        
        InGameUIManager.Instance.InstantCloseTab();

        buildMethod = new System.Action(BuildStructure);

        InGameUIManager.Instance.InstantCloseTab();
        InBuildMode = true;
        OnStructureBuildModeEnter?.Invoke();
    }

    public void EnterItemBuildMode(ItemSlot slot) 
    {
        ExitBuildMode();

        InGameUIManager.Instance.InstantCloseTab();

        buildItemSlot = slot;
        buildMethod = new System.Action(BuildItem);

        CreatePreview(buildItemSlot.slotItem.itemData.prefab);

        InGameUIManager.Instance.InstantCloseTab();
        InBuildMode = true;
        OnItemBuildModeEnter?.Invoke();
    }

    public void ExitBuildMode() 
    {
        buildItemSlot = null;
        canBuild = false;
        Destroy(preview);

        structure = null;
        InBuildMode = false;
        currentStructureRecipe = null;
        buildMethod = null;
        OnBuildModeExit?.Invoke();
    }

    public void BuildStructure() 
    {
        if (currentStructureRecipe == null || !canBuild) return;
        if (CraftingManager.Instance.TryRemoveStructureCraftMaterials(currentStructureRecipe))
        {
            GameObject inst = WorldManager.Instance.InstantiateChunkObject(currentStructureRecipe.craftedMaterial.itemData.prefab, preview.transform.position, preview.transform.rotation);
            StartCoroutine(GameUtility.TweenScaleBell(inst, 1.0f, 1.125f, 40));
            //On structure created events here
        }
    }

    public void BuildItem() 
    {
        if (!canBuild || buildItemSlot == null) return;
        GameObject inst = WorldManager.Instance.InstantiateChunkObject(buildItemSlot.slotItem.itemData.prefab, preview.transform.position, preview.transform.rotation);
        StartCoroutine(GameUtility.TweenScaleBell(inst, 1.0f, 1.125f, 40));        
        //On item created events here

        buildItemSlot.Remove(false);
        ExitBuildMode();
    }

    public void SetStructurePreview(CraftingRecipe structureRecipe) 
    {
        currentStructureRecipe = structureRecipe;
        structure = structureRecipe.craftedMaterial.itemData.prefab;
        CreatePreview(structure);
    }

    private void CreatePreview(GameObject prefab) 
    {
        if (prefab == null) 
        { 
            Debug.Log("No Buildable Object Selected");
            ExitBuildMode();
            return;
        }

        Destroy(preview);
        preview = Instantiate(prefab);

        if (preview.TryGetComponent(out PlaceableItem placeable))
            Destroy(placeable);
        if (preview.TryGetComponent(out Collider collider))
            Destroy(collider);
    }

    private bool CanBuild(RaycastHit hit) 
    {
        return Vector3.Distance(hit.point, playerInteractor.transform.position) <= buildRange;
    }
}
