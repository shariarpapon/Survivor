using UnityEngine;

namespace Survivor.Core
{
    public class EquipmentManager : MonoBehaviour
    {
        public static EquipmentManager Instance { get; private set; }
        public ToolSlot toolSlot;
        public Transform toolEquipOrigin;
        public ItemData EquipedTool { get { return toolSlot.slotItem.itemData; } }

        private GameObject toolInstance;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public void EquipTool()
        {
            if (toolSlot.IsEmpty || toolSlot.slotItem.itemData.type != ItemType.Tool) return;

            Destroy(toolInstance);

            toolInstance = Instantiate(toolSlot.slotItem.itemData.prefab, toolEquipOrigin);
            toolInstance.transform.localScale = Vector3.one;
            toolInstance.transform.localPosition = Vector3.zero;

            Interactable interactable = toolInstance.GetComponent<Interactable>();
            if (interactable) Destroy(interactable);
        }

        public void UnequipTool()
        {
            Destroy(toolInstance);
        }

        public void DamageTool(float damage)
        {
            toolSlot.slotItem.DamageTool(damage);
            toolSlot.UpdateDurabilityBar();

            if (toolSlot.slotItem.stats.durability <= 0) DestroyTool();
        }

        private void DestroyTool()
        {
            Destroy(toolInstance);
            toolSlot.Remove(false);
        }
    }
}
