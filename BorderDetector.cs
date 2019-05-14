using UnityEngine;
using System.Collections;
using UnityEditor;

//Script to create a button for the FPSWalkerEnhanced to recalculate the object colision point and draw it on the unity GUI.
//Since the FPSWalkerEnhanced is a player controller for different types of planets (the player can walk through different directions and walks like in super mario galaxy)
//The player object will rotate, and so this lower bound point indicates the contact point of the player feet. Used in checking conditions wether the player
//is jumping or not

[CustomEditor(typeof(FPSWalkerEnhanced))]
public class BorderDetector : Editor {

	public FPSWalkerEnhanced walker;
	public GameObject myTarget;

	protected Vector3 point = new Vector3();
	protected float dotSize = 10.0f;

	protected bool activated = false;

	Vector3 max;
	Vector3 min;

	void Awake(){
		walker = target as FPSWalkerEnhanced;
		myTarget = walker.gameObject;
	}

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		if(GUILayout.Button("Recalculate lower bound"))
		{
			walker.DetectColisionPoint();

			EditorUtility.SetDirty(walker);
			activated = true;

		}
		dotSize = EditorGUILayout.Slider("Dot Size", dotSize, 1f, 100.0f);
	}

	public void OnSceneGUI() {
		if(activated == true){
			Handles.SphereHandleCap(0, myTarget.transform.position + (myTarget.transform.rotation * walker.downColliderBound) , myTarget.transform.rotation, dotSize / 100.0f, EventType.Repaint);
		}
	}
}
