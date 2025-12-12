using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class LastBreathCorpse : Skill
{
	[SerializeField] private Character _character;
	private float _timer = 12;
	private float _cooldown = 12;
	private bool _isAvaliable = true;

	protected override bool IsCanCast => true;

    protected override int AnimTriggerCastDelay => throw new System.NotImplementedException();

    protected override int AnimTriggerCast => throw new System.NotImplementedException();

    //private float _cooldown = 12;


    private void Update()
	{
		if (!_isAvaliable)
		{
			Timer();
		}
	}
    public override void LoadTargetData(TargetInfo targetInfo)
    {
        
    }

    private void Timer()
	{
		_timer -= Time.deltaTime;
		if (_timer < 0) 
		{
			_isAvaliable = true;
			_timer = _cooldown;
		}
	}

	protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
	{
		callbackDataSaved(null);
		yield return null;
	}

	protected override IEnumerator CastJob()
	{
		_character.CharacterState.CmdAddState(States.LastBreath, 12, 0, _character.gameObject, name);
		yield return null;
	}

	protected override void ClearData()
	{
		return;
	}
}
