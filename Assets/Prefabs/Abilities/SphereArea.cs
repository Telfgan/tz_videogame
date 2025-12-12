using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereArea : MonoBehaviour
{
	[SerializeField] private Collider _collider;
	[SerializeField] private MeshRenderer _meshRenderer;

	private bool _isConcernsEnemy;
	private Damage _damage;

	public bool IsConcernsEnemy { get => _isConcernsEnemy; set => _isConcernsEnemy = value; }

	public void SetSize(float size, Damage damage)
	{
		transform.localScale = new Vector3(size, size, size);
		_damage = damage;

		if (_collider is SphereCollider sphereCollider)
		{
			sphereCollider.radius = size / 2f;
		}
	}

	public void SetColor(Color color)
	{
		_meshRenderer.material.color = color;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (_collider.bounds.size != Vector3.zero && other.transform != transform.parent)
		{
			if (other.TryGetComponent(out UIPlayerComponents enemy))
			{
				_isConcernsEnemy = true;
				enemy.ChangeSelection(true);
			}

			if (other.TryGetComponent(out Health hpEnemy))
			{
				hpEnemy.ShowPhantomValue(_damage);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (_collider.bounds.size != Vector3.zero && other.transform != transform.parent)
		{
			if (other.TryGetComponent(out UIPlayerComponents enemy))
			{
				_isConcernsEnemy = false;
				enemy.ChangeSelection(false);
			}

			if (other.TryGetComponent(out Health hpEnemy))
			{
				Damage zeroDamage = _damage;
				zeroDamage.Value = 0;
				hpEnemy.ShowPhantomValue(zeroDamage);
			}
		}
	}
}
