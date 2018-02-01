using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

// handle player movement and collision with obstacles
public class PlayerController : MonoBehaviour {

	[SerializeField] private float  maxturnspeed = 1;
	[SerializeField]  private float maxforwardspeed = 10;
	[SerializeField]  private float accelTime = 10;
	[SerializeField]  private AnimationCurve accelCurve;
	[SerializeField] private GameController _GC;
	[SerializeField]  private GameObject cam ;
	[SerializeField]  private ImageProcessing imgProcessing ;
	[SerializeField] private AudioClip _deathSound;

	private float forwardspeed = 0;
	private float turnspeed = 0;
	private float timer = 0;
	private Vector2 objecPosition = Vector2.zero;
	private Vector3 forwardTarget = Vector3.forward;

	
	// Update is called once per frame
	void Update () {

		Movement ();
	}

	void Movement(){

		// object position is extracted from the processed webcam flux
		objecPosition = imgProcessing.GetObjectPosition ();

		forwardspeed = maxforwardspeed;
		turnspeed = maxturnspeed;

		if (timer < accelTime) {
			timer += Time.deltaTime;
			forwardspeed = maxforwardspeed * accelCurve.Evaluate (timer / accelTime);
			turnspeed = maxturnspeed * accelCurve.Evaluate (timer / accelTime);
			cam.GetComponent<CameraFollowPlayer> ().UpdatePlayerSpeed (forwardspeed/maxforwardspeed);
		} 
	

		forwardTarget = Vector3.forward + new Vector3(objecPosition.x, objecPosition.y, 0f) ;
		forwardTarget.Normalize ();

		transform.forward = Vector3.RotateTowards (transform.forward, forwardTarget, 0.005f * turnspeed, 0f);

		GetComponent<CharacterController> ().Move (transform.forward * forwardspeed * Time.deltaTime);

	}

	// collision with obstacle
	void OnCollisionEnter(Collision col){

		timer = 0;
		GetComponent<Rigidbody> ().velocity = Vector3.zero;
		GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
		transform.position = Vector3.zero;
		transform.forward = Vector3.forward;
		cam.transform.position = new Vector3 (0, 0, -5);
		GetComponent<AudioSource> ().PlayOneShot (_deathSound, 0.25f);
		_GC.ReStart ();
	}
}
