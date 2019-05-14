using UnityEngine;
using System.Collections;

//A class for a bat enemy that uses the basic flyinf follow script to follow the player around and go back to it's original position when loses sight of the player
//Has a tackel animation to hit the player if it's get too close that is made by script

public class BatController : BasicFlyingFolower {

	//Variable that contains information about the type of gravity the planet the enemy is attached to
	public PlanetData.PlanetGravityType gravityType;
	//The planet that the enemy is attached to
	public GameObject gravityObject;

    //Distance for the enemy to perform it's attack
	public float attackDistance = 5.0f;

    //Position where it begins the attack so it can come back
	protected Vector3 attackPoint;

    //Control booleans to make the attack animation
	protected bool isAttacking = false;
	protected bool attackBacking = false;
	protected bool attackGoin = false;
    
    //variables to configure the attack
	public float attackCooldown = 3;
	public float attackBackingDistance = 2.0f;
	public float attackGoinDistance = 4.0f;
	public float attackMoveSpeedMultiplier = 7.0f;
	protected float currentCooldown = 3f;

	#region Setters and Getters
	public GameObject GravityObject{
		get{
			return gravityObject;
		}
		set{
			this.gravityObject = value;
			this.gravityType = gravityObject.GetComponent<PlanetData>().PlanetOrientation;
		}
	}
	#endregion


    //Upon awake find which planet is closest to it and attach it as the enemy's planet for rotation during the movement
	override protected void Awake () {
		base.Awake();
		CalculateClosestPlanet();
	}
    
	override protected void Start () {
		base.Start ();
	}

	void Update(){
		currentCooldown += Time.deltaTime;
	}

    //enemy AI that decides wether it should follow the player or if it can attack
	override protected void FixedUpdate() {
		body.velocity = Vector3.zero;
		if((Vector3.Distance (this.transform.position, objectToFollow.transform.position) < 5f || isAttacking) && IsObjectInSight() && (currentCooldown > attackCooldown)){
			if(!isAttacking){
				attackPoint = this.transform.position;
				attackBacking = true;
			}
			isAttacking = true;
			Attack ();
		}else{
			FollowObject();
		}
	}

    //function that makes the attack animation for the enemy. There is no damage script because damage is done during colision
	void Attack(){		
		if(Vector3.Distance(this.transform.position, attackPoint) > attackBackingDistance && attackBacking){
			attackBacking = false;
			attackGoin = true;
			attackPoint = this.transform.position;
		}
		if(Vector3.Distance(this.transform.position, attackPoint) > attackGoinDistance && attackGoin){
			attackGoin = false;
			isAttacking = false;
			currentCooldown = 0;
		}

		if(attackBacking == true){
			Quaternion targetRotation =  Quaternion.LookRotation (objectToFollow.transform.position - transform.position);
			this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, rotationSpeed * 3);
			body.AddForce(this.transform.forward*-1* movementSpeed*attackMoveSpeedMultiplier, ForceMode.VelocityChange);
		}else if(attackGoin == true){
			body.AddForce(this.transform.forward* movementSpeed*attackMoveSpeedMultiplier, ForceMode.VelocityChange);
		}
	}

    //Function to check what is the closes planet and it's type
	void CalculateClosestPlanet(){
		this.gravityObject = GravityController.instance.SelectClosestGravityPlanet(transform.position, this.gravityObject);
		this.gravityType = gravityObject.GetComponent<PlanetData>().PlanetOrientation;
	}
}
