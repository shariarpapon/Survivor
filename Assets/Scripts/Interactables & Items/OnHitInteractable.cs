using UnityEngine;

public abstract class OnHitInteractable : MonoBehaviour
{
    public float hitRange = 3;
    public GameObject onHitPartcile;

    public abstract bool Hit(Interactor interactor);
}
