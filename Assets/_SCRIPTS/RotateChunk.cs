using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateChunk : MonoBehaviour {
	
	[SerializeField] private float RotateSpeed = 1.0f;
	private Vector3 RotationCenter = new Vector3(1,7.5f,0);
	private Transform trans;
	private bool active = false;

	// Use this for initialization
	void Start () {

		active = false;
		trans = this.transform;
		
	}
	
	// Update is called once per frame
	void Update () {
		if(active)
		trans.RotateAround (RotationCenter, Vector3.forward, RotateSpeed * Time.deltaTime);
	}

	public void SetState(bool state){
		active = state;
		
	}
		
}
