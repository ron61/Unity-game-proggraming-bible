using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; 
using UnityEngine;
using UnityEngine.UI;

public class MoguraMain : MonoBehaviour {
	[SerializeField] public Text txtScore;			// スコア表示するテキストエリア
	[SerializeField] public Text txtTime;			// 制限時間を表示するテキストエリア
	[SerializeField] public float limitTime = 5.0f;	// ゲームの制限時間
	[SerializeField]GameObject GameOver;			// ゲームオーバー文字演出を設定
	[SerializeField]GameObject rockUI;				// UIイベントを設定

	[SerializeField] public float app_time = 1.0f;	// モグラの出現時間
	[SerializeField] public float interval_time = 1.0f;	// モグラが次に出るまでの時間

	[SerializeField] public int totalScore;			// スコアの合計値
	[SerializeField] public GameObject[] mogra = new GameObject[1]; // モグラモデルを設定
	[SerializeField] public int[] score = new int[1]; 				// モグラを叩いて得られる得点
	[SerializeField] public int[] rate 	= new int[1]; 				// モグラの出現確率

	[SerializeField] public GameObject[] effect 	= new GameObject[1]; 				// モグラの消去エフェクト

	bool  endFlg;							// ゲームが終了したかどか
	float time;								// 時間を記録


	// ゲーム開始前にゲームオーバー文字を隠す
	void Start () {
		GameOver.SetActive(false);
	}

	// 時間制限
	void Update () {
		float deltaTime = Time.deltaTime;

		time += deltaTime;			// 実時間を取得

		// 制限時間をカウントダウン
		if (!endFlg)
			limitTime -= deltaTime;		// 制限時間をカウント
		if (limitTime <= 0.0f) {
			StartCoroutine ("Game_Over");
		} else {
			txtTime.text = "Time:" + Mathf.Floor (limitTime);
		}
	}



	// ゲームオーバー(制限時間オーバー)時の処理
	IEnumerator Game_Over(){
		rockUI.SetActive(false);	// 操作出来ないようにする
		GameOver.SetActive(true);
		yield return new WaitForSeconds(2f);
		SceneManager.LoadScene ("Mogura/main");
	}
}
