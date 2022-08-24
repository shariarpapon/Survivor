using UnityEngine;

public class WeaponUseAnimationEvents : MonoBehaviour
{
    public GameObject hitArea;
    public Interactor player;
    private readonly Vector3 hitAreaSize = new Vector3(0.5f, 0.5f, 0.3f);

    public void AnimationEvent_OnHit()
    {
        Collider[] overlapColliders = Physics.OverlapBox(hitArea.transform.position, hitAreaSize);
        foreach (Collider col in overlapColliders)
        {
            if (col.TryGetComponent<OnHitInteractable>(out OnHitInteractable interactable))
            {
                if (player.InRange(interactable.transform.position, interactable.hitRange)) 
                {
                    if (interactable.Hit(player)) 
                    {
                        Destroy(Instantiate(interactable.onHitPartcile, hitArea.transform.position, Quaternion.identity), 2f);
                    }
                }
            }
        }
    }
}
