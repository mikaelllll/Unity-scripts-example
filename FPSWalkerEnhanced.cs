using UnityEngine;
using System.Collections;

//Script for the player movement on changing surfaces in a style similar to super mario galaxy where you can walk on a sphere or a weird-shaped "planet"

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (Collider))]

public class FPSWalkerEnhanced: MonoBehaviour {

	//Player velocity
	public float speed = 8.0f;
	//Player gravity strength
	public float gravity = 10.0f;
	//Maximum velocity change
	public float maxVelocityChange = 10.0f;
	//Character control to change direction in middle jump
	public float inAirControl = 0.1f;

	//Boolean to check if player can jump or is grounded
	public bool canJump = true;
	//Player jump height
	public float jumpHeight = 2.0f;

	//Boolean to detect wether player is grounded or not
	public bool isGrounded = false;
	//Player velocity while touching ground
	private Vector3 groundVelocity;
	//Var to check if player is jumping or not
	private bool isJump = false;
	//Player movement var
	private Vector3 movement;

	//Variables to aid detection to check wether the player is touching ground or not
	[HideInInspector]
	public Vector3 downColliderBound;
	//Rotation speed for turning the player upright relative to it's ground surface
	public float rotationSpeed = 8.0f;
	//Global object with a list of the planets
	public PlanetData.PlanetGravityType gravityType;
	//Object which the player will be turning upright to
	public GameObject gravityObject;
	//Player gravity rotation direction
	protected Vector3 gravityRotationDirection;
	//Player lowest point distance to detect wether is grounder or not
	protected float minimumCollisionRange = 0.5f;
	
	//Setters e Getters
	#region Setters e Getters
	public GameObject GravityObject{
		get{
			return gravityObject;
		}
		set{
			this.gravityObject = value;
			this.gravityType = gravityObject.GetComponent<PlanetData>().PlanetOrientation;
		}
	}

	public Vector3 DownColliderBound {
		get{
			return this.downColliderBound;
		}
		set{
			this.downColliderBound = value;
		}
	}
	#endregion
		
	//Initially detects player object collision point and set initial variables
    //Detech what planet is the cloest to the player to set it as it's center of gravity
	void Awake ()
	{
		DetectColisionPoint();
		CalculateClosestPlanet();
		GetComponent<Rigidbody>().freezeRotation = true;
		GetComponent<Rigidbody>().useGravity = false;
	}

	void FixedUpdate ()
	{
		if(!isGrounded)
			isJump = false;
		//Detects collision point because it might change due to player's rotation
		DetectColisionPoint();
		CalculateClosestPlanet();

        //Calculate the direction and rotation of the player relative to the gravity source
		gravityRotationDirection = transform.position - gravityObject.transform.position;

		if (isGrounded)
		{
			// Calculate how fast we should be moving
			movement = transform.TransformDirection(movement);
			movement *= speed;
			
			// Apply a force that attempts to reach our target velocity
			var velocity = GetComponent<Rigidbody>().velocity;
			var velocityChange = (movement - velocity) + groundVelocity;
			velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
			velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
			//velocityChange.y = 0;
			//velocityChange *= Vector3.one - (transform.up * -1);
			GetComponent<Rigidbody>().AddForce(velocityChange, ForceMode.VelocityChange);
			
			// Jump
			if (canJump && isJump)
			{
				//trocar transform.up por collider.transform.up?
				GetComponent<Rigidbody>().velocity = velocity + (CalculateJumpVerticalSpeed() * transform.up);//new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
			}
			isGrounded = false;
		}
		else
		{
			// Add an air force
			movement = transform.TransformDirection(movement) * inAirControl;
			GetComponent<Rigidbody>().AddForce(movement, ForceMode.VelocityChange);
		}
		

		PlanetData.PlanetGravityType type = gravityObject.GetComponent<PlanetData>().PlanetOrientation;

        //Rotation velocity relative to the distance
		float currDistance = Vector3.Distance(transform.position, gravityObject.transform.position) - gravityObject.GetComponent<PlanetData>().GravityForce;
		if(currDistance < 25){
			rotationSpeed = 8.0f;
		}
		else if(currDistance > 200){
			rotationSpeed = 1.0f;
		}
		else{
			rotationSpeed = currDistance /(200/8);
		}

        //Calculate the rotation according to the surface. If it is a sphere type planet the player will ALWAYS be upright relative to the center of the planet
        //If the planet is a plante, he will be upright relative to the plane direction
        //If it's a snap tipe, a raycast will be cast from the player to the planet. The normal direction of the surface hit by the raycast will be used as the angle for the player
		Quaternion targetRotation;
		switch(type){
		case PlanetData.PlanetGravityType.Sphere:
			targetRotation = (Quaternion.FromToRotation(transform.up, gravityRotationDirection) * transform.rotation);
			transform.rotation =  Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);
			GetComponent<Rigidbody>().AddForce(this.transform.rotation * (new Vector3 (0, -gravity * GetComponent<Rigidbody>().mass)), 0);
			break;

		case PlanetData.PlanetGravityType.Plane:
			targetRotation = (Quaternion.FromToRotation(transform.up, gravityObject.transform.up) * transform.rotation);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);
			GetComponent<Rigidbody>().AddForce(gravityObject.transform.rotation * (new Vector3 (0, -gravity * GetComponent<Rigidbody>().mass)), 0);
			break;
		case PlanetData.PlanetGravityType.Snap:
			RaycastHit hit = new RaycastHit();
			if(Physics.Raycast(transform.position, transform.up * -1, out hit, 10) && hit.collider.gameObject == gravityObject){
				targetRotation = (Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation);
				transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);
				GetComponent<Rigidbody>().AddForce(this.transform.rotation * (new Vector3 (0, -gravity * GetComponent<Rigidbody>().mass)), 0);
			}
			else{
				targetRotation = (Quaternion.FromToRotation(transform.up, gravityRotationDirection) * transform.rotation);
				transform.rotation =  Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);
				GetComponent<Rigidbody>().AddForce(this.transform.rotation * (new Vector3 (0, -gravity * GetComponent<Rigidbody>().mass)), 0);
			}
			break;
		}


	}

	public void Move(Vector3 movement){
		this.movement = movement;
	}

	public void Jump(float height){
		jumpHeight = height;
		Jump();
	}
	public void Jump(){
		isJump = true;
	}


    
    //Claculates the collision point of the player according to which ground he walks on
	public void DetectColisionPoint(){
		Collider[] collisions = this.GetComponents<Collider>();
		Collider coll = null;
		
        //Find which collider the point should calculate to in case the player has more than one collider for trigger reasons
		foreach(Collider c in collisions){
			if(!c.isTrigger)
				coll = c;
		}
		
		downColliderBound = Vector3.zero;
		
		//Calculate for the different tipes of collider
		switch(coll.GetType().ToString()){
		case "UnityEngine.BoxCollider":
			
			float BY = (coll as BoxCollider).size.y * transform.localScale.y * 0.5f;
			downColliderBound += transform.up * BY * -1;
			Vector3 Bcenter = (coll as BoxCollider).center;
			Vector3 BmultCenter = new Vector3(Bcenter.x * transform.localScale.x, Bcenter.y * transform.localScale.y, Bcenter.z * transform.localScale.z);
			downColliderBound += transform.rotation * BmultCenter;
			
			break;
		case "UnityEngine.CapsuleCollider":
			
			float CY = Mathf.Max((coll as CapsuleCollider).radius * 2, (coll as CapsuleCollider).height) *  transform.localScale.y * 0.5f;
			downColliderBound += transform.up * CY * -1;
			Vector3 Ccenter = (coll as CapsuleCollider).center;
			Vector3 multCenter = new Vector3(Ccenter.x * transform.localScale.x, Ccenter.y * transform.localScale.y, Ccenter.z * transform.localScale.z);
			downColliderBound += transform.rotation * multCenter;
			
			break;
			
		}
		downColliderBound = Quaternion.Inverse(transform.rotation) * downColliderBound;


	}

    //Catch the point in which the colliders touch, check the collision point and detect if the player is grounder or not
	void TrackGrounded (Collision col)
	{
		foreach (ContactPoint c in col.contacts)
		{
			if (Vector3.Distance(c.point, transform.position + ( transform.rotation * downColliderBound) ) < minimumCollisionRange)
			{
				if (col.rigidbody)
					groundVelocity = col.rigidbody.velocity;
				else
					groundVelocity = Vector3.zero;
				isGrounded = true;
			}
		} 
	}

	
	void OnCollisionStay (Collision col)
	{
		TrackGrounded (col);
	}
	
	void OnCollisionEnter (Collision col)
	{
		TrackGrounded (col);
	}

    // From the jump height and gravity we deduce the upwards speed
    // for the character to reach at the apex.
    float CalculateJumpVerticalSpeed ()
	{
		return Mathf.Sqrt(2 * jumpHeight * gravity);
	}

    //Calculate the closest planet object to the player and sets it as the player's gravity source planet
	void CalculateClosestPlanet(){
		this.gravityObject = GravityController.instance.SelectClosestGravityPlanet(downColliderBound + transform.position, this.gravityObject);
		this.gravityType = gravityObject.GetComponent<PlanetData>().PlanetOrientation;
	}

	public bool IsGroundedValue(){
		return this.isGrounded;
	}





}