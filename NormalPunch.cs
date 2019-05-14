using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Class that controls normal attack for the player. A object that represent a arm punching is instantiated randomly on the player vision and causes damage to a enemy that is aiming at.
//Multiple arms can be instantiated

[RequireComponent(typeof(HitLine))]
public class NormalPunch : MonoBehaviour {

    //Variables that take the character, variable that stores the arm of the punch and variable that stores the effect when the punch hits something
    public GameObject rotationController;
	public GameObject GO_Punch;
	public GameObject GO_PunchEffect;
	public GameObject damageFont;

    //Variable that contains the arms for the normal punch, the loaded punch and the variable that makes the hit detection of the punches
    protected GameObject[] GO_PunchInstancesNormal;
	protected GameObject[] GO_PunchInstancesCharged;
	protected GameObject[] GO_PunchInstancesGroundSmash;
	protected HitLine hitLine;

    //Variables used to calculate the randomness from where fast punches appear
    public float randX = 1;
	public float randY = 1;

    //Distance between the opening of two arms of the loaded punch
    public float chargedOpening = 0.5f;
    //Distance of the height of the left and right arms
    public float leftChargedHeight = 0.2f;
	public float rightChargedHeight = 0.2f;

	//Lifetime of each arm object
	public float modelLifespanNormal = 0.25f;
	public float modelLifespanCharged = 0.5f;

	//Each atack type cooldown
	public float coolDownTimerNormal = 1f;
	public float coolDownTimerCharged = 2f;

    //Power charge speed ratio
    public float chargeRatio = 1f;

	//Maximum hands visible
	public int maxHandsNormal = 10;

	//Punch damage distance
	public float punchDistante = 10.0f;

	//Ground smash distance
	public float groundSmashDistance = 100.0f;
	//Minimum distance for ground smash
	public float groundSmashMin = 0.0f;
	//Bool to check if strong attack is being charged
	public bool isGroundSmash = false;
	//Strong attaack origin point in player
	protected Vector3 groundSmashOrigin;
	//Ground smash attack location
	protected Vector3 groundSmashDestiny;
	//Ground smash speed
	public float groundSmashSpeed = 30.0f;
	//Ground smash acceleration
	public float groundSmashAcceleration = 0.1f;
	//Ground smash current speed
	protected float groundSmashCurrentSpeed;
	//Ground smash highlight
	protected bool groundSmashIsHilight = false;


	//Current cooldown for both attacks
	protected float currCoolDownTimerNormal;
	protected float currCoolDownTimerCharged;

	//Current strong attack charge
	public float currChargePower;

	//Player damage
	protected float damage;

	//Player rigidbody
	protected Rigidbody playerBody;

	//Player center height distance offset
	protected float heightOffset;

	/*
     * Layer reminder
	protected enum layersEnum {
		EnemyHitArea = 8,
		RigidMesh = 9,
		StaticMesh = 10,
		Player = 11,
		Cenario = 12
	}
	*/

	public LayerMask enemyLayer;
	public LayerMask sceneLayer;

	public Texture2D crosshairTexture;
	public float crosshairScale = 1;
	
	#region Status update
	public void UpdateDamage(float power){
		this.damage = 10 * Mathf.Pow((power / 10.0f), 2);
	}

	public void UpdateAttackSpeed(float speed){
		this.coolDownTimerNormal = 0.5f / (speed / 100.0f);
		currCoolDownTimerNormal = 0;
		currCoolDownTimerCharged = 0;
	}

	public void UpdateChargeSpeed(float concentration){
		this.chargeRatio = 1 * (concentration / 10.0f);
	}
	#endregion

	void OnGUI()
	{	
		if(groundSmashIsHilight){
			if(Time.timeScale != 0)
			{
				if(crosshairTexture!=null)
					GUI.DrawTexture(new Rect((Screen.width-crosshairTexture.width*crosshairScale)/2 ,(Screen.height-crosshairTexture.height*crosshairScale)/2, crosshairTexture.width*crosshairScale, crosshairTexture.height*crosshairScale),crosshairTexture);
				else
					Debug.Log("No crosshair texture set in the Inspector");
			}
		}
	}
	
	void Awake(){
		this.playerBody = transform.GetComponentInChildren<Rigidbody>();
		hitLine = GetComponent<HitLine>();

		heightOffset = this.transform.position.y - this.playerBody.GetComponent<Collider>().bounds.min.y;
	}
    
	void Start () {
		currCoolDownTimerNormal = 0;
		currCoolDownTimerCharged = 0;
		currChargePower = 0;
       
        //Instantiate the arms for the quick punches
		GO_PunchInstancesNormal = new GameObject[maxHandsNormal];
		for(int i = 0 ; i < maxHandsNormal ; ++i){
			GO_PunchInstancesNormal[i] = Instantiate(GO_Punch) as GameObject;
			PunchClass pun = GO_PunchInstancesNormal[i].AddComponent<PunchClass>();
			pun.Set(GO_PunchInstancesNormal[i]);

			pun.transform.parent = transform;
		}

		//Instantiate the arms for the strong puncch
		GO_PunchInstancesCharged = new GameObject[2];
		for(int i = 0 ; i < 2 ; ++i){
			GO_PunchInstancesCharged[i] = Instantiate(GO_Punch) as GameObject;
			PunchClass pun = GO_PunchInstancesCharged[i].AddComponent<PunchClass>();
			pun.Set(GO_PunchInstancesCharged[i]);
			
			pun.transform.parent = transform;
			pun.transform.position = transform.position;
		}

		//Adjust the position of the arms relative to the player
		Vector3 armPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		armPosition.x -= chargedOpening;
		armPosition.y += leftChargedHeight;
		GO_PunchInstancesCharged[0].transform.position = armPosition;

		armPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		armPosition.x += chargedOpening;
		armPosition.y += leftChargedHeight;
		GO_PunchInstancesCharged[1].transform.position = armPosition;


		//Instantiate the arms for the ground smash
		GO_PunchInstancesGroundSmash = new GameObject[2];
		for(int i = 0 ; i < 2 ; ++i){
			GO_PunchInstancesGroundSmash[i] = Instantiate(GO_Punch) as GameObject;
			PunchClass pun = GO_PunchInstancesGroundSmash[i].AddComponent<PunchClass>();
			pun.Set(GO_PunchInstancesGroundSmash[i]);
			
			pun.transform.parent = transform;
			pun.transform.position = transform.position;
		}

        //Adjust the position of the arms relative to the player
        armPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		armPosition.x -= chargedOpening;
		armPosition.y += leftChargedHeight;
		GO_PunchInstancesGroundSmash[0].transform.position = armPosition;
		
		armPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		armPosition.x += chargedOpening;
		armPosition.y += leftChargedHeight;
		GO_PunchInstancesGroundSmash[1].transform.position = armPosition;

	}


	void Update () {
		if(isGroundSmash){
			//Check if player reached the destiny
			if(Vector3.Distance(this.transform.position, groundSmashDestiny) <= 0){    
				GroundSmashHitGround();
			}
			//Still goin to the destiny
			else{
				this.transform.position = Vector3.MoveTowards(this.transform.position, groundSmashDestiny, groundSmashCurrentSpeed * Time.deltaTime);  
				groundSmashCurrentSpeed = groundSmashCurrentSpeed * (1 + groundSmashAcceleration);
			}
		}

		if(Input.GetButtonDown("GroundSmash")){
			if(!isGroundSmash && !gameObject.GetComponent<FPSWalkerEnhanced>().IsGroundedValue()){
				groundSmashIsHilight = true;
			}
		}

		if(Input.GetButtonUp("GroundSmash")){
			if (!isGroundSmash && groundSmashIsHilight && !gameObject.GetComponent<FPSWalkerEnhanced>().IsGroundedValue()){
				groundSmashIsHilight = false;
				GroundSmash();
			}
		}

		if(gameObject.GetComponent<FPSWalkerEnhanced>().IsGroundedValue()){
			groundSmashIsHilight = false;
		}
        
        //Detecs if the player can use the charged punch
		if(Input.GetButton("Fire2") && !isGroundSmash){
			if(groundSmashIsHilight){
				groundSmashIsHilight = false;
			}
			else{
				if(currCoolDownTimerCharged < 0 & currChargePower < 100 && !Input.GetButton("GroundSmash")){
					if(currChargePower < 100){
						currChargePower += Time.deltaTime*chargeRatio;
					}
					else{
						currChargePower = 100;
					}
				}
			}
		}

		if(groundSmashIsHilight){
			GroundSmashHilight();
		}
		
		//Detect if strong punch is activated
		if(Input.GetButtonUp("Fire2") && !isGroundSmash && !Input.GetButton("GroundSmash")){
			if(currCoolDownTimerCharged < 0){
				StrongPunch();
			}
		}

		//Detection for fast attacks
		if(Input.GetButton("Fire1") &&
		  	(currCoolDownTimerCharged < (coolDownTimerCharged - modelLifespanCharged)) &&
		   	!(Input.GetButton("Fire2") &&
		  	(currCoolDownTimerCharged < 0)) &&
		   	(!isGroundSmash)){
				if(currCoolDownTimerNormal < 0)
					Punch();
		}
        
		currCoolDownTimerNormal -= Time.deltaTime;
		currCoolDownTimerCharged -= Time.deltaTime;
	}

	void Punch(){
		//Detects punch collision
		RaycastHit hit = hitLine.checkHitDir(rotationController.transform.position, rotationController.transform.forward, punchDistante);

		//Creates hit effect
		if(hit.collider != null){
			if(1 << hit.collider.gameObject.layer == enemyLayer){
				SetEffect(hit);
				PushHit(hit);
			
			}
		}

		//Create normal punch
		GameObject GOInstPunch = GO_PunchInstancesNormal[Random.Range(0, maxHandsNormal-1)];

		//Modify current position
		Vector3 pos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		pos.x += Random.Range(-randX, randX);
		pos.y += Random.Range(-randY, randY);

		//Activate arm timer for destruction
		GOInstPunch.GetComponent<PunchClass>().Begin(pos, rotationController.transform.rotation, modelLifespanNormal);

		//Normal punch cooldown
		currCoolDownTimerNormal = coolDownTimerNormal;
	}

	void StrongPunch(){

        //Detects punch collision
        RaycastHit hit = hitLine.checkHitDir(rotationController.transform.position, rotationController.transform.forward, punchDistante);
		
		//Effect
		if(hit.collider != null){
			//Checking if hit an enemy
			if(1 << hit.collider.gameObject.layer == enemyLayer){
				SetEffect(hit);
				PushHit(hit);
			}
			//Check if it hit ground
			else if(1 << hit.collider.gameObject.layer == sceneLayer){
				this.playerBody.AddForce((playerBody.position - hit.point) * 1000);
			}
		}

		// Activate arms timer
		GO_PunchInstancesCharged[0].GetComponent<PunchClass>().Begin(GO_PunchInstancesCharged[0].transform.position, rotationController.transform.rotation, modelLifespanCharged);
		GO_PunchInstancesCharged[1].GetComponent<PunchClass>().Begin(GO_PunchInstancesCharged[1].transform.position, rotationController.transform.rotation, modelLifespanCharged);

		//Adjust strong punch cooldown
		currCoolDownTimerCharged = coolDownTimerCharged;
		currChargePower = 0;
	}

	void GroundSmash(){
		RaycastHit hit = hitLine.checkHitDir(rotationController.transform.position, rotationController.transform.forward, groundSmashDistance);
		if(hit.collider != null && hit.collider.gameObject == gameObject.GetComponent<FPSWalkerEnhanced>().GravityObject && (hit.distance > groundSmashMin)){
			int layerMask = 1 << 10;
			if (Physics.Raycast(rotationController.transform.position, rotationController.transform.forward, groundSmashDistance, layerMask)){
				isGroundSmash = true;
				//Groundsmash origin
				groundSmashOrigin = this.transform.position;
				//Groundsmash destiny
				groundSmashDestiny = hit.point;
				//Groundsmash initial speed
				groundSmashCurrentSpeed = groundSmashSpeed;

				this.GetComponent<Rigidbody>().isKinematic = true;

				//Deactivate already active arms
				if(GO_PunchInstancesCharged[0].activeSelf || GO_PunchInstancesCharged[1].activeSelf){
					GO_PunchInstancesCharged[0].GetComponent<PunchClass>().Finish();
					GO_PunchInstancesCharged[1].GetComponent<PunchClass>().Finish();
				}
				foreach(GameObject punch in GO_PunchInstancesNormal){
					if(punch.activeSelf){
						GO_PunchInstancesCharged[1].GetComponent<PunchClass>().Finish();
					}
				}

				//Activate ground smash arms
				GO_PunchInstancesGroundSmash[0].GetComponent<PunchClass>().Begin(GO_PunchInstancesGroundSmash[0].transform.position, rotationController.transform.rotation, Mathf.Infinity);
				GO_PunchInstancesGroundSmash[1].GetComponent<PunchClass>().Begin(GO_PunchInstancesGroundSmash[1].transform.position, rotationController.transform.rotation, Mathf.Infinity);
			}
		}
	}

	void GroundSmashHitGround(){
        //Groundsmash attack camera shake
		this.GetComponentInChildren<CameraShake>().Shake();
		isGroundSmash = false;
		GetComponent<Rigidbody>().isKinematic = false;

		if(GO_PunchInstancesGroundSmash[0].activeSelf || GO_PunchInstancesGroundSmash[1].activeSelf){
			GO_PunchInstancesGroundSmash[0].GetComponent<PunchClass>().Finish();
			GO_PunchInstancesGroundSmash[1].GetComponent<PunchClass>().Finish();
		}

		transform.Translate(0, heightOffset, 0);

		List<GameObject> enemyToHit = new List<GameObject>();
		List<Vector3> enemyHitPoint = new List<Vector3>();
        
		float damageDistance = 500;
        //Damage all enemies in a distance from the ground smash attack
		foreach(GameObject enemy in GroupController.instance.GetGroup("Enemy", this.transform.position, damageDistance)){
			RaycastHit[] hits = hitLine.checkHitDirAll(this.transform.position, enemy.GetComponent<Collider>().bounds.center - this.transform.position, damageDistance);
			foreach (RaycastHit hit in hits){
				if(hit.collider != null){
					//acerta inimigo
					if(1 << hit.collider.gameObject.layer == enemyLayer && hit.collider.gameObject == enemy){
						if(!enemyToHit.Contains(enemy)){
							enemyToHit.Add(enemy);
							enemyHitPoint.Add(hit.point);
						}
					}
				}
			}
		}
		for(int i = 0; i < enemyToHit.Count; i++){
			PushHit (enemyToHit[i], enemyHitPoint[i]);
			SetEffect(enemyToHit[i], enemyHitPoint[i]);
		}
	}

    //Creates a highlight for the ground smash to show the affected area (Not finished)
	void GroundSmashHilight(){
		int layerMask = 1 << 10;
		RaycastHit hitPoint;
		if (Physics.Raycast(rotationController.transform.position, rotationController.transform.forward, out hitPoint, groundSmashDistance, layerMask) && (hitPoint.collider.gameObject == gameObject.GetComponent<FPSWalkerEnhanced>().GravityObject)){
		}
		else{
		}
	}

	//Detects if the punch caused damaged with raycast
	void PushHit(RaycastHit hit){
		DamageHit dmgHit = hit.collider.GetComponentInChildren<DamageHit>();
		if(dmgHit == null)
			return;

		float myDamage = dmgHit.calculateDamage(damage);
		dmgHit.GetHit(myDamage, hit.point);

		if(damageFont != null){
			GameObject txt = GameObject.Instantiate(damageFont, hit.point, Quaternion.identity) as GameObject;
			txt.GetComponent<TextMesh>().text = myDamage.ToString();
		}
	}

    //Detects if the punch caused damaged with gameobject and direction
    void PushHit(GameObject hit, Vector3 point){
		DamageHit dmgHit = hit.GetComponentInChildren<DamageHit>();
		if(dmgHit == null)
			return;
		
		float myDamage = dmgHit.calculateDamage(damage);
		dmgHit.GetHit(myDamage, point);
		
		if(damageFont != null){
			GameObject txt = GameObject.Instantiate(damageFont, point, Quaternion.identity) as GameObject;
			txt.GetComponent<TextMesh>().text = myDamage.ToString();
		}
	}



	//Spawn punch effect
	void SetEffect(RaycastHit hit){
		var goEffect = Instantiate(GO_PunchEffect, hit.point, Quaternion.identity) as GameObject;
		goEffect.transform.parent = hit.collider.transform;
	}

	void SetEffect(GameObject hit, Vector3 point){
		var goEffect = Instantiate(GO_PunchEffect, point, Quaternion.identity) as GameObject;
		goEffect.transform.parent = hit.transform;
	}
}
