using UnityEngine;
using Photon.Pun;


public class FlyBehaviour : GenericBehaviour
{
	public string flyButton = "Fly";              
	public float flySpeed = 4.0f;                 
	public float sprintFactor = 2.0f;             
	public float flyMaxVerticalAngle = 60f;       
	public GameObject Trail1;
	public GameObject Trail2;
	public GameObject Trail3;
	public GameObject Trail4;

	private int flyBool;                          
	private bool fly = false;                     
	private CapsuleCollider col;                  

	
	void Start()
	{
		// Set up the references.
		flyBool = Animator.StringToHash("Fly");
		col = this.GetComponent<CapsuleCollider>();
		behaviourManager.SubscribeBehaviour(this);
	}

	
	void Update()
	{
        if (GetComponent<PhotonView>().IsMine == true)
		{
            if (Input.GetButtonDown(flyButton) && !behaviourManager.IsOverriding()
            && !behaviourManager.GetTempLockStatus(behaviourManager.GetDefaultBehaviour))
            {
                fly = !fly;


                behaviourManager.UnlockTempBehaviour(behaviourManager.GetDefaultBehaviour);


                behaviourManager.GetRigidBody.useGravity = !fly;

                // Player is flying.
                if (fly)
                {

                    behaviourManager.RegisterBehaviour(this.behaviourCode);
                    Trail1.SetActive(true);
                    Trail2.SetActive(true);
                    Trail3.SetActive(true);
                    Trail4.SetActive(true);
                }
                else
                {

                    col.direction = 1;
                    // Set camera default offset.
                    behaviourManager.GetCamScript.ResetTargetOffsets();


                    behaviourManager.UnregisterBehaviour(this.behaviourCode);
                    Trail1.SetActive(false);
                    Trail2.SetActive(false);
                    Trail3.SetActive(false);
                    Trail4.SetActive(false);
                }
            }
            
		}

		
		fly = fly && behaviourManager.IsCurrentBehaviour(this.behaviourCode);

		
		behaviourManager.GetAnim.SetBool(flyBool, fly);
	}

	
	public override void OnOverride()
	{
		
		col.direction = 1;
	}

	
	public override void LocalFixedUpdate()
	{
		if (GetComponent<PhotonView>().IsMine == true)
		{
            behaviourManager.GetCamScript.SetMaxVerticalAngle(flyMaxVerticalAngle);


            FlyManagement(behaviourManager.GetH, behaviourManager.GetV);
        }

           
	}
	// Deal with the player movement when flying.
	void FlyManagement(float horizontal, float vertical)
	{
		
		Vector3 direction = Rotating(horizontal, vertical);
		behaviourManager.GetRigidBody.AddForce(direction * (flySpeed * 100 * (behaviourManager.IsSprinting() ? sprintFactor : 1)), ForceMode.Acceleration);
	}

	// Rotate the player to match correct orientation, according to camera and key pressed.
	Vector3 Rotating(float horizontal, float vertical)
	{
		Vector3 forward = behaviourManager.playerCamera.TransformDirection(Vector3.forward);
		
		forward = forward.normalized;

		Vector3 right = new Vector3(forward.z, 0, -forward.x);

		
		Vector3 targetDirection = forward * vertical + right * horizontal;

		// Rotate the player to the correct fly position.
		if ((behaviourManager.IsMoving() && targetDirection != Vector3.zero))
		{
			Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

			Quaternion newRotation = Quaternion.Slerp(behaviourManager.GetRigidBody.rotation, targetRotation, behaviourManager.turnSmoothing);

			behaviourManager.GetRigidBody.MoveRotation(newRotation);
			behaviourManager.SetLastDirection(targetDirection);
		}

		
		if (!(Mathf.Abs(horizontal) > 0.2 || Mathf.Abs(vertical) > 0.2))
		{
			
			behaviourManager.Repositioning();
			
			col.direction = 1;
		}
		else
		{
			
			col.direction = 2;
		}

		
		return targetDirection;
	}
}
