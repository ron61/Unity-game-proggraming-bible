using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class hit : MonoBehaviour {
	Button button;
	GameObject goal_UI;

	//　各種UIを取得する/UI画面の初期化
	void Awake () {
		button = GameObject.Find ("Canvas/Panel/Button_Front").GetComponent<Button> ();
		goal_UI = GameObject.Find ("Canvas/Panel/Button_Goal");
		goal_UI.SetActive(false);
	}


	// 進むボタンを非表示にする
	void OnTriggerStay(Collider collider)  {
		string name = LayerMask.LayerToName (collider.gameObject.layer);
		if (name == "Goal") goal_UI.SetActive (true);		// ゴールが手前にあるときは専用ボタンを表示
		button.interactable = false;

	}

	// 進むボタンを表示する
	void OnTriggerExit(Collider collider) { 
		string name = LayerMask.LayerToName (collider.gameObject.layer);
		if (name != "Goal") goal_UI.SetActive (false);		// 進むボタンに戻す
		button.interactable = true;
	}
}
