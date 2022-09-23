using UnityEngine;

public class PlayerController_Movement : MonoBehaviour
{
    public float speed = 7.0f;
    public Transform camTransform;

    private void Update() 
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, camTransform.eulerAngles.y, transform.rotation.eulerAngles.z);
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(x,0, y);
        if (dir.normalized.magnitude >= 1)
        {
            transform.Translate(transform.forward * speed * Time.deltaTime, Space.Self);
        }

        if (Input.GetKey(KeyCode.LeftShift))
            transform.Translate(Vector3.down * speed * Time.deltaTime) ;
        
        if (Input.GetKey(KeyCode.Space))
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        
    }
}
