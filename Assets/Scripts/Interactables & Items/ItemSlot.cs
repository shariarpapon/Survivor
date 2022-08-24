using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ItemSlot : MonoBehaviour, IInitializer
{
    private Image iconImage;
    private TextMeshProUGUI countText;
    protected Image durabilityBar;

    [HideInInspector] public Item slotItem;
    [HideInInspector] public int itemCount;

    private Interactor player;

    public bool IsEmpty { get { return itemCount <= 0; } }

    public IEnumerator Init()
    {
        player = FindObjectOfType<PlayerController>().GetComponent<Interactor>();
        iconImage = transform.Find("Icon").GetComponent<Image>();
        countText = transform.Find("Count").GetComponent<TextMeshProUGUI>();
        durabilityBar = transform.Find("Durability").GetComponent<Image>();
        OnSlotModified();
        yield return null;
    }

    public virtual bool TryAdd(Item item)
    {
        if (IsAddable(item))
        {
            slotItem = item;
            itemCount++;
            OnSlotModified();
            return true;
        }
        OnSlotModified();
        return false;
    }

    public void Remove(bool dropToGround)
    {
        itemCount--;
        if (dropToGround) player.DropToGround(slotItem);
        if (IsEmpty) OnSlotEmpty();
        OnSlotModified();
    }

    public void RemoveAmount(int amount, bool dropToGround) 
    {
        itemCount -= amount;

        if (dropToGround) 
            for (int i = 0; i < amount; i++) 
            {
                if (itemCount > 0) player.DropToGround(slotItem);
                else break;
            }

        if (IsEmpty) OnSlotEmpty();

        OnSlotModified();
    }

    private void OnSlotEmpty() 
    {
        itemCount = 0;
        slotItem = Item.Null;
        iconImage.sprite = null;
        iconImage.gameObject.SetActive(false);
    }

    protected virtual void OnSlotModified() 
    {
        if (!IsEmpty)
        {
            iconImage.sprite = slotItem.itemData.icon;
            iconImage.gameObject.SetActive(true); 
            countText.text = itemCount.ToString();

            if (slotItem.itemData.type == ItemType.Tool) 
            {
                durabilityBar.gameObject.SetActive(true);
                durabilityBar.fillAmount = slotItem.stats.DurabilityPercentage;
            }
        }
        else 
        {
            slotItem = Item.Null;
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(false);
            countText.text = string.Empty;
            durabilityBar.gameObject.SetActive(false);
        }
    }

    public void Clear()
    {
        itemCount = 0;
        OnSlotModified();
    }

    private bool IsAddable(Item item)
    {
        if (slotItem != Item.Null)
        {
            if (slotItem == item && itemCount < item.itemData.maxItemsPerSlot) return true;
            else return false;
        }
        else return true;
    }
}
