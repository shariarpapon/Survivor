using UnityEngine;

public class ItemGrabber : MonoBehaviour
{
    private const float lifetime = 300;
    private const float rotSpeed = 200;
    private readonly Vector3 rotDir = new Vector3(0.2f, 1, 0.2f).normalized;

    [HideInInspector] public Item item;

    private float timeAlive;

    private void Update()
    {
        timeAlive += Time.deltaTime;
        if (timeAlive > lifetime) { Destroy(gameObject); return; }
        transform.Rotate(rotDir, rotSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider col) 
    {
        if (col.CompareTag("Player")) col.GetComponent<Interactor>().AddToInventory(item, gameObject);
    }

    public void Create(Item item) 
    {
        this.item = item;
    }
}
