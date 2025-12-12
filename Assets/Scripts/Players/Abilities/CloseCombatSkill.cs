using System;
using System.Collections;
using UnityEngine;

public abstract class CloseCombatSkill : Skill
{
	//TEST CLASS FOR OVERRIDE PREPARE
	//private Vector3 _targetPoint = Vector3.positiveInfinity;

    protected override IEnumerator PrepareJob(Action<TargetInfo> targetDataSavedCallback)
    {
		//while (float.IsPositiveInfinity(_targetPoint.x) || GetTarget() == null)
		while (GetTargetCharacter() == null)
		{
			if (GetMouseButton)
			{
				//_targetPoint = GetMousePoint();

				FindTargetCharacter();
			}
			yield return null;
		}
		TargetInfo targetInfo = new TargetInfo();
		targetInfo.AddTarget(GetTargetCharacter());
		//targetInfo.Points.Add(_targetPoint);
		//callbackDataSaved(targetInfo);
	}
}
