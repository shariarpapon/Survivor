using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Survivor.Core
{
    public class InGameUIManager : MonoBehaviour, IInitializer
    {
        public static InGameUIManager Instance { get; private set; }

        #region Inspector Variables

        [Header("Utility UI")]
        [SerializeField] private GameObject utilityTab;
        [SerializeField] private GameObject _itemOptionsUI;
        [SerializeField] private GameObject _recipeMenuButton;
        [SerializeField] private GameObject _recipeMaterialSlot;
        [SerializeField] private GameObject _insufficientMaterialStatus;
        [SerializeField] private GameObject structureBuildModePanel;
        [SerializeField] private GameObject itemBuildModePanel;

        [Header("Player UI")]
        [SerializeField] private Image healthBar;
        [SerializeField] private Image energyBar;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private TextMeshProUGUI temperatureText;
        [SerializeField] private Image thermometerBorder;
        [SerializeField] private Image thermometerBar;
        [SerializeField] private Gradient temperatureGradient;

        [Header("Loading UI")]
        [SerializeField] private TextMeshProUGUI loadingStatus;
        [SerializeField] private Image loadingBar;

        #endregion

        #region Hidden Variables

        private Transform _recipeListContent;
        private Transform _recipeMaterialContent;
        private TextMeshProUGUI _craftedItemCount;
        private Image _craftedSlotIcon;
        private TextMeshProUGUI _craftedItemName;

        private Button _craftButton;
        private GameObject craftingPanel;
        private GameObject inventoryPanel;
        private string[] recipeNames;
        private Button itemUseButton;
        private Button itemRemoveButton;
        private CraftingRecipe selectedRecipe;
        private Interactor interactor;
        private Player player;
        private Vector3 itemOptionOffset;

        #endregion

        private void OnEnable()
        {
            GameManager.OnComponentInitialized += UpdateInitializationProgress;
            GameManager.OnGameInitilizationComplete += DestroyLoadingScreen;
            BuildingManager.OnItemBuildModeEnter += OpenItemBuildPanel;
            BuildingManager.OnStructureBuildModeEnter += OpenStructureBuildPanel;
            BuildingManager.OnBuildModeExit += CloseBuildPanel;
        }

        private void OnDisable()
        {
            GameManager.OnComponentInitialized -= UpdateInitializationProgress;
            GameManager.OnGameInitilizationComplete -= DestroyLoadingScreen;
            BuildingManager.OnItemBuildModeEnter -= OpenItemBuildPanel;
            BuildingManager.OnStructureBuildModeEnter -= OpenStructureBuildPanel;
            BuildingManager.OnBuildModeExit -= CloseBuildPanel;
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public IEnumerator Init()
        {
            //Get ui object references
            Transform frame = utilityTab.transform.Find("Frame");
            craftingPanel = frame.Find("Crafting Panel").gameObject;
            inventoryPanel = frame.Find("Inventory Panel").gameObject;
            _itemOptionsUI = Instantiate(_itemOptionsUI, frame.root);

            _recipeListContent = craftingPanel.transform.Find("Recipe List").Find("Recipe ScrollView").Find("Viewport").Find("Recipe List Content");
            _recipeMaterialContent = craftingPanel.transform.Find("Craft Mat");

            Transform craftSlotHolder = craftingPanel.transform.Find("Craft Tab").Find("Craft Slot Holder");
            _craftedSlotIcon = craftSlotHolder.Find("Craft Slot").Find("_Icon").GetComponent<Image>();
            _craftedItemCount = craftSlotHolder.Find("Craft Slot").Find("_Count").GetComponent<TextMeshProUGUI>();
            _craftedItemName = craftSlotHolder.Find("Craft Name").GetComponent<TextMeshProUGUI>();
            _craftButton = craftSlotHolder.Find("Craft Button").GetComponent<Button>();

            interactor = FindObjectOfType<Interactor>();
            player = FindObjectOfType<Player>();

            itemUseButton = _itemOptionsUI.transform.GetChild(0).GetComponent<Button>();
            itemRemoveButton = _itemOptionsUI.transform.GetChild(1).GetComponent<Button>();

            Rect rect = _itemOptionsUI.GetComponent<RectTransform>().rect;
            itemOptionOffset = new Vector3(rect.width / 2, -rect.height / 2, 0);

            var recipes = CraftingManager.Instance.craftingRecipes;
            recipeNames = new string[recipes.Count]; //Cache an array for the names of the recipes for easier searches later
            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe recipe = recipes[i];
                Button button = Instantiate(_recipeMenuButton, _recipeListContent).GetComponent<Button>();
                button.onClick.AddListener(delegate { OnRecipeButtonClick(recipe); });
                button.GetComponentInChildren<TextMeshProUGUI>().text = recipe.craftedMaterial.itemData.name;
                recipeNames[i] = recipe.craftedMaterial.itemData.name;
            }

            //Initialize object active states
            selectedRecipe = null;
            _itemOptionsUI.SetActive(false);
            _craftedSlotIcon.gameObject.SetActive(false);
            _insufficientMaterialStatus.SetActive(false);
            inventoryPanel.SetActive(true);
            craftingPanel.SetActive(false);
            utilityTab.SetActive(false);
            yield return null;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab) && !BuildingManager.InBuildMode)
            {
                if (utilityTab.activeSelf) CloseInventoryTab();
                else OpenInventoryTab();
            }
        }

        private void LateUpdate()
        {
            if (!GameManager.IsGameInitialized) return;
            UpdateTemperatureDisplay();
        }

        private void OpenStructureBuildPanel()
        {
            if (CommandManager.IsCommandWindowOpen) return;

            CloseBuildPanel();
            structureBuildModePanel.SetActive(true);
        }

        private void OpenItemBuildPanel()
        {
            if (CommandManager.IsCommandWindowOpen) return;

            CloseBuildPanel();
            itemBuildModePanel.SetActive(true);
        }

        private void CloseBuildPanel()
        {
            itemBuildModePanel.SetActive(false);
            structureBuildModePanel.SetActive(false);
        }

        private void UpdateInitializationProgress(string status, float progress)
        {
            loadingStatus.text = status;
            loadingBar.fillAmount = progress;
        }

        private void DestroyLoadingScreen()
        {
            StartCoroutine(GameUtility.TweenUISlideOutLeft(loadingBar.transform.parent.gameObject, 45, true));
        }

        private void UpdateTemperatureDisplay()
        {
            thermometerBar.fillAmount = player.vitals.temperature.value;
            thermometerBar.color = temperatureGradient.Evaluate(player.vitals.temperature.value);
            thermometerBorder.color = temperatureGradient.Evaluate(player.vitals.temperature.value);
            temperatureText.text = Mathf.RoundToInt(player.vitals.temperature.value * 100).ToString() + "\u00B0";
        }

        public void UpdatePlayerUI()
        {
            healthBar.fillAmount = player.vitals.HealthPercentage;
            healthText.text = $"Health {Mathf.Floor(healthBar.fillAmount * 100)}%";

            energyBar.fillAmount = player.vitals.EnergyPercentage;
            energyText.text = $"Energy {Mathf.Floor(energyBar.fillAmount * 100)}%";
        }

        public void OpenInventoryTab()
        {
            PlayerController.SetCursor(CursorLockMode.Confined, true);
            if (selectedRecipe != null) OnRecipeButtonClick(selectedRecipe);

            _itemOptionsUI.SetActive(false);

            StartCoroutine(GameUtility.TweenUIFadeIn(utilityTab, 40));
        }

        public void CloseInventoryTab()
        {
            PlayerController.SetCursor(CursorLockMode.Locked, false);

            _itemOptionsUI.SetActive(false);

            StartCoroutine(GameUtility.TweenUIFadeOut(utilityTab, 40, false));
        }

        public void InstantCloseTab()
        {
            PlayerController.SetCursor(CursorLockMode.Locked, false);
            _itemOptionsUI.SetActive(false);
            utilityTab.SetActive(false);
        }

        public void SearchRecipe(TMP_InputField input)
        {
            if (string.IsNullOrEmpty(input.text)) return;

            int index = GameUtility.SearchStringArray(input.text, recipeNames);
            if (index != -1)
            {
                string previousFocus = recipeNames[0];
                recipeNames[0] = recipeNames[index];
                recipeNames[index] = previousFocus;
                _recipeListContent.GetChild(index).SetAsFirstSibling();
            }
        }

        public void OpenCraftingPanel()
        {
            craftingPanel.SetActive(true);
            inventoryPanel.SetActive(false);
            _itemOptionsUI.SetActive(false);
        }

        public void OpenInventoryPanel()
        {
            inventoryPanel.SetActive(true);
            craftingPanel.SetActive(false);
            _itemOptionsUI.SetActive(false);
        }

        private void OnRecipeButtonClick(CraftingRecipe recipe)
        {
            if (recipe == null) return;

            selectedRecipe = recipe;
            for (int i = 0; i < _recipeMaterialContent.childCount; i++) Destroy(_recipeMaterialContent.GetChild(i).gameObject);

            _craftedSlotIcon.sprite = recipe.craftedMaterial.itemData.icon;
            _craftedItemCount.text = recipe.craftedMaterial.amount.ToString();
            _craftedItemName.text = recipe.craftedMaterial.itemData.name;
            _craftedSlotIcon.gameObject.SetActive(true);

            bool insufficientItems = false;
            foreach (CraftingMaterial mat in recipe.craftingMaterials)
            {
                var _matSlot = Instantiate(_recipeMaterialSlot, _recipeMaterialContent).transform;
                _matSlot.Find("Icon").GetComponent<Image>().sprite = mat.itemData.icon;
                _matSlot.Find("Count").GetComponent<TextMeshProUGUI>().text = mat.amount.ToString();

                if (interactor.inventory.ContainsItem(mat.itemData, mat.amount)) _matSlot.Find("Border").gameObject.SetActive(false);
                else if (!insufficientItems)
                {
                    insufficientItems = true;
                    _matSlot.Find("Border").gameObject.SetActive(true);
                }
            }

            if (insufficientItems == false) _insufficientMaterialStatus.SetActive(false);
            else _insufficientMaterialStatus.SetActive(true);
        }

        public void OnCraftButtonClicked()
        {
            if (selectedRecipe != null)
                CraftingManager.Instance.CraftItem(selectedRecipe);

            StartCoroutine(GameUtility.TweenScaleBell(_craftButton.gameObject, 1, 1.2f, 25));
            OnRecipeButtonClick(selectedRecipe);
        }

        public void DisplayItemOptions(ItemSlot slot)
        {
            if (!slot || slot.IsEmpty) return;

            if (!_itemOptionsUI.activeSelf)
            {
                itemUseButton.onClick.RemoveAllListeners();
                itemUseButton.onClick.AddListener(delegate { OnUseItemClicked(slot); });

                itemRemoveButton.onClick.RemoveAllListeners();
                itemRemoveButton.onClick.AddListener(delegate { OnRemoveItemClicked(slot); });

                _itemOptionsUI.transform.position = Input.mousePosition + itemOptionOffset;

                _itemOptionsUI.SetActive(true);
            }
            else
            {
                _itemOptionsUI.SetActive(false);
            }
        }

        private void OnUseItemClicked(ItemSlot slot)
        {
            interactor.UseItem(slot);
            _itemOptionsUI.SetActive(false);
        }

        private void OnRemoveItemClicked(ItemSlot slot)
        {
            slot.Remove(true);
            _itemOptionsUI.SetActive(false);
        }
    }
}
