using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Absorption : Skill
{
	[SerializeField] private Character _playerLinks;
	//private IcyCorpse _target;
	private Energy _energy;

	protected override bool IsCanCast
	{
		get { return GetTargetCharacter() != null; }
	}

    protected override int AnimTriggerCastDelay => throw new System.NotImplementedException();

    protected override int AnimTriggerCast => throw new System.NotImplementedException();

    /*private void Update()
	{
		if(!_active) return;

		if (Input.GetMouseButton(0))
		{
			var _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			RaycastHit2D[] hits =
				Physics2D.CircleCastAll(_mousePos, _radius, Vector2.zero);
			for (int i = 0; i < hits.Length; i++)
			{
				if (hits[i].collider.TryGetComponent<IcyCorpse>(out var shadow))
				{
					CmdAction(shadow.gameObject);
					if (shadow.IsAlive)
					{
						CmdAction(shadow.gameObject);
					}
				}
			}
			_active = false;
		}
	}*/
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
        SetTarget((IcyCorpse)targetInfo.GetTargets()[0]);
    }

    [Command]
	private void CmdAction(GameObject bodyObj)
	{
		Debug.Log(bodyObj.name);
		Action(bodyObj);
		//RpcAction(bodyObj);
	}

	[ClientRpc]
	private void RpcAction(GameObject bodyObj) 
	{
		Debug.Log(bodyObj.name);
		Action(bodyObj);
	}

	private void Action(GameObject bodyObj)
	{
		Debug.Log(bodyObj.name);
		IcyCorpse body = bodyObj.GetComponent<IcyCorpse>();
		//NetworkServer.UnSpawn(body.gameObject);
		//float regen = 0.1f * body.HP + 0.05f * _player.Stamina.Value / 10;
		//_energy.TryUse(_energy.CurrentValue);
		//_player.Health.AddHeal(regen);
		body.DestroyCorpse();

	}

	protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
	{
		while (GetTargetCharacter() == null)
		{
			if (GetMouseButton)
			{
				FindTargetCharacter();
				if(GetTargetCharacter() is IcyCorpse)
				{

				}
				else
				{
					ClearTarget();
				}
					//_target = (IcyCorpse)GetRaycastTarget();
			}
			yield return null;
		}
		TargetInfo targetInfo = new();
		targetInfo.AddTarget(GetTargetCharacter());
		callbackDataSaved(targetInfo);
	}

	protected override IEnumerator CastJob()
	{
		Debug.Log("cast job");
		CmdAction(GetTargetCharacter().gameObject);

		yield return null;
	}

	protected override void ClearData()
	{
		ClearTarget();
		//_target = null;
		return;
	}
}
