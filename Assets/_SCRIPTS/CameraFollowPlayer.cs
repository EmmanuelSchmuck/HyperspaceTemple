using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour {

	[SerializeField] private GameObject Player;
	[SerializeField] private GameObject Target;
	[SerializeField] private float playerSpeedFactor = 1.0f;
	[SerializeField] private float smoothTime = 1.0f;
	[SerializeField] private float turnSpeed = 5.0f;

	private Vector3 velocity = Vector3.zero;
	private float playerSpeed = 0.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		transform.position = Vector3.SmoothDamp(transform.position, Target.transform.position, ref velocity, smoothTime/(1+playerSpeedFactor*playerSpeed));

		Quaternion targetRotation = Quaternion.LookRotation(Player.transform.position - transform.position);

		// Smoothly rotate towards the target point.
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
		
	}

	public void UpdatePlayerSpeed(float newSpeed){

		playerSpeed = newSpeed;

	}
}
