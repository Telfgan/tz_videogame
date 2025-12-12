using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class IceShield : Skill
{
	[SerializeField] private float _percentOfShield = 0.9f;
	[SerializeField] private float _decreaseSpeed = 0.2f;
	[SerializeField] private HeroComponent _playerLinks;
	[SerializeField] private SeriesOfStrikes _combo;
	[SerializeField] private IceShieldObject _shield;

	private bool _active = false;
	private float _timer = 1f;
	private Energy _energy;

	protected override bool IsCanCast => true;

    protected override int AnimTriggerCastDelay => throw new System.NotImplementedException();

    protected override int AnimTriggerCast => throw new System.NotImplementedException();

    private void Start()
	{
		for (int i = 0; i < _playerLinks.Resources.Count; i++)
		{
			if (_playerLinks.Resources[i].Type == ResourceType.Energy)
			{
				_energy = (Energy)_playerLinks.Resources[i];
			}
		}
	}

	private void Update()
	{
		if (!_active) return;

		_timer -= Time.deltaTime;
		if (_timer <= 0)
		{
			_timer = 1;
			if (_energy != null)
			{
				_energy.CmdUse(1);

				if (_energy.CurrentValue <= 0)
				{
					_active = false;
					_shield.gameObject.SetActive(_active);
					_shield.SetActive(_active);
					CmdRemoveShield();
				}
			}

		}
	}
    public override void LoadTargetData(TargetInfo targetInfo)
    {
        
    }

    private void Shoot() 
	{
		_active = !_active;
		Debug.Log(_playerLinks.Health.Shields.Count);

		_shield.gameObject.SetActive(_active);
		_shield.SetActive(_active);
		
		if (_active) 
		{
			CmdAddShield();
			_playerLinks.Move.ChangeMoveSpeed(0.8f);			
		}
		else
		{
			CmdRemoveShield();
			_playerLinks.Move.ChangeMoveSpeed(1.25f);
		}
	}

	/*private void Timer()
	{
		if (_active)
		{
			_timer -= Time.deltaTime;
			if (_timer > 0) return;

		/*	if (_character.Stamina.TryUse(1))
			{
				_timer = _delay;
			}
			else
			{
				_active = false;
			}
		
		}
	}*/

	protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
	{
		yield return null;
	}

	protected override IEnumerator CastJob()
	{
		Shoot();
		yield return null;
	}

	protected override void ClearData()
	{
		
	}

	[Command]
	private void CmdAddShield()
	{
		_shield.gameObject.SetActive(true);
		_shield.SetActive(true);
		_shield.Initialize(1000, DamageType.Both, 0.9f);
		_playerLinks.Health.Shields.Add(_shield);
		ClientRpcSwitchShield(true);
	}

	[Command]
	private void CmdRemoveShield()
	{
		_shield.gameObject.SetActive(false);
		_shield.SetActive(false);
		_playerLinks.Health.Shields.Remove(_shield);
		ClientRpcSwitchShield(false);
	}

	[ClientRpc]
	private void ClientRpcSwitchShield(bool value)
	{
		_shield.gameObject.SetActive(value);
		_shield.SetActive(value);
	}

	/*[ClientRpc]
	private void ClientRpcRemoveShield()
	{
		_shield.gameObject.SetActive(false);
		_shield.SetActive(false);
	}*/
}

