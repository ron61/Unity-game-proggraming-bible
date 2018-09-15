using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortalCollision : MonoBehaviour
{
	[SerializeField] Text scoreText;

	public int scoreCount = 0;

	void OnTriggerEnter(Collider collider)
	{
		if (collider.CompareTag("Collectable"))
		{
			scoreCount++;
			scoreText.text = scoreCount.ToString();

			Destroy(collider.gameObject);
		}
	}
}
