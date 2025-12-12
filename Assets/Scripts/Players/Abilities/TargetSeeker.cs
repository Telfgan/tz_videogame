using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetSeeker : MonoBehaviour
{
	[SerializeField] protected LayerMask _targetsLayers;
	[SerializeField] private Character _hero;

	private SkillType _skillType;
	private float _radius = 0;
	private Skill _skill;

	public LayerMask TargetsLayers { get => _targetsLayers; protected set => _targetsLayers = value; }

	/*
	public TargetToShot GetTarget(TypeClick click, Action<Vector3> ClickPoint, SkillType skillType, float radius, Skill skill, bool isCanTargetHimself = false, bool canTargetDead = false)
	{
		_skillType = skillType;
		_radius = radius;
		_skill = skill;
		TargetToShot target = new TargetToShot();

		switch (click)
		{
			case TypeClick.LMB:

				target = LeftClick();
				ClickPoint?.Invoke(target.Position);
				break;

			case TypeClick.ShiftLMB:

				target = ShiftLeftClick();
				ClickPoint?.Invoke(target.Position);
				break;

			case TypeClick.CtrlLMB:

				target = CtrlLeftClick();
				ClickPoint?.Invoke(target.Position);
				break;

			case TypeClick.SpaceLMB:

				target = SpaceLeftClick();
				ClickPoint?.Invoke(target.Position);
				break;
		}

		if(target != null)
		{
			if (!target.isCharater) return target;

			if (target.character.IsDead && !canTargetDead) return null;

			return target;
		}

		return null;
	}*/

	public ITargetable ClosedTarget(bool isCanTargetHimself = false)
	{
		var closerTargets = GetCloserTargets(transform.position, 1000, isCanTargetHimself);

		if (closerTargets != null && closerTargets.Count > 0)
		{
			for (int i = closerTargets.Count - 1; i >= 0; i--)
			{
				/*if (!closerTargets[i].IsTargetable)
				{
					closerTargets.Remove(closerTargets[i]);
				}*/
			}
			if(closerTargets[0] != null)
				return closerTargets[0];
		}
		return null;
	}

	public List<ITargetable> GetCloserTargets(Vector3 position, float radius, bool isCanTargetHimself = false)
	{
		List<ITargetable> targets = new List<ITargetable>();
		Collider[] collider = Physics.OverlapSphere(position, radius, TargetsLayers);

		foreach (var item in collider)
		{
			if (collider.Length > 0 && item.transform.TryGetComponent<ITargetable>(out ITargetable enemy))
			{
				if (isCanTargetHimself == false && enemy.Transform == _hero.transform)
				{
					continue;
				}
				targets.Add(enemy);
			}
		}
		targets = targets.OrderBy(character => Vector3.Distance(character.Transform.position, gameObject.transform.position)).ToList();

		if (targets.Count <= 0)
			return null;

		/*for(int i = targets.Count - 1; i >= 0; i--)
		{
			if (!targets[i].IsTargetable)
			{
				targets.Remove(targets[i]);
			}
		}*/

		return targets;
	}

	public Character ClosedTargetCharacter(bool isCanTargetHimself = false)
	{
		var closerTargets = GetCloserTargetsCharacter(transform.position, 1000, isCanTargetHimself);

		if (closerTargets != null && closerTargets.Count > 0)
		{
			/*for (int i = closerTargets.Count - 1; i >= 0; i--)
			{
				if (closerTargets[i].IsDead)
				{
					closerTargets.Remove(closerTargets[i]);
				}
			}*/
			if (closerTargets[0] != null)
				return closerTargets[0];
		}

		return null;
	}

	public List<Character> GetCloserTargetsCharacter(Vector3 position, float radius, bool isCanTargetHimself = false)
	{
		List<Character> targets = new List<Character>();
		Collider[] collider = Physics.OverlapSphere(position, radius, TargetsLayers);

		foreach (var item in collider)
		{
			if (collider.Length > 0 && item.transform.TryGetComponent<Character>(out Character enemy))
			{
				if (isCanTargetHimself == false && enemy.Transform == _hero.transform)
				{
					continue;
				}
				targets.Add(enemy);
			}
		}
		targets = targets.OrderBy(character => Vector3.Distance(character.Transform.position, gameObject.transform.position)).ToList();

		if (targets.Count <= 0)
			return null;

		/*for (int i = targets.Count - 1; i >= 0; i--)
		{
			if (targets[i].IsDead)
			{
				targets.Remove(targets[i]);
			}
		}*/
		return targets;
	}

	public IDamageable GetRaycastTarget(Skill skill, bool isCanTargetHimself = false)
	{
		_skill = skill;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit[] rayHit = Physics.RaycastAll(ray, 100f, TargetsLayers);

		foreach (var hit in rayHit)
		{
			if (_skill.AutoAttack == AutoAttack.autoAttack)
			{
				if (UnityEngine.InputSystem.Keyboard.current.leftCtrlKey.isPressed)
				{
					if (hit.collider.TryGetComponent<IDamageable>(out _))
					{
						_skill.IsAutoMode = true;						
					}
				}
			}
		}

		foreach (var item in rayHit)
		{
			if (item.collider.TryGetComponent<IDamageable>(out var damageable))
			{
				if(item.collider.TryGetComponent<Character> (out var character))
				{
					if (character.IsDead)
						return null;
				}
				return damageable;
			}
		}

		return null;
	}

	public Character GetClosestTargets()
	{
		Collider2D[] enemyDetected = Physics2D.OverlapCircleAll(transform.position, 100);
		Vector2 closest = Vector2.positiveInfinity;
		Character enemys = null;
		foreach (Collider2D collider in enemyDetected)
		{
			if (collider.gameObject != _hero.gameObject)

				if (collider.TryGetComponent<Character>(out var enemy))
				{
					if (Vector2.Distance(collider.transform.position, transform.position) < Vector2.Distance(closest, transform.position))
					{
						enemys = enemy;
						closest = collider.transform.position;
						Debug.Log(enemy);
					}
				}
		}
		if (Vector2.Distance(closest, transform.position) < 100) return enemys;
		else return null;
	}

	/*
	private TargetToShot LeftClick()
	{
		TargetToShot target = new TargetToShot();
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		switch (_skillType)
		{
			case SkillType.Target:
				Debug.Log("SkillType Target");
				target.character = ClosedTarget();
				target.isCharater = true;
				break;
			case SkillType.Projectile:
				Debug.Log("SkillType Projectile");

				if (Physics.Raycast(ray, out hit))
				{
					if (hit.collider.TryGetComponent<Character>(out Character character))
					{
						if (character != null)
						{
							target.character = character;
							target.isCharater = true;
						}
					}
					else
					{
						var distance = Vector3.Distance(hit.point, transform.position);
						if (distance <= _radius || distance > _radius) target.Position = hit.point;
						target.isCharater = false;
					}
				}
				break;
			case SkillType.Zone:
				Debug.Log("SkillType Zone");

				if (Physics.Raycast(ray, out hit))
				{
					Debug.Log(hit);
				}
				if (Vector3.Distance(hit.point, transform.position) <= _radius)
					target.Position = hit.point;
				target.isCharater = false;
				break;
			case SkillType.NonTarget:
				Debug.Log("SkillType NonTarget");
				break;
			default:
				if (Physics.Raycast(ray, out hit))
				{
					Debug.Log(hit.point);
				}
				target.Position = hit.point;
				target.isCharater = false;
				break;
		}
		return target;
	}

	private TargetToShot ShiftLeftClick()
	{
		TargetToShot target = new TargetToShot();

		target.Position = transform.position;
		target.character = _hero;
		target.isCharater = true;

		return target;
	}

	private TargetToShot CtrlLeftClick()
	{
		TargetToShot target = new TargetToShot();
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (_skill.AutoAttack == AutoAttack.autoAttack)
		{
			_skill.IsAutoMode = true;
		}

		switch (_skillType)
		{
			case SkillType.Target:
				target.character = ClosedTarget();
				target.isCharater = true;
				break;
			case SkillType.Projectile:
				if (Physics.Raycast(ray, out hit))
				{
					Debug.Log(hit.point);
				}
				target.Position = hit.point;
				target.isCharater = false;
				break;
			case SkillType.Zone:
				if (Physics.Raycast(ray, out hit))
				{
					Debug.Log(hit.point);
				}
				target.Position = hit.point;
				target.isCharater = false;
				break;
			default:
				if (Physics.Raycast(ray, out hit))
				{
					Debug.Log(hit.point);
				}
				target.Position = hit.point;
				target.isCharater = false;
				break;
		}
		return target;
	}

	private TargetToShot SpaceLeftClick()
	{
		TargetToShot target = new TargetToShot();
		var closerTargets = GetCloserTargets(transform.position, 1000);
		Character closerTarget = null;

		if (closerTargets != null && closerTargets.Count > 0)
		{
			closerTarget = GetCloserTargets(transform.position, 1000)[0];
		}
		target.character = closerTarget;
		target.isCharater = true;
		return target;
	}
	
	 public Vector3 GetMousePoint()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			if (autoAttack == AutoAttack.autoAttack)
			{
				if (UnityEngine.InputSystem.Keyboard.current.leftCtrlKey.isPressed)
				{
					if (hit.collider.TryGetComponent<IDamageable>(out _))
					{

						IsAutoMode = true;
						AutoModeChanged?.Invoke(true);
					}
				}
			}

			return hit.point;
		}
		return Vector3.zero;
	}
	 */
}

public enum TypeClick
{
	None,
	LMB,
	ShiftLMB,
	SpaceLMB,
	CtrlLMB
}
