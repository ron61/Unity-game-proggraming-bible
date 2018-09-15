using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;
using UnityEngine;

public class Playing : MonoBehaviour {
	[SerializeField]GameObject mainChar;	// キャラモデルを設定する
	[SerializeField]GameObject GoalEffect;	// クリア文字演出を設定
	[SerializeField]GameObject GameOver;	// ゲームオーバー文字演出を設定
	[SerializeField]GameObject rockUI;		// UIイベントを設定
	[SerializeField]Text txtLimitTime;		// 制限時間を表示するテキスト
	[SerializeField]float limitTime = 10.0f;// 制限時間

	GameObject insCtrl;						// 実際に表示するキャラ
	Animator insAnim;						// 表示キャラのアニメーションコンポーネント

	float roll;								// キャラの回転角度
	Vector3 move=new Vector3(1,0,1);		// キャラの位置座標
	float time;								// 時間を記録
	bool  endFlg;							// ゲームが終了したかどか

	//　事前にキャラモデルをインスタンス化しておく
	void Start () {
		insCtrl = Instantiate(mainChar);
		insAnim = insCtrl.GetComponent<Animator> ();
	}


	// 移動ボタンを押した時はキャラ座標を一歩前へ移動させる
	public void OnClick_Position () {
		if (time <= 0.2f) return;	// 一定時間ボタン連打禁止
		time = 0;
		move = insCtrl.transform.position + insCtrl.transform.forward;

		insAnim.Play ("walk");		// 歩行アニメーションの再生
	}


	// 回転ボタンを押した時は回転させる(rollValue=回転させたい角度)
	public void OnClick_Rotation (int rollValue) {
		if (time <= 0.2f) return;	// 一定時間ボタン連打禁止
		time = 0;
		roll += rollValue;
	}


	// ゴールボタンを押した時はゲームを終了する
	public void OnClick_Goal () {
		endFlg = true;				// ゲーム終了
		rockUI.SetActive(false);	// 操作出来ないようにする
		move = insCtrl.transform.position + insCtrl.transform.forward;
		insAnim.Play ("walk");		// 歩行アニメーションの再生
		GoalEffect.SetActive(true);
		StartCoroutine("End_Game");
	}


	// ゲームクリア時の処理
	IEnumerator End_Game(){
		yield return new WaitForSeconds(1.1f);
		SceneManager.LoadScene ("Mize/main");
	}

	// ゲームオーバー(制限時間オーバー)時の処理
	IEnumerator Game_Over(){
		rockUI.SetActive(false);	// 操作出来ないようにする
		GameOver.SetActive(true);
		yield return new WaitForSeconds(2f);
		SceneManager.LoadScene ("Mize/main");
	}

	// キャラの回転と移動処理
	void Update () {
		float deltaTime = Time.deltaTime;

		time += deltaTime;			// 実時間を取得

		// 制限時間をカウントダウン
		if (!endFlg) limitTime -= deltaTime;		// 制限時間をカウント
		if (limitTime <= 0.0f) {
			StartCoroutine ("Game_Over");
		} else {
			txtLimitTime.text = "Time:"+ Mathf.Floor(limitTime);
		}

		// キャラの回転と移動
		Transform myChar = insCtrl.transform;
		Quaternion toRoll = Quaternion.AngleAxis (roll, Vector3.up);
		myChar.rotation = Quaternion.Lerp(myChar.rotation, toRoll, Time.deltaTime * 20);
		myChar.position = Vector3.Lerp(myChar.position, move, Time.deltaTime * 20);
	}

}
