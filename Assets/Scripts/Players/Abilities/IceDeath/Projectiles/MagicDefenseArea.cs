using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class MagicDefenseArea : Shield
{
	private float _shieldCapacity = 600;

	private void Start()
	{
		StartCoroutine(DestroyObj());
	}

	[Server]
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.TryGetComponent<Character>(out var character))
		{
			Debug.Log("added server");
			character.Health.Shields.Add(this);
			TargetRpcAdd(collision.gameObject);
			//character.CharacterState.AddState(States.MagicBuff, 10, _shieldCapacity, null, name);
		}
	}

	[Server]
	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.TryGetComponent<Character>(out var character))
		{
			Debug.Log("remove server");
			character.Health.Shields.Remove(this);
			TargetRpcRemove(collision.gameObject);
			//character.CharacterState.CmdAddState(States.MagicBuff, 10, _shieldCapacity + _energyDad * 30, _dad.gameObject, _skill.name);
		}
	}

	[ClientRpc]
	private void TargetRpcAdd(GameObject target)
	{
		Debug.Log("added target");
		Character character = target.GetComponent<Character>();
		character.Health.Shields.Add(this);
	}

	[ClientRpc]
	private void TargetRpcRemove(GameObject target)
	{
		Debug.Log("remove target");
		Character character = target.GetComponent<Character>();
		character.Health.Shields.Remove(this);
	}
	private IEnumerator DestroyObj()
	{
		yield return new WaitForSeconds(10);
	}

}