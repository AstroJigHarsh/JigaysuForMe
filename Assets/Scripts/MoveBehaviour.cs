using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

// MoveBehaviour inherits from GenericBehaviour. This class corresponds to basic walk and run behaviour, it is the default behaviour.
public class MoveBehaviour : GenericBehaviour

{
    public Animator Animator;
    public CapsuleCollider CapHeiChar;
    float walkSpeed;                 
    float runSpeed;                   
    float sprintSpeed;                
    public float speedDampTime = 0.1f;              
    public string jumpButton = "Jump";              
    float jumpHeight;                
    float jumpInertialForce;         
    public float standHeight = 1.47f;
    public float crouchHeight = 0.9f;
    public Vector3 standingCapsuleCentre = new Vector3(0f, 0.65f, 0f);
    public Vector3 crouchingCapsuleCentre = new Vector3(0f, 0.44f, 0f);
    public Transform barrelTransform;  
    public GameObject bulletPrefab;   
    public float bulletSpeed = 10f;  
    public GameObject GunOBJ;
    


    private float speed, speedSeeker;            
    private int jumpBool;                      
    private int groundedBool;                     
    private bool jump;                           
    private bool isColliding;                    
    private bool Crouching = false;
    private bool Gun = false;
    private int Aiming;
    private int Fire;
    private bool CanAiming;
    private bool canJump = true;
    public GameObject CanvasName;
    public TMP_Text Name;
    public bool Walking;
    public bool Running;
    void Start()
    {
        walkSpeed = GetComponent<CharFeatures>().walkSpeed;
        runSpeed = GetComponent<CharFeatures>().runSpeed;
        sprintSpeed = GetComponent<CharFeatures>().sprintSpeed;
        jumpHeight = GetComponent<CharFeatures>().jumpHeight;
        jumpInertialForce = GetComponent<CharFeatures>().jumpInertialForce;
        if(GetComponent<PhotonView>().IsMine == true)
        {
            CanvasName.SetActive(false);
        }
        else
        {
            Name.text = GetComponent<PhotonView>().Controller.NickName;
        }
        jumpBool = Animator.StringToHash("Jump");
        groundedBool = Animator.StringToHash("Grounded");
        behaviourManager.GetAnim.SetBool(groundedBool, true);
        Aiming = Animator.StringToHash("Aim");
        Fire = Animator.StringToHash("Fire");

        behaviourManager.SubscribeBehaviour(this);
        behaviourManager.RegisterDefaultBehaviour(this.behaviourCode);
        speedSeeker = runSpeed;
    }

    void Update()
    {
        if (GetComponent<PhotonView>().IsMine == true)
        {
            Crouch();
            Gunning();
            if (!jump && Input.GetButtonDown(jumpButton) && behaviourManager.IsCurrentBehaviour(this.behaviourCode) && !behaviourManager.IsOverriding())
            {
                jump = true;
            }
            if (GetComponent<PhotonView>().IsMine && Input.GetMouseButtonDown(0) && Gun == true && CanAiming == true)
            {
                FireBullet();
            }
            
            
        }
        

    }

    public override void LocalFixedUpdate()
    {
        if (GetComponent<PhotonView>().IsMine == true)
        {
            MovementManagement(behaviourManager.GetH, behaviourManager.GetV);

            JumpManagement();
        }
    }

    void JumpManagement()
    {
        if (jump && !behaviourManager.GetAnim.GetBool(jumpBool) && behaviourManager.IsGrounded() && canJump == true)
        {
            behaviourManager.LockTempBehaviour(this.behaviourCode);
            behaviourManager.GetAnim.SetBool(jumpBool, true);
            if (behaviourManager.GetAnim.GetFloat(speedFloat) > 0.1)
            {
                GetComponent<CapsuleCollider>().material.dynamicFriction = 0f;
                GetComponent<CapsuleCollider>().material.staticFriction = 0f;
                RemoveVerticalVelocity();
                float velocity = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
                velocity = Mathf.Sqrt(velocity);
                behaviourManager.GetRigidBody.AddForce(Vector3.up * velocity, ForceMode.VelocityChange);
            }
        }
        else if (behaviourManager.GetAnim.GetBool(jumpBool))
        {
            if (!behaviourManager.IsGrounded() && !isColliding && behaviourManager.GetTempLockStatus())
            {
                behaviourManager.GetRigidBody.AddForce(transform.forward * (jumpInertialForce * Physics.gravity.magnitude * sprintSpeed), ForceMode.Acceleration);
            }
            if ((behaviourManager.GetRigidBody.velocity.y < 0) && behaviourManager.IsGrounded())
            {
                behaviourManager.GetAnim.SetBool(groundedBool, true);
                GetComponent<CapsuleCollider>().material.dynamicFriction = 0.6f;
                GetComponent<CapsuleCollider>().material.staticFriction = 0.6f;
                jump = false;
                behaviourManager.GetAnim.SetBool(jumpBool, false);
                behaviourManager.UnlockTempBehaviour(this.behaviourCode);
            }
        }
    }

    void MovementManagement(float horizontal, float vertical)
    {
        if (behaviourManager.IsGrounded())
            behaviourManager.GetRigidBody.useGravity = true;

        else if (!behaviourManager.GetAnim.GetBool(jumpBool) && behaviourManager.GetRigidBody.velocity.y > 0)
        {
            RemoveVerticalVelocity();
        }

        Rotating(horizontal, vertical);

        Vector2 dir = new Vector2(horizontal, vertical);
        speed = Vector2.ClampMagnitude(dir, 1f).magnitude;
        speedSeeker += Input.GetAxis("Mouse ScrollWheel");
        speedSeeker = Mathf.Clamp(speedSeeker, walkSpeed, runSpeed);
        speed *= speedSeeker;
        if (behaviourManager.IsSprinting())
        {
            speed = sprintSpeed;
            Running = true;
        }
        else
        {
            Running = false;
        }
        
        behaviourManager.GetAnim.SetFloat(speedFloat, speed, speedDampTime, Time.deltaTime);
    }

    private void RemoveVerticalVelocity()
    {
        Vector3 horizontalVelocity = behaviourManager.GetRigidBody.velocity;
        horizontalVelocity.y = 0;
        behaviourManager.GetRigidBody.velocity = horizontalVelocity;
    }

    Vector3 Rotating(float horizontal, float vertical)
    {
        Vector3 forward = behaviourManager.playerCamera.TransformDirection(Vector3.forward);

        forward.y = 0.0f;
        forward = forward.normalized;

        Vector3 right = new Vector3(forward.z, 0, -forward.x);
        Vector3 targetDirection = forward * vertical + right * horizontal;

        if ((behaviourManager.IsMoving() && targetDirection != Vector3.zero))
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            Quaternion newRotation = Quaternion.Slerp(behaviourManager.GetRigidBody.rotation, targetRotation, behaviourManager.turnSmoothing);
            behaviourManager.GetRigidBody.MoveRotation(newRotation);
            behaviourManager.SetLastDirection(targetDirection);
            Walking = true; 
        }
        else
        {
            Walking = false;
        }
        if (!(Mathf.Abs(horizontal) > 0.9 || Mathf.Abs(vertical) > 0.9))
        {
            behaviourManager.Repositioning();
        }

        return targetDirection;
    }
    private void OnCollisionStay(Collision collision)
    {
        isColliding = true;
        if (behaviourManager.IsCurrentBehaviour(this.GetBehaviourCode()) && collision.GetContact(0).normal.y <= 0.1f)
        {
            GetComponent<CapsuleCollider>().material.dynamicFriction = 0f;
            GetComponent<CapsuleCollider>().material.staticFriction = 0f;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
        GetComponent<CapsuleCollider>().material.dynamicFriction = 0.6f;
        GetComponent<CapsuleCollider>().material.staticFriction = 0.6f;
    }
    void Crouch()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (Crouching == true)
            {
                Crouching = false;
                Animator.SetBool("Crouching", false);
                CapHeiChar.height = standHeight;
                CapHeiChar.center = standingCapsuleCentre;
                canJump = true;
               
            }
            else
            {
                Crouching = true;
                Animator.SetBool("Crouching", true);
                CapHeiChar.height = crouchHeight;
                CapHeiChar.center = crouchingCapsuleCentre;
                canJump = false;
            }
        }
    }

    void Gunning()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Gun == true)
            {
                Gun = false;
                CanAiming = false;
                GunOBJ.SetActive(false);
                Animator.SetBool("Gun", false);
                canJump = true;
               
            }
            else
            {
                Gun = true;
                CanAiming = true;
                GunOBJ.SetActive(true);
                Animator.SetBool("Gun", true);
                canJump = false;
            }
        }
        
    }

    
    [PunRPC]
    void FireBullet()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Animator.SetBool(Fire, true);
            GameObject bullet = PhotonNetwork.Instantiate(bulletPrefab.name, barrelTransform.position, barrelTransform.rotation);
            Vector3 direction = (hit.point - barrelTransform.position).normalized;
            bullet.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;
            Animator.SetBool(Fire, false);
            Destroy(bullet, 3f);
        } 
    }
}