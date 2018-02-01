using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateRotators : MonoBehaviour {

	[SerializeField] private List<RotateChunk> _List;

	void OnTriggerEnter(){

		for (int i = 0; i < _List.Count; i++) {
			_List [i].SetState (true);
		}
	}

	void OnTriggerExit(){

		for (int i = 0; i < _List.Count; i++) {
			_List [i].SetState (false);
		}
	}
}
