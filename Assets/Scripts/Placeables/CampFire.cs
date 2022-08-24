using UnityEngine;

public class CampFire : PlaceableItem
{
    public float lifetime;

    public override void InitBuild()
    {
        Destroy(gameObject, lifetime);
    }
}
