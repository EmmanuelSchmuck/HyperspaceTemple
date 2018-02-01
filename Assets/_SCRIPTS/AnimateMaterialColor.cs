using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateMaterialColor : MonoBehaviour {

	[SerializeField] private Material LightWallMat;
	[SerializeField] private Color C1;
	[SerializeField] private Color C2;
	[SerializeField] private float ColorChangeSpeed = 1.0f;
	[SerializeField] private float ColorIntensity = 1.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		LightWallMat.SetColor("_EmissionColor",Color.Lerp(C1,C2,0.5f+0.5f*Mathf.Cos(ColorChangeSpeed*Time.time/100)) * ColorIntensity);
	
		
	}
}
