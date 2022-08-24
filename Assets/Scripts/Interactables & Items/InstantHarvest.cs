using UnityEngine;

public class InstantHarvest : Interactable
{
    public ItemData dropItem;
    public GameObject harvestGameObject = null;

    private bool canAdd = true;

    public override void Interact(Interactor interactor)
    {
        if (!canAdd) return;

        canAdd = false;
        interactor.AddToInventory(new Item(dropItem), harvestGameObject == null ? gameObject : harvestGameObject);
    }
}
