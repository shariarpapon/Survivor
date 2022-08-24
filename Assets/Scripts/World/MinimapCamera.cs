using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [SerializeField] private float mapViewRange = 20;
    [SerializeField] private Transform playerIndicator;

    private Transform playerTransform;


    private void Start() 
    {
        playerTransform = FindObjectOfType<Player>().transform;
    }

    private void Update() 
    {
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y + mapViewRange, playerTransform.position.z);

        float angle = Vector3.SignedAngle(playerTransform.forward, Vector3.forward, Vector3.up);
        playerIndicator.rotation = Quaternion.Euler(0, 0, angle);
    }
}
