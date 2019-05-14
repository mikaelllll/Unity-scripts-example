using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Script created for a flying enemy that follows the player a certain distance.
//If the player moves far away from the enemy and the enemy loses line of sight, it walks a certain distance looking for the player
//before walking back to it's initial point

[RequireComponent (typeof (Rigidbody))]
public class BasicFlyingFolower : MonoBehaviour {

	public GameObject objectToFollow;

    //Variables for the movement of the enemy
	public float movementSpeed = 3f;
	public float rotationSpeed = 0.1f;
	public float followDistance = 5.0f;

    //The discance the enemy will walk in the direction of the point where it has lost vision of the object being followed
    protected float lostPlayerDistanceOffset = 15.0f;

    //A list containing the points for the enemy to walk the way back to it's original point
	protected List<Vector3> followPoints;

    //Variables to fill the list with the path to walk
	protected Vector3 lastSeenPoint;
	protected Vector3 nextPoint;

    //Enemy rigidbody
	protected Rigidbody body;

    //Bool that contains whether or not the enemy has vision of the object being followed
	protected bool gotVision = false;

    //Start body variable with the enemy rigisdoby, freezes the enemy rotation, removes the enemy gravity since it's a flying enemy, create the walking path list and get the hitline component
    //into a variable
	protected virtual void Awake(){
		this.body = GetComponent<Rigidbody>();
		this.body.freezeRotation = true;
		this.body.useGravity = false;
		followPoints = new List<Vector3>();
    }

	// Initialize the list with the enemy initial position so it knows where it should go back to
	protected virtual void Start () {
		AddCurrentPoint();
	}
	
	// Clean the object velocity so it doesn't stack it infinitely and call the function to follow the object
	protected virtual void FixedUpdate () {
		body.velocity = Vector3.zero;
		FollowObject();
	}

    //Checks whether or not the enemy has line of sight of the object being followed. If it has then the enemy follows the object being followed. If it has vision it walks towards the
    //object being followed. If it lost vision it walks towards the object being followed last known position. If it doesn't it walks back the path it followed the object being followed through.
    protected virtual void FollowObject(){
		if(IsObjectInSight()){
			WalksTowardObject();
		}else if(gotVision){
			LostObjectVision();
		}else{
			WalkTowardsLastPoint();
		}
	}

    //Check if the enemy has line of sight of the object being followed, the layer mask ~(1 << 8) being that the player is in layer 8
    protected virtual bool IsObjectInSight() {
		RaycastHit hit = new RaycastHit();
		Vector3 hitDirection = objectToFollow.transform.position - this.transform.position;
		int layerMask = ~(1 << 8);
		Physics.Raycast(this.transform.position, hitDirection, out hit, followDistance, layerMask);

		if(!(hit.collider == null)){
			if(hit.collider.gameObject == objectToFollow || hit.collider.gameObject.transform.IsChildOf(objectToFollow.transform)){
				return true;
			}
		}
		return false;

	}

    //Adds the enemy current location to the list to keep track of the path to go back through
	protected virtual void AddCurrentPoint(){
		followPoints.Add(this.transform.position);
	}

    //Moves the enemy in direction to the position of the object being followed
    //Activates a flag that obtains vision of the object being followed and saves it's last position
    protected virtual void WalksTowardObject(){
		Quaternion targetRotation =  Quaternion.LookRotation (objectToFollow.transform.position - transform.position);
		this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, rotationSpeed);
		body.AddForce(this.transform.forward * movementSpeed, ForceMode.VelocityChange);
		
		lastSeenPoint = objectToFollow.transform.position;

		gotVision = true;

		if(Vector3.Distance(this.transform.position, followPoints[followPoints.Count - 1]) > 2.0f){
			followPoints.Add(this.transform.position);
		}
	}

    //Toogle the flag that the enemy lost vision of the object being followed and saves it's last seen position
    protected virtual void LostObjectVision(){
		gotVision = false;
		
		Quaternion angleToRotatePoint = Quaternion.FromToRotation(lastSeenPoint,objectToFollow.transform.position);
		nextPoint = angleToRotatePoint * ((lastSeenPoint - objectToFollow.transform.position)*lostPlayerDistanceOffset) + lastSeenPoint;
		
		followPoints.Add(nextPoint);
	}

    //Makes the enemy move to the last seen point known of the object being followed. When he get's to that point he starts backtracking the way back to it's initial position
    protected virtual void WalkTowardsLastPoint(){
		if(Vector3.Distance(this.transform.position, followPoints.Last()) < 0.5f){
			if(followPoints.Count > 1){
				followPoints.RemoveAt(followPoints.Count() - 1);
			}
		}else{
			Quaternion targetRotation =  Quaternion.LookRotation (followPoints.Last() - transform.position);
			this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, rotationSpeed);
			body.AddForce(this.transform.forward * movementSpeed, ForceMode.VelocityChange);
		}
	}
}
