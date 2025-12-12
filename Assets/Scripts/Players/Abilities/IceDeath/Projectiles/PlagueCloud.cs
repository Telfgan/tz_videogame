using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlagueCloud : Projectiles
{
	private float _timer = 30f;
	private void Update()
	{
		Timer();		
	}

	[Server]
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (_dad == null) return;
		if (collision.gameObject == _dad.gameObject || collision.CompareTag("Ability"))
			return;
		//damage, freez etc
		if (collision.TryGetComponent<Character>(out var target))
		{			
			target.CharacterState.AddState(States.Plague, 4, 0, _dad.gameObject, _skill.name);			
		}
		//Explode();
	}

	private void Explode()
	{
		if (_hitEffect != null)
		{
			GameObject hitEffect = Instantiate(_hitEffect, transform.position, Quaternion.identity);
			Destroy(hitEffect, 5f);
		}
		Destroy(gameObject);
	}
	private void Timer()
	{
		_timer -= Time.deltaTime;
		if( _timer < 0 ) 
		{
			//Explode();
		}
	}
}
