using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BoxArea : MonoBehaviour
{
    [SerializeField] BoxCollider _colider;
    [SerializeField] SpriteRenderer _sprite;
    [SerializeField] private DecalProjector  _projector;

    private List<Health> _enemies = new List<Health>();
    private List<Character> _targets = new List<Character>();
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

	public void SetSize(float width, float length, Damage damage)
    {
        /*_sprite.size = new Vector2(width, length);
        
        */
        //gameObject.transform.localScale = new Vector3(width, length, 1);
        _colider.center = new Vector3(0, length / 2, 0);
        _colider.size = new Vector3(width, length, 5);
        _projector.size = new Vector3(width, length, 5);
        _projector.pivot = new Vector3(0, length/2, 0.01f);
        _damage = damage;
    }

    public void SetColor(Color color)
    {
        _sprite.color = color;
    }

    private void OnTriggerEnter(Collider collision)
    {
       
        if(collision.transform != transform.parent && collision.transform.TryGetComponent(out Character enemy))
        {
            // deistvie s enemy
            _targets.Add(enemy);
			enemy.SelectedCircle.SwitchClostestTarget(true);
        }
        if (collision.TryGetComponent<Health>(out var hpEnemy) && collision.transform != transform.root)
		{
			//Debug.Log("ENTER " + collision.transform + "  / " + transform.root);
            _enemies.Add(hpEnemy);
			hpEnemy.ShowPhantomValue(_damage);
		}
	}

    private void OnTriggerExit(Collider collision)
    {
        if (collision.transform != transform.parent && collision.transform.TryGetComponent(out Character enemy))
        {
            // bezdeistvie s enemy
			_targets.Remove(enemy);
            enemy.SelectedCircle.SwitchClostestTarget(false);
        }
		if (collision.TryGetComponent<Health>(out var hpEnemy) && collision.transform != transform.root)
		{
			Damage damage = _damage;
            damage.Value = 0;
			//Debug.Log("Exit " + collision.name + "  / " + damage.Value);
			hpEnemy.ShowPhantomValue(damage);
			_enemies.Remove(hpEnemy);
		}
	}

	private void OnDestroy()
	{
        OnExit();
    }

	private void OnDisable()
	{
        OnExit();
    }

	private void OnExit()
	{
        if (_enemies.Count > 0)
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                Damage damage = _damage;
                damage.Value = 0;
                _enemies[i].ShowPhantomValue(damage);
                _enemies.Remove(_enemies[i]);
            }

        foreach (var enemy in _targets)
        {
            enemy.SelectedCircle.SwitchClostestTarget(false);
        }
    }
}
