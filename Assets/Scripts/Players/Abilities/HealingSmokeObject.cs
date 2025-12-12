using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingSmokeObject : MonoBehaviour
{
	public Character dad;

	[SerializeField] private float _timeToDestroy = 3;
	private float _timer = 0;
	private float _timeBetweenHeal = 1;
	private float _healTimer = 1;
	private List<Character> _allys = new List<Character>();

	private void OnTriggerEnter2D(Collider2D collision)
	{
		//if ally
		if(collision.TryGetComponent<Character>(out var ally))
		{
			//if its ally
			_allys.Add(ally);
		}
	}
	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.TryGetComponent<Character>(out var ally))
		{
			//if its ally
			_allys.Remove(ally);
		}
	}

	private void Update()
	{
		_timer += Time.deltaTime;
		_healTimer += Time.deltaTime;
		if(_timer >= _timeToDestroy) 
		{
			Destroy(gameObject);
		}
		if (_allys.Count > 0 && _healTimer >= _timeBetweenHeal)
		{
			_healTimer = 0;
			foreach (var ally in _allys)
			{
				ally.Health.Add(3 * Time.deltaTime);
			}
		}
	}
}
