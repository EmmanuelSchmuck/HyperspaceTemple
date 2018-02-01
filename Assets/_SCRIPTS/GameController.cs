using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {


	[SerializeField] private int targetFPS = 40;
	[SerializeField] private float audioStartTime = 10.0f;
	private AudioSource _Audio;
	[SerializeField] private AnimationCurve _MusicVolumeCurve;
	[SerializeField] private float timeToMaxVolume;
	[SerializeField] private float minVolume;
	[SerializeField] private float maxVolume;
	[SerializeField] private float victoryDelay;
	[SerializeField] private GameObject fadeScreenStart;
	[SerializeField] private GameObject fadeScreenEnd;

	[SerializeField] private GameObject cam ;
	[SerializeField] private GameObject player ;
	[SerializeField] private GameObject menu ;
	[SerializeField] private GameObject victory ;
	[SerializeField] private Transform victoryPoint ;
	[SerializeField] private GameObject progress ;
	[SerializeField] private Text progressText ;
	[SerializeField] private Vector3 camStart;
	[SerializeField] private Vector3 playerStart;
	private bool gameIsOn = false;
	private bool victorious = false;
	private float completion = 0f;
	private float distToVictory = 0f;
	private float timeSinceStart;
	private float timeSinceVictory;



	// Use this for initialization
	void Start () {

		distToVictory = Vector3.Distance (playerStart, victoryPoint.position);
		_Audio = GetComponent<AudioSource> ();
		//_Audio.time = audioStartTime;

		Application.targetFrameRate = targetFPS;

		StartMenu ();

		
	}
	
	// Update is called once per frame
	void Update () {

		if (gameIsOn) {
			timeSinceStart += Time.deltaTime;
			_Audio.volume = Mathf.Lerp(minVolume,maxVolume,_MusicVolumeCurve.Evaluate(timeSinceStart/timeToMaxVolume));
			completion = 1.0f - Vector3.Distance (player.transform.position, victoryPoint.position) / distToVictory;
			progressText.text = "Progress : " + (int)(100 * completion) + " %";
			if (completion > 0.98f) {
				Victory ();
			}

			if (Input.GetKeyDown(KeyCode.Escape)){

				StartMenu ();
			}
		}
		if (victorious) {
			timeSinceVictory += Time.deltaTime;
			_Audio.volume = Mathf.Lerp(maxVolume,0f,_MusicVolumeCurve.Evaluate(timeSinceVictory/victoryDelay));
			if (timeSinceVictory > victoryDelay) {
				StartMenu ();
			}
		}

	}

	void Victory(){
		fadeScreenEnd.SetActive (true);
		victorious = true;
		victory.SetActive (true);
		progress.SetActive (false);
	}

	public void StartMenu(){
		_Audio.Stop ();
		fadeScreenStart.SetActive (false);
		fadeScreenEnd.SetActive (false);
		victory.SetActive (false);
		player.SetActive (false);
		menu.SetActive (true);
		progress.SetActive (false);
		gameIsOn = false;
		victorious = false;
		cam.GetComponent<CameraFollowPlayer> ().enabled = false;
		cam.transform.position = camStart;
		cam.transform.forward = Vector3.forward;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	public void ReStart(){
		gameIsOn = true;
		victorious = false;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		player.SetActive (true);
		progress.SetActive (true);
		cam.GetComponent<CameraFollowPlayer> ().enabled = true;
		cam.transform.position = camStart;
		player.transform.position = playerStart;
		fadeScreenStart.SetActive (true);
		fadeScreenEnd.SetActive (false);
		menu.SetActive (false);
		timeSinceStart = 0;
		_Audio.time = audioStartTime;
		_Audio.Play ();
	}

	public void Exit(){
		Application.Quit ();
	}
}
