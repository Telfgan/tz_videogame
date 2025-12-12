using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CircularFrosting : Skill
{
	//[SerializeField] private CircularFrostingObject _circle;
	//[SerializeField] private FrostingFrozenTalant _talant;
	[SerializeField] private SeriesOfStrikes _seriesOfStrikes;

	private float _baseDuration = 2;
	private float _duration = 2;
	private Energy _energy;
	private bool _talentFrostingFrozen;

	protected override bool IsCanCast => true;

	protected override int AnimTriggerCastDelay => 0;

    protected override int AnimTriggerCast => 0;

    private void Start()
	{
		for (int i = 0; i < Hero.Resources.Count; i++)
		{
			if (Hero.Resources[i].Type == ResourceType.Energy)
			{
				_energy = (Energy)Hero.Resources[i];
			}
		}

	}

    public override void LoadTargetData(TargetInfo targetInfo)
    {

    }

    protected override IEnumerator CastJob()
	{
		CreateSmoke();
		yield return null;
	}

	protected override void ClearData()
	{
		
	}

	protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
	{
		callbackDataSaved(null);
		yield return null;
	}

	private void CreateSmoke()
	{
		Collider[] enemyDetected = Physics.OverlapSphere(transform.position, Radius);
		float usedEnergy = 0;
		if (_energy.CurrentValue >= 30)
		{
			_duration = _baseDuration + 3;
			usedEnergy = 30;
			_energy.CmdUse(30);
		}
		else
		{
			_duration = _baseDuration + _energy.CurrentValue / 10;
			usedEnergy = _energy.CurrentValue;
			_energy.CmdUse(_energy.CurrentValue);
		}
		foreach (var enemy in enemyDetected) 
		{
			Debug.Log(enemy);
			if (enemy.TryGetComponent<Character>(out var enemyCharacter))
			{
				if (enemyCharacter != Hero)
				{
					_seriesOfStrikes.MakeHit(enemyCharacter, AbilityForm.Magic, 1, usedEnergy, 0);
					CmdAdd(enemy.gameObject);
					//enemyCharacter.CharacterState.CmdAddState(States.Frosting, _duration, 0, _playerLinks.gameObject, name);
				}
				/*if (_talant != null)
				{
					if (_talant.IsActive)
					{
						enemyCharacter.CharacterState.CmdAddState(States.Frozen, _duration, 0);
						//enemyCharacter.CharacterState.AddState(new FrozenState(), _duration, 0, States.Frozen);
					}
				}*/
			}
		}
		//var smoke = Instantiate(_circle, transform);
		//smoke.dad = _links;
		//_canCast = false;
	}

	[Command]
	private void CmdAdd(GameObject enemy)
	{
		Character enemyCharacter = enemy.GetComponent<Character>();
		if (_talentFrostingFrozen && enemyCharacter.CharacterState.CheckForState(States.Frosting))
		{
			enemyCharacter.CharacterState.AddState(States.Frozen, _duration, 0, Hero.gameObject, name);
		}

		enemyCharacter.CharacterState.AddState(States.Frosting, _duration, 0, Hero.gameObject, name);
	}

	public void SetTalentFrostingFrozen(bool value)
	{
		_talentFrostingFrozen = value;
	}
}
