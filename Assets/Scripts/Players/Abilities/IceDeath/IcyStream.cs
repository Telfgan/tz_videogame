using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IcyStream : Skill
{
	[SerializeField] private IcyStreamProjectile _projectile;
	[SerializeField] private HeroComponent _playerLinks;
	[SerializeField] private SeriesOfStrikes _seriesOfStrikes;
	private bool _talent = false;

	private Vector3 _mousePos = Vector3.positiveInfinity;
	private Energy _energy;
	//private Character _target = null;
	//private RuneComponent _rune;

	protected override bool IsCanCast => IsCanCastCheck();

    protected override int AnimTriggerCastDelay => 0;

    protected override int AnimTriggerCast => 0;

    private bool IsCanCastCheck()
	{
		if(GetTargetCharacter() != null)
		{
			if(Vector3.Distance(GetTargetCharacter().transform.position, _playerLinks.transform.position) > Radius)
			{
				return false;
			}
		}
		return true;
	}

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

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        _mousePos = targetInfo.Points[0];
    }

    private void Shoot()
	{
		Debug.Log("shot");
		_mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector3 lookDir = _mousePos - _playerLinks.transform.position;
		float angle = Mathf.Atan2(lookDir.z, lookDir.x) * Mathf.Rad2Deg - 90f;
		CmdCreateProjecttile(angle);

		_projectile.gameObject.SetActive(true);
		_projectile.Init(_playerLinks, _energy.CurrentValue, _talent, this);

		float usedEnergy = 0;
		if (_energy.CurrentValue >= 40)
		{
			usedEnergy = 40;
		}
		else
		{
			usedEnergy = _energy.CurrentValue;
		}
		_energy.CmdUse(usedEnergy);
		_seriesOfStrikes.MakeHit(null, AbilityForm.Magic, 1, usedEnergy, 1);
	}

	[Command]
	private void CmdCreateProjecttile(float angle)
	{
		_projectile.gameObject.SetActive(true);
		_projectile.Init(_playerLinks, _energy.CurrentValue, _talent, this);

		/*IcyStreamProjectile projectile = Instantiate(_projectile, gameObject.transform.position, Quaternion.Euler(0, -angle, 0));
		SceneManager.MoveGameObjectToScene(projectile.gameObject, _hero.NetworkSettings.MyRoom);
		projectile.Init(_playerLinks, _energy.CurrentValue, _talent, this); //its talent bool, no last hit 

		NetworkServer.Spawn(projectile.gameObject);*/

		RpcInit(_projectile.gameObject, _energy.CurrentValue);
	}

	[ClientRpc]
	private void RpcInit(GameObject obj, float manaValue)
	{
		//obj.gameObject.SetActive(true);
		//obj.GetComponent<IcyStreamProjectile>().Init(_playerLinks, manaValue, false, this);
	}

	protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
	{
		//while (_target == null)
		while (float.IsPositiveInfinity(_mousePos.x))
		{
			if (GetMouseButton)
			{
				_mousePos = GetMousePoint();
				/*if (GetTarget() != null)
				{
					if (GetTarget().character != null)
					{
						_target = GetTarget().character;
					}
				}*/
				//_mousePos = GetMousePoint();
			}
			yield return null;
		}
		TargetInfo targetInfo = new TargetInfo();
		targetInfo.Points.Add(_mousePos);
		callbackDataSaved(targetInfo);
	}

	protected override IEnumerator CastJob()
	{
		Shoot();
		yield return null;
	}

	protected override void ClearData()
	{
		ClearTarget();
		//_target = null;
		StartCoroutine(TurnOff());
		//_projectile.gameObject.SetActive(false);
		//_projectile.Init(_playerLinks, _energy.CurrentValue, _talent, this);
		_mousePos = Vector3.positiveInfinity;
	}

	private IEnumerator TurnOff()
	{
		yield return new WaitForSeconds(3);
		_projectile.gameObject.SetActive(false);
	}

	public void Talent(bool value)
	{
		_talent = value;
	}
}
