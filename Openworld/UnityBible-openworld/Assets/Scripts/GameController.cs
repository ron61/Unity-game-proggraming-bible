using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
	[SerializeField] int timelimitHour = 24;
	[SerializeField] SkyboxController skyboxController;
	[SerializeField] GameObject gamePlayUIObject;
	[SerializeField] Text timelimitText;
	[SerializeField] GameObject gameEndUIObject;
	[SerializeField] Text gameEndScoreText;
	[SerializeField] PortalCollision portalCollision;
	
	bool isGameEnd = false;

	void Update()
	{
		if (isGameEnd) return;

		TimelimitUpdate();
	}

	void TimelimitUpdate()
	{
		timelimitText.text = string.Format("残り{0}時間", Mathf.CeilToInt(timelimitHour - skyboxController.TotalHour));

		if (skyboxController.TotalHour >= timelimitHour)
		{
			isGameEnd = true;
			gamePlayUIObject.SetActive(false);
			gameEndUIObject.SetActive(true);
			gameEndScoreText.text = portalCollision.scoreCount.ToString();
		}
	}

	
}
