using System;
using UnityEngine;
using Survivor.WorldManagement;

namespace Survivor.Core
{
    public class BuildingManager : MonoBehaviour
    {
        public static BuildingManager Instance { get; private set; }
        public static event Action OnStructureBuildModeEnter;
        public static event Action OnItemBuildModeEnter;
        public static event Action OnBuildModeExit;
        public static bool InBuildMode = false;

        [SerializeField] private float buildRange = 5;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private LayerMask groundLayer;

        private Action currentBuildMethod;
        private GameObject structure;
        private ItemSlot buildItemSlot;
        private GameObject preview;
        private Interactor playerInteractor;
        private CraftingRecipe currentStructureRecipe;
        private Camera playerCamera;
        private bool canBuild;

        private void Awake()
        {
            #region Singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else 
            {
                Destroy(gameObject);
                return;
            }
            #endregion

            playerInteractor = FindObjectOfType<Interactor>();
            playerCamera = Camera.main;
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
                RotatePreviewOnUserInput();
                if (Input.GetMouseButtonDown(0)) currentBuildMethod?.Invoke();
            }
        }

        private void RotatePreviewOnUserInput() 
        {
            if (Input.GetKeyDown(KeyCode.Q)) preview.transform.eulerAngles += new Vector3(0, 45, 0);
            if (Input.GetKeyDown(KeyCode.E)) preview.transform.eulerAngles += new Vector3(0, -45, 0);
        }

        private void PreviewBuild()
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
            if (Physics.Raycast(ray, out RaycastHit hit, 300, groundLayer))
            {
                preview.transform.position = hit.point;
                Vector3 axis = Vector3.Cross(preview.transform.up, hit.normal).normalized;
                float angle = Vector3.SignedAngle(preview.transform.up, hit.normal, axis);
                preview.transform.Rotate(axis, angle);
                canBuild = InBuildRange(hit);
            }
        }

        public void EnterStructureBuildMode()
        {
            ExitBuildMode();
            InGameUIManager.Instance.InstantCloseTab();
            currentBuildMethod = BuildStructure;
            InGameUIManager.Instance.InstantCloseTab();
            InBuildMode = true;
            OnStructureBuildModeEnter?.Invoke();
        }

        public void EnterItemBuildMode(ItemSlot slot)
        {
            ExitBuildMode();
            InGameUIManager.Instance.InstantCloseTab();
            buildItemSlot = slot;
            currentBuildMethod = BuildItem;
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
            currentBuildMethod = null;
            OnBuildModeExit?.Invoke();
        }

        public void BuildStructure()
        {
            if (currentStructureRecipe == null || !canBuild) return;
            if (CraftingManager.Instance.TryRemoveStructureCraftMaterials(currentStructureRecipe))
            {
                Chunk chunk = WorldManager.Instance.overworld.GetClosestChunk(preview.transform.position);
                GameObject inst = chunk.Instantiate(currentStructureRecipe.craftedMaterial.itemData.prefab, preview.transform.position, preview.transform.rotation);
                StartCoroutine(GameUtility.TweenScaleBell(inst, 1.0f, 1.125f, 40));
                //On structure created events here
            }
        }

        public void BuildItem()
        {
            if (!canBuild || buildItemSlot == null) return;
            Chunk chunk = WorldManager.Instance.overworld.GetClosestChunk(preview.transform.position);
            GameObject inst = chunk.Instantiate(currentStructureRecipe.craftedMaterial.itemData.prefab, preview.transform.position, preview.transform.rotation);
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

        private bool InBuildRange(RaycastHit hit)
        {
            return Vector3.Distance(hit.point, playerInteractor.transform.position) <= buildRange;
        }
    }
}
