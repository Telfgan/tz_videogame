using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IceShard : Skill
{
	[SerializeField] private IceShardProjectile _projectile;
	[SerializeField] private HeroComponent _playerLinks;
	[SerializeField] private SeriesOfStrikes _seriesOfStrikes;

	private Vector3 _mousePos = Vector2.positiveInfinity;
	private bool _talentPlague = false;
	private bool _talentChragesPlague = false;
	private Energy _energy;

	protected override bool IsCanCast => true;

    protected override int AnimTriggerCastDelay => 0;

    protected override int AnimTriggerCast => 0;

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

	private void Shoot()
	{
		Vector3 lookDir = _mousePos - _playerLinks.transform.position;
		float angle = Mathf.Atan2(lookDir.z, lookDir.x) * Mathf.Rad2Deg - 90f;
		_seriesOfStrikes.MakeHit(null, AbilityForm.Magic, 1, 5, 3);

		CmdCreateProjecttile(angle, _energy.CurrentValue, _talentPlague, _talentChragesPlague);
	}

	[Command]
	private void CmdCreateProjecttile(float angle, float manaValue, bool talentPlague, bool talentChargesPlague)
	{
		IceShardProjectile projectile = Instantiate(_projectile, gameObject.transform.position, Quaternion.Euler(0, -angle, 0));
		SceneManager.MoveGameObjectToScene(projectile.gameObject, _hero.NetworkSettings.MyRoom);
		projectile.Init(_playerLinks, manaValue, false, this);
		projectile.Talents(talentPlague, talentChargesPlague);

		NetworkServer.Spawn(projectile.gameObject);

		RpcInit(projectile.gameObject, manaValue, talentPlague, talentChargesPlague);
	}

	[ClientRpc]
	private void RpcInit(GameObject obj, float manaValue, bool talentPlague, bool talentChargesPlague)
	{
		obj.GetComponent<IceShardProjectile>().Init(_playerLinks, manaValue, false, this);
	}

	public void TalentPlague(bool value)
	{
		_talentPlague = value;
	}
	public void TalentChargesPlague(bool value)
	{
		_talentChragesPlague = value;
	}
    public override void LoadTargetData(TargetInfo targetInfo)
    {
        _mousePos = targetInfo.Points[0];
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
	{
		//Debug.Log("MOUSE POS " + float.IsPositiveInfinity(_mousePos.x));
		while (float.IsPositiveInfinity(_mousePos.x))
		{
			if (GetMouseButton)
			{
				_mousePos = GetMousePoint();
				/*if (GetTarget().character == null)
				{
					_mousePos = GetTarget().Position;
				}
				else
				{
					_mousePos = GetTarget().character.transform.position;
				}*/
			}
			yield return null;
		}
		TargetInfo targetInfo = new();
		targetInfo.Points.Add( _mousePos );
		callbackDataSaved( targetInfo );
	}

	protected override IEnumerator CastJob()
	{
		Shoot();
		yield return null;
	}

	protected override void ClearData()
	{
		Debug.Log("CLEARED");
		_mousePos = Vector2.positiveInfinity;
	}
}
