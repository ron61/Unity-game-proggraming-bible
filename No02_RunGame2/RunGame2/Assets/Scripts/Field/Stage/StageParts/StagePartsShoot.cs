using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StagePartsShoot : StageParts
{
	[SerializeField] StageEvent stageEvent;
	[SerializeField] GameObject wall;
	[SerializeField] List<GameObject> shootPoints;

	List<GameObject> targets = new List<GameObject>();

	public override void Initialize(int xNumber, int zNumber)
	{
		base.Initialize(xNumber, zNumber);
		stageEvent.onEventTap = OnTap;
		stageEvent.onEventSuccess = OnSuccess;
		stageEvent.onEventFailed = OnFailed;

		// タップ数分だけランダムに射撃目標を選ぶ
		var candidate = new List<GameObject>(shootPoints);
		for (int i = 0; i < stageEvent.NeedTapCount; i++)
		{
			var target = candidate[Random.Range(0, candidate.Count)];
			targets.Add(target);
			candidate.Remove(target);
		}
		foreach (var g in candidate) g.SetActive(false); // 選択から外れたのは非表示にしておく
	}

	void OnTap(RunCharacter character, int remainCount)
	{
		var target = targets[remainCount];
		character.Shoot(target.transform.position);
		target.SetActive(false);
	}

	void OnSuccess(RunCharacter character)
	{
		wall.SetActive(false);
	}

	void OnFailed(RunCharacter character)
	{
	}
}
