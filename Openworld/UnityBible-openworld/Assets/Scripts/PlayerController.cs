using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[SerializeField]
	CharacterController characterController;
	[SerializeField]
	float speed;
	[SerializeField]
	float rotateSpeed;

	void Update()
	{
		CharacterMoveUpdate();
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
}
