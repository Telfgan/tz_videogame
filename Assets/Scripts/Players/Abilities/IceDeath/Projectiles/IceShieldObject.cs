using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class IceShieldObject : Shield
{
	[SerializeField] private GameObject _rotatePoint;
	private bool _isActive = false;
	private Vector2 _mousePos;
	private float _angle;

	private void Update()
	{
		if(_isActive)
		{
			_mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector2 lookDir = _mousePos - (Vector2)_rotatePoint.transform.position;
			_angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
			_rotatePoint.transform.rotation = Quaternion.Euler(0, 0, _angle);
		}
	}

	public void SetActive(bool value)
	{
		_isActive=value;
	}
}
