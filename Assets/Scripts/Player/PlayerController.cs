using UnityEngine;
using Survivor.Core;

public class PlayerController : MonoBehaviour 
{
    public static bool IsControllable = false;

    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Vector2 inputDirection;

    private Rigidbody rb;

    private void Awake() 
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update() 
    {
        if (IsControllable == false) return;

        inputDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void FixedUpdate() 
    {
        
    }

    public static void SetCursor(CursorLockMode mode, bool visible)
    {
        Cursor.lockState = mode;
        Cursor.visible = visible;
    }
    
    public void TeleportTo(Vector3 target)
    {
        transform.position = target;
    }

    //Error remover code
    public enum ControlMode { Normal, Fly}
    public void SetControlMode(ControlMode m) { }
    public void SetMoveSpeed(float s) { }
    public int GetMoveSpeed() { return default; }
    public void SetFlySpeed(float s) { }
    public int GetFlySpeed () { return default; }
}

#region Old Code
//using UnityEngine;
//using Survivor.Core;

//public class PlayerController : MonoBehaviour
//{
//    public static bool IsControllable = true;

//    [SerializeField] private float moveSpeed = 6;
//    [SerializeField] private float angularSpeed = 12;
//    [SerializeField] private float flySpeed;
//    [SerializeField] private float gravitationalAccel = -19.0f;
//    [SerializeField] private float maxDownwardVelocity = -20;
//    [Space]
//    [SerializeField] private Transform playerCamera;
//    [SerializeField] private LayerMask whatIsGround;

//    private CharacterController cc;
//    private Vector2 input = Vector3.zero;
//    private Vector3 velocity = Vector3.zero;
//    private bool fly;

//    private void Awake()
//    {
//        cc = GetComponent<CharacterController>();
//        SetControlMode(ControlMode.Normal);
//    }

//    private void Update()
//    {
//        if (GameManager.GameMode != GameMode.Playing) return;

//        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

//        Fly();

//        AdjustRotation();
//        ApplyVelocity();
//    }

//    private void Fly() 
//    {
//        if (!fly) return;
//        if (Input.GetKey(KeyCode.Space)) velocity.y = flySpeed;
//        else if (Input.GetKey(KeyCode.LeftShift)) velocity.y = -flySpeed;
//        else velocity.y = 0;        
//    }

//    private void AdjustRotation() 
//    {
//        if (IsControllable)
//        {
//            Vector3 camRot = new Vector3(playerCamera.forward.x, 0, playerCamera.forward.z);
//            Quaternion targetRot = Quaternion.LookRotation(camRot, Vector3.up);
//            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, angularSpeed * Time.deltaTime);
//        }
//    }

//    private void ApplyVelocity() 
//    {
//        Vector3 input3D = new Vector3(input.x, 0, input.y);
//        float angleOffset = Vector3.SignedAngle(Vector3.forward, transform.forward, Vector3.up);
//        Vector3 dir = (Quaternion.Euler(0, angleOffset, 0) * input3D).normalized;
//        velocity = new Vector3(dir.x * moveSpeed, velocity.y, dir.z * moveSpeed);

//        if (!fly)
//        {
//            if (!IsGrounded())
//            {
//                velocity.y += gravitationalAccel;
//                if (velocity.y < maxDownwardVelocity)
//                    velocity.y = maxDownwardVelocity;
//            }
//            else velocity.y = -0.1f;
//        }

//        cc.Move(velocity * Time.deltaTime);
//    }

//    private bool IsGrounded()
//    {
//        Ray ray = new Ray();
//        ray.origin = transform.position;
//        ray.direction = Vector3.down;
//        if (Physics.Raycast(ray, 1.05f, (int)whatIsGround)) 
//            return true;

//        return false;
//    }

//    public void TeleportTo(Vector3 target) 
//    {
//        cc.enabled = false;
//        transform.position = target;
//        cc.enabled = true;
//    }

//    public static void SetCursor(CursorLockMode mode, bool visible) 
//    {
//        Cursor.lockState = mode;
//        Cursor.visible = visible;
//    }

//    public void SetControlMode(ControlMode mode) 
//    {
//        switch (mode) 
//        {
//            case ControlMode.Fly:
//                fly = true;
//                break;

//            case ControlMode.Normal:
//                fly = false;
//                break;
//        }
//    }

//    public void SetMoveSpeed(float speed) 
//    {
//        moveSpeed = speed;
//    }

//    public float GetMoveSpeed() => moveSpeed;

//    public void SetFlySpeed(float speed)
//    {
//        flySpeed = speed;
//    }

//    public float GetFlySpeed() => flySpeed;

//    public enum ControlMode 
//    {
//        Normal,
//        Fly
//    }
//}
#endregion
