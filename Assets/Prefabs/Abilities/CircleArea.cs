using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class CircleArea : MonoBehaviour
{
    [SerializeField] SphereCollider _colider;
	//[SerializeField] private Collider _collider3d;
	[SerializeField] SpriteRenderer _sprite;
	[SerializeField] private DecalProjector _projector;

	private List<Health> _enemiesHP = new List<Health>();
	private List<UIPlayerComponents> _enemiesUIPlayerComponents = new List<UIPlayerComponents>();
	private bool _isConcernsEnemy;
    private Damage _damage;
    /*private Damage _zeroDamage;

	private void Start()
	{
		_zeroDamage = new Damage
		{
			Value = 0,
			Type = DamageType.Physical,
			Range = AttackRangeType.RangeAttack,
		};
	}*/

	public bool IsConcernsEnemy { get => _isConcernsEnemy; set => _isConcernsEnemy = value; }

    public void SetSize(float size, Damage damage)
    {
        /*_sprite.size = new Vector2(size, size);
        _colider.radius = size / 2f;*/
		_damage = damage;

		gameObject.transform.localScale = new Vector3(size, size, 0);
		_projector.size = new Vector3(size, size, 4);
		//_projector.pivot = new Vector3(0, size / 2, 0.01f);
	}

    public void SetColor(Color color)
    {
        _sprite.color = color;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (_sprite.size != Vector2.zero && collision.transform != transform.root && collision.transform.TryGetComponent(out UIPlayerComponents enemy))
        {
            _isConcernsEnemy = true;
            enemy.CircleSelect1.SwitchClostestTarget(true);
            _enemiesUIPlayerComponents.Add(enemy);
        }
        if(collision.TryGetComponent<Health>(out var hpEnemy) && collision.transform != transform.root)
        {
            hpEnemy.ShowPhantomValue(_damage);
			_enemiesHP.Add(hpEnemy);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (_sprite.size != Vector2.zero && collision.transform != transform.root && collision.transform.TryGetComponent(out UIPlayerComponents enemy))
        {
            _isConcernsEnemy = false;
            enemy.CircleSelect1.SwitchClostestTarget(false);
            _enemiesUIPlayerComponents.Remove(enemy);
        }
		if (collision.TryGetComponent<Health>(out var hpEnemy) && collision.transform != transform.root)
		{
			Damage damage = _damage;
			damage.Value = 0;
			hpEnemy.ShowPhantomValue(damage);
			_enemiesHP.Remove(hpEnemy);
		}
	}

	private void OnDestroy()
	{
		foreach (var item in _enemiesUIPlayerComponents)
		{
			item.CircleSelect1.SwitchClostestTarget(false);
        }

		if (_enemiesHP.Count > 0)
			for (int i = _enemiesHP.Count - 1; i >= 0; i--)
			{
				Damage damage = _damage;
				damage.Value = 0;
				_enemiesHP[i].ShowPhantomValue(damage);
				_enemiesHP.Remove(_enemiesHP[i]);
			}
	}

	private void OnDisable()
	{
		if (_enemiesHP.Count > 0)
			for (int i = _enemiesHP.Count - 1; i >= 0; i--)
			{
				Damage damage = _damage;
				damage.Value = 0;
				_enemiesHP[i].ShowPhantomValue(damage);
				_enemiesHP.Remove(_enemiesHP[i]);
			}
	}
}