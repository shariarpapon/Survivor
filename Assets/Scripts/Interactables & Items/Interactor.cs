using UnityEngine;
using Survivor.Core;

public class Interactor : MonoBehaviour
{
    public const int defaultDamage = 1;
    public const float interactionRange = 2f;

    public ItemContainer inventory;

    [SerializeField] private Camera playerCamera;
    [SerializeField] private Animator handAnimator;

    [Space]
    [SerializeField] private float attackDelay;

    private Ray ray;
    private RaycastHit hit;
    private Interactable target;
    private Player player;

    private readonly Vector3 dropLift = new Vector3(0, 0.5f, 0);
    public Vector3 ItemDropPosition { get { return transform.position + transform.forward + dropLift; } }
    private float attackTimer;

    private void Awake() 
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (GameManager.GameMode != GameMode.Playing) return;


        attackTimer += Time.deltaTime;
        attackTimer = Mathf.Clamp(attackTimer, 0, attackDelay);

        HandleInteraction();
    }

    private void HandleInteraction()
    {
        ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)) target = hit.transform.GetComponent<Interactable>();
        
        if (target != null && InRange(target.transform.position, interactionRange)) ObjectHighlighter.Instance.HighlightObject(target.gameObject);
        else { ObjectHighlighter.Instance.UnhighlightObject(null); }

        if (Input.GetMouseButtonDown(0)) InvokeInteraction();
    }

    private void InvokeInteraction() 
    {
        if (target != null && InRange(target.transform.position, interactionRange)) target.Interact(this);
        else Attack();
    }

    private void Attack() 
    {
        if (attackTimer < attackDelay) return;

        handAnimator.SetTrigger("Hit");
        attackTimer = 0;
    }

    public  bool InRange(Vector3 targetPosition, float range) 
    {
        return Vector3.Distance(targetPosition, transform.position) <= range;
    }

    public void AddToInventory(Item item, GameObject instance) 
    {
        if (inventory.AddItem(item))
            if (instance) StartCoroutine(GameUtility.TweenScaleOut(instance, 20, true));
    }

    public void DropToGround(Item item) 
    {
        GameUtility.InstantiateItemGrabber(item, ItemDropPosition);
        TooltipManager.Instance.TimedPopup(null, $"{item.itemData.name} dropped.", 3.5f);
    }

    #region Use Item Functions

    public void UseItem(ItemSlot slot)
    {
        if (slot == null || slot.IsEmpty) return;

        switch (slot.slotItem.itemData.type)
        {
            case ItemType.Consumable:
                ConsumeItem(slot);
                break;
            case ItemType.Tool:
                EquipItem(slot);
                break;
            case ItemType.Placeable:
                PlaceItem(slot);
                break;
            default:
                TooltipManager.Instance.TimedPopup(null, "Cannot directly use this item in any way.", 3.5f, TextOptions.YellowContent);
                break;
        }
    }

    public void ConsumeItem(ItemSlot slot) 
    {
        Debug.Log("Consuming " + slot.slotItem.itemData.name);
        player.AddVitals(slot.slotItem.itemData.healthConsumeAmount, slot.slotItem.itemData.energyConsumeAmount);
        slot.Remove(false);
    }

    public void EquipItem(ItemSlot slot)
    {
        GameUtility.SwapSlot(slot, EquipmentManager.Instance.toolSlot);
    }

    public void PlaceItem(ItemSlot slot)
    {
        if (slot.slotItem.itemData.type != ItemType.Placeable) return;

        BuildingManager.Instance.EnterItemBuildMode(slot);
    }

    #endregion
}
