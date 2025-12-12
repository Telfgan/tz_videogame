using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MagicDefense : Skill
{
	[SerializeField] private PlagueAbsorption _plagueAbsorption;
	[SerializeField] private DeathSpiral _deathSpiral;
	[SerializeField] private HeroComponent _playerLinks;
	[SerializeField] private MagicDefenseArea _magDefArea;

	private Character _target;
	private bool _isArea = false;
	private Vector2 _position = Vector2.positiveInfinity;
	private Energy _energy;
	private RuneComponent _rune;

	protected override bool IsCanCast => CheckCanCast();

    protected override int AnimTriggerCastDelay => throw new System.NotImplementedException();

    protected override int AnimTriggerCast => throw new System.NotImplementedException();

    private bool CheckCanCast()
	{
		Debug.Log("Check");
		//return true;
		if (_target != null)
		return Vector3.Distance(_target.transform.position, transform.position) <= Radius;

		return Vector3.Distance(_position, transform.position) <= Radius;
	}

	private void Start()
	{
		for (int i = 0; i < _playerLinks.Resources.Count; i++)
		{
			if (_playerLinks.Resources[i].Type == ResourceType.Energy)
			{
				_energy = (Energy)_playerLinks.Resources[i];
			}
			if (_playerLinks.Resources[i].Type == ResourceType.Rune)
			{
				_rune = (RuneComponent)_playerLinks.Resources[i];
			}
		}

	}

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        _target = (Character)targetInfo.GetTargets()[0];
		_position = targetInfo.Points[0];
    }

    [Command]
	private void Shoot(GameObject targetGm)
	{
		Character target = targetGm.GetComponent<Character>();

		/*float boostHp = 0.1f + 0.003f * _energy.CurrentValue;
		_energy.CmdUse(_energy.CurrentValue);*/
		target.CharacterState.AddState(States.MagicBuff, 6, 200, _playerLinks.gameObject, name);

	}

	[Command]
	private void CmdSpawnArea(Vector3 position)
	{
		MagicDefenseArea area = Instantiate(_magDefArea, position, Quaternion.identity);
		area.Initialize(600, DamageType.Both);
		SceneManager.MoveGameObjectToScene(area.gameObject, _hero.NetworkSettings.MyRoom);

		NetworkServer.Spawn(area.gameObject);
		//RpcInit(area.gameObject, position);
	}
	

/*	[ClientRpc]
	private void RpcInit(GameObject area, Vector3 position)
	{
		MagicDefenseArea magArea = area.GetComponent<MagicDefenseArea>();
		//magArea.Init(_playerLinks, _energy.CurrentValue, false, this);
	}*/

	protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
	{
		while(_target == null && Vector2.Distance(_position, transform.position) > Radius)
		{
			Debug.Log("loop");
			if (GetMouseButton)
			{
			//	_target = GetRaycastTarget(true);
				_position = GetMousePoint();
			}
			yield return null;
		}
		TargetInfo targetInfo = new();
		targetInfo.AddTarget(_target);
		targetInfo.Points.Add(_position);
		callbackDataSaved(targetInfo);
	}

	protected override IEnumerator CastJob()
	{
		if(_target != null) 
		{
			Shoot(_target.gameObject);
		}
		//else if(_isArea) 
		{
			CmdSpawnArea(_position);
		}
		yield return null;
	}

	protected override void ClearData()
	{
		_target = null;
		_position = Vector2.positiveInfinity;
	}

	protected override bool TryPayCost(List<SkillEnergyCost> skillEnergyCosts, bool startCooldown = true)
	{
		Debug.Log("trypay");
		if (_target != null)
		{
			if (_deathSpiral.Chargers >= 1 && _plagueAbsorption.Charges >= 1 && _energy.CurrentValue >= 70 && _rune.CurrentValue >= 1)
			{
				Debug.Log("Casting");
				_rune.CmdUse(1);
				_energy.CmdUse(70);
				_plagueAbsorption.CmdUseCharge(1);
				_deathSpiral.CmdUseCharge(1);
				return true;
			}
		}
		else 
		if (_deathSpiral.Chargers >= 2 && _plagueAbsorption.Charges >= 2 && _energy.CurrentValue >= 70 && _rune.CurrentValue >= 2)
		{
			_isArea = true;
			_rune.CmdUse(2);
			_energy.CmdUse(70);
			_plagueAbsorption.CmdUseCharge(2);
			_deathSpiral.CmdUseCharge(2);
			return true;
		}
		return true;	
	}
}
