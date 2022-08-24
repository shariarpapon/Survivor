using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public virtual void Interact(Interactor interactor) 
    {
        Debug.Log($"<color=yellow>Interacting with {transform.name}</color>");
    }
}
