using UnityEngine;
using System.Collections;

//Script to update the gravity correctly any object affected by gravity of a "planet" according to it's relative rotation

[RequireComponent(typeof(CharacterController))]
public class CharacterControllerPhysics : MonoBehaviour {

    //Controll variables
	public bool useGravity = true;
	public float gravityStrength = 10;
	public float friction = 0.05f;

	public Vector3 force;
	public Vector3 gravityForce;

	protected CharacterController controller;


	public void ResetForce(){
		force = Vector3.zero;
	}
	public void AddToForce(Vector3 setForce){
		force += setForce;
	}
	public void Jump(float force){
		this.force += Vector3.up * force;
	}
    
	void Start () {
		controller = GetComponentInChildren<CharacterController>();
		force = Vector3.zero;
	}
	
	void Update () {
		UpdateForce();
		if(useGravity){
			UpdateGravity();
		}
	}

	protected void UpdateGravity(){
		force.y -= gravityStrength * Time.deltaTime;

		if(controller.isGrounded && force.y < 0){
			force.y = 0;
		}
		//Debug.Log(controller.isGrounded + ": " + force);
	}

	protected void UpdateForce(){
		controller.Move(force * Time.deltaTime);
	}
}
