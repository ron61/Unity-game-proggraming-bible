using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeMake : MonoBehaviour {
	[SerializeField]int mapSize = 5;		// マップサイズ(必ず奇数値)
	int[,] maze;			// マップ生成用(0=壁  1=通路)
	int cnt = 0;			// 迷路の壁を生成した回数

	[SerializeField]
	GameObject wallObject;	// 壁オブジェクトを設定

	[SerializeField]
	GameObject groundObject;// 地面オブジェクトを設定

	[SerializeField]
	GameObject GoalObject;	// ゴールオブジェクトを設定

	void Start () {

		// マップ生成終了条件を算出(マップ内で通路が置ける場所-1)
		int endNum = ((mapSize + 1) / 2) * ((mapSize + 1) / 2) - 1;

		maze = new int[mapSize+2, mapSize+2];

		while(endNum > cnt)
		{
			//Debug.Log(cnt);
			// ランダムで迷路を掘る場所を決める(必ず偶数になるようにする)
			int x = Random.Range (0, (mapSize + 1) / 2) * 2;
			int y = Random.Range (0, (mapSize + 1) / 2) * 2;


			if (cnt == 0) maze [x+1, y+1] = 1;						// 初回だけ予め穴を開けておく
			if (maze [x+1, y+1] == 1) WallDig (x, y, 0);

		}
			
		// パズル画面の表示
		Output ();

		// ゴールを描画
		Instantiate (GoalObject, new Vector3 (mapSize, 0, mapSize), Quaternion.identity);
	}



	// 迷路生成（穴掘り法）
	void WallDig(int x, int y, int oldVec){
		int[] vx = { 0, 2, 0, -2 };
		int[] vy = { -2, 0, 2, 0 };

		bool retFlg = false; 

		// 四方向をランダムで選んで2マス先が掘れるかどうかをチェック
		int r = Random.Range (0, 4);

		// マップの端は掘ることが出来ない
		if (r == 0 && y <= 0) retFlg = true;
		if (r == 1 && (x+2) >= mapSize) retFlg = true;
		if (r == 2 && (y+2) >= mapSize) retFlg = true;
		if (r == 3 && x <= 0) retFlg = true;


		// 問題があるときはもう一度やり直す
		if (retFlg) {
			WallDig (x, y, oldVec);
			return;
		}

		// 壁を掘れそうなら掘る
		if (maze [x+1 + vx [r], y+1 + vy [r]] == 0) {
			maze [x+1 + vx [r], y+1 + vy [r]] = 1;
			maze [x+1 + vx [r] / 2, y+1 + vy [r] / 2] = 1;
			cnt++;

			// 再帰ループ
			WallDig (x + vx[r], y + vy[r], r);
		}
	}




	// パズルをモデルを使って出力
	void Output () {
		GameObject obj = new GameObject();	// 親オブジェクトの中に格納する
		obj.name = "Mize";

		for (int x = 0; x < mapSize+2; x++) {
			for (int y = 0; y < mapSize+2; y++) {	
				if (maze [x, y] == 0) {
					Instantiate (wallObject, new Vector3 (x, 0, y), Quaternion.identity).transform.parent = obj.transform;
				} else {
					Instantiate (groundObject, new Vector3 (x, 0, y), Quaternion.identity).transform.parent = obj.transform;
				}
			}
		}
	}
}