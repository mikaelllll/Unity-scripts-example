using UnityEngine;
using System.Collections;
using UnityEditor;

//Class to show the planet gravity range and force through a wire disc handler

[CustomEditor(typeof(PlanetData))]
public class PlanetDataEditor : Editor {

	public PlanetData data;
	public GameObject myTarget;

	void Awake(){
		data = target as PlanetData;
		myTarget = data.gameObject;
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if(GUILayout.Button("Set BoundArea"))
		{
			data.CalculateBoundArea();
		}
	}

	void OnSceneGUI() {
		Handles.DrawWireDisc(myTarget.transform.position, Vector3.up, data.gravityForce);

		data.gravityForce = Handles.ScaleValueHandle(data.gravityForce,
		                                             myTarget.transform.position + new Vector3(data.gravityForce,0,0),
		                                             Quaternion.identity,
		                                             20,
		                                             Handles.CylinderHandleCap,
		                                             2);
		if (GUI.changed)
			EditorUtility.SetDirty(myTarget);
	}
}
