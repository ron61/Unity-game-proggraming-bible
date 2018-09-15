using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OpenworldController : MonoBehaviour
{
	[SerializeField] CharacterController characterController;
	[SerializeField] float speed;
	[SerializeField] float rotateSpeed;

	[SerializeField] int timelimitHour = 24;
	[SerializeField] SkyboxController skyboxController;
	[SerializeField] GameObject gamePlayUIObject;
	[SerializeField] Text scoreText;
	[SerializeField] Text timelimitText;
	[SerializeField] GameObject gameEndUIObject;
	[SerializeField] Text gameEndScoreText;

	int scoreCount = 0;
	bool isGameEnd = false;

	void Update()
	{
		if (isGameEnd) return;

		CharacterMoveUpdate();
		TimelimitUpdate();
	}

	void CharacterMoveUpdate()
	{
		//横方向の入力を使用して方向転換する
		transform.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeed, 0);

		//移動速度と移動方向を計算する
		Vector3 forward = transform.transform.forward;
		float moveSpeed = speed * Input.GetAxis("Vertical");

		//方向に移動速度をかけて移動させる
		characterController.SimpleMove(forward * moveSpeed * Time.deltaTime);
	}

	void TimelimitUpdate()
	{
		timelimitText.text = string.Format("残り{0}時間", Mathf.CeilToInt(timelimitHour - skyboxController.TotalHour));

		if(skyboxController.TotalHour >= timelimitHour)
		{
			isGameEnd = true;
			gamePlayUIObject.SetActive(false);
			gameEndUIObject.SetActive(true);
			gameEndScoreText.text = scoreCount.ToString();
		}
	}

	void OnTriggerEnter(Collider collider)
	{
		if(collider.CompareTag("Collectable"))
		{
			scoreCount++;
			scoreText.text = scoreCount.ToString();

			Destroy(collider.gameObject);
		}
	}
}