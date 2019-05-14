using UnityEngine;
using System.Collections;

//Script that makes the camera shake through script instead of animation

public class CameraShake : MonoBehaviour {
	
	private Vector3 localPosition;
	public float shakeDecay = 0.002f;
	public float shakeIntensity = 0.3f;
	protected float currentShakeDecay;
	protected float currentShakeIntensity;

	public bool isShaking = false;
	
	void Update () {
		if(isShaking){
			if (currentShakeIntensity > 0){

				transform.localPosition = new Vector3(
					localPosition.x + Random.Range (-currentShakeIntensity,currentShakeIntensity) * .2f,
					localPosition.y + Random.Range (-currentShakeIntensity,currentShakeIntensity) * .2f,
					localPosition.z);
				currentShakeIntensity -= currentShakeDecay;
			}
			else{
				isShaking = false;
			}
		}
	}

	public void Shake(){
		isShaking = true;
		localPosition = this.transform.localPosition;
		currentShakeIntensity = shakeIntensity;
		currentShakeDecay = shakeDecay;
	}
}
