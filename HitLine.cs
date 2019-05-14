using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Class that cast a raypoint into a direction and return either the first object it hits or all object it hits in a given direction

public class HitLine : MonoBehaviour {
	
	public LayerMask layerCollisions;

	public LayerMask sceneLayer;
	public LayerMask enemyLayer;

	protected List<RaycastHit> returnRaycasts;

	public void Awake(){
		returnRaycasts = new List<RaycastHit>();
	}

	public RaycastHit checkHit(Vector3 p1, Vector3 p2, float distanceLimit){
		Vector3 direction = p2 - p1;
		return checkHitDir(p1, direction, distanceLimit);
	}

	public RaycastHit checkHitDir(Vector3 p1, Vector3 direction, float distanceLimit){
		RaycastHit hit = new RaycastHit();
		Physics.Raycast(p1, direction, out hit, distanceLimit, layerCollisions);
		return hit;
	}

	public RaycastHit[] checkHitDirAll(Vector3 p1, Vector3 direction, float distanceLimit){
		returnRaycasts.Clear();

		RaycastHit[] hits = Physics.RaycastAll(p1, direction, distanceLimit, layerCollisions);

		float closestWallDist = Mathf.Infinity;

		foreach(RaycastHit hit in hits){
			if(1 << hit.collider.gameObject.layer == sceneLayer){
				float dist = Vector3.Distance(p1, hit.point);
				if(dist < closestWallDist){
					closestWallDist = dist;
				}
			}
		}
		foreach(RaycastHit hit in hits){
			if(1 << hit.collider.gameObject.layer == enemyLayer){
				float dist = Vector3.Distance(p1, hit.point);
				if(dist < closestWallDist){
					returnRaycasts.Add(hit);
				}
			}
		}

		return returnRaycasts.ToArray();
	}
}
