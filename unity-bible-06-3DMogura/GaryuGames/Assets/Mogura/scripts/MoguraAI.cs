using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoguraAI : MonoBehaviour {
	float t;			// 時間を集計
	Animator anim;		// モグラのアニメーションを取得 
	GameObject insMogu;	// モグラモデルを一時的に格納 
	int mode;			// モグラの状態(0=出現, 1=出現中, 2=撤退)
	int moguNo;			// 読込まれたモグラの配列番号
	int totalRate;

	MoguraMain main;	// 別のスクリプトを読込む


	void Start () {
		// GameControllerから初期設定値を読み込む
		main = GameObject.Find("GameController").GetComponent<MoguraMain> ();
		anim = gameObject.GetComponent<Animator>();		// モグラのアニメーションを取得

		// モグラの出現割合の合計値を求める
		for (int i = 0; i < main.rate.Length; i++) {
			totalRate += main.rate[i];
		}
	}


	int sum;
	void Update () {


		switch (mode) {
		case 0:		// モグラの出現
			// 一定時間経過までモグラを出現させない
			t += Time.deltaTime;
			if (t < main.interval_time)	return;	
			t = 0.0f;


			int rate = Random.Range (0, totalRate) + 3;		// もぐらの出現率を出す
			sum = 0;
			// 各モグラの出現率が乱数の値より大きい場合モグラを出現させる
			for (int i = 0; i < main.rate.Length; i++) {
				sum += main.rate[i];
				if (sum >= rate) {
					if (insMogu)
						return;	// モグラが消えずに残っている場合は処理をスキップする

					// モグラモデルの読み込み
					moguNo = i;
					insMogu = Instantiate (main.mogra [moguNo]);
					insMogu.transform.parent = gameObject.transform;
					insMogu.transform.localPosition = Vector3.zero;
					insMogu.transform.localScale = Vector3.one;
					mode = 1;
				}
			}
			break;
			
			case 1:		// モグラを一定時間表示させる
			t += Time.deltaTime;
			if (t > main.app_time) {
				t = 0.0f;
				insMogu.GetComponent<Animator>().Play ("in");
				mode = 2;
			}
			break;

			case 2:		// モグラの撤退
			t += Time.deltaTime;
			if (t > 0.5f) {
				Destroy (insMogu);
				t = 0.0f;
				mode = 0;
			}
			break;
		}
	}



	// モグラを叩いた
	public void Mogra_Click(){
		if (mode == 1) {
			//anim.Play ("none");

			// スコアの集計と表示
			main.totalScore += main.score[moguNo];
			main.totalScore = (main.totalScore < 0) ? 0 : main.totalScore;	// 下限処理
			main.txtScore.text = "SCORE:" + main.totalScore;

			GameObject ef = Instantiate(main.effect[moguNo]);
			ef.transform.parent = gameObject.transform;
			ef.transform.localPosition = new Vector3(0, 0, -2.5f);
			ef.transform.localScale 	= Vector3.one;
			Destroy (insMogu);
			t = 0.0f;
			mode = 0;
		}
	}
}
