using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotUIEventHandler : MonoBehaviour, IInitializer, IEndDragHandler, IDragHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler
{
    public static bool isInspecting;
    public static ItemSlot dragSlot;
    public static ItemSlot targetSlot;

    private ItemSlot mySlot;
    private Image slotUI;

    private Vector3 originalLocalPosition;
    private Vector3 originalGlobalPosition;

    private Vector3 offset;
    private int originalIndex;
    private Transform originalParent;
    private Transform rootParent;

    public IEnumerator Init()
    {
        mySlot = GetComponent<ItemSlot>();
        originalLocalPosition = transform.localPosition;
        originalGlobalPosition = transform.position;
        slotUI = GetComponent<Image>();
        originalIndex = transform.GetSiblingIndex();
        originalParent = transform.parent;
        rootParent = transform.root;
        GetComponent<Button>()?.onClick.AddListener(delegate { InGameUIManager.Instance.DisplayItemOptions(mySlot); });
        yield return null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && dragSlot && targetSlot)
        {
            GameUtility.TransferItem(dragSlot, targetSlot, 1);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        transform.SetParent(rootParent);
        offset = originalGlobalPosition - Input.mousePosition;
        dragSlot = mySlot;
        targetSlot = null;
        slotUI.raycastTarget = false;
        transform.SetAsLastSibling();
        SetBackgroundAlpha(0.6f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !dragSlot) return;

        dragSlot.transform.position = Input.mousePosition + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (targetSlot && dragSlot) GameUtility.SwapSlot(dragSlot, targetSlot);
        transform.SetParent(originalParent);
        transform.SetSiblingIndex(originalIndex);
        slotUI.raycastTarget = true;
        targetSlot = null;
        dragSlot.transform.localPosition = originalLocalPosition;
        dragSlot = null;
        SetBackgroundAlpha(1);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetSlot = mySlot;

        if (!isInspecting && !mySlot.IsEmpty)
        {
            isInspecting = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetSlot = null;

        if (isInspecting) 
        {
            isInspecting = false;
        }
    }

    private void SetBackgroundAlpha(float alpha) 
    {
        slotUI.color = new Color(slotUI.color.r, slotUI.color.g, slotUI.color.b, alpha);
    }
}
