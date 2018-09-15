using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideObject : MonoBehaviour {
	[SerializeField]float time = 1.0f;

	void Start () {
		StartCoroutine("Hide");
	}

	// ゲームクリア時の処理
	IEnumerator Hide(){
		yield return new WaitForSeconds(time);
		gameObject.SetActive (false);
	}
}
