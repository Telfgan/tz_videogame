using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
using System;

public class IceRolling : Skill
{
	[Header("Ability properties")]

	[SerializeField] private Character _playerLinks;
	[SerializeField] private PhysicalAttack _physicalAttack;
	[SerializeField] private SeriesOfStrikes _seriesOfStrikes;
	[SerializeField] private float _jumprange = 5f;
	[SerializeField] private float _durationOfJumpPerCell = 0.3f;
	[SerializeField] private AudioClip _audioClip;
	//[SerializeField] private LayerMask _groundLayer;

	private static readonly int iceRollingStart = Animator.StringToHash("IceRollingStart");
	private static readonly int iceRollingEnd = Animator.StringToHash("IceRollingEnd");

	private AudioSource _audioSource;
	private Vector3 _mousePos = Vector2.positiveInfinity;
	//private Vector3 _jumpPos;
	private Vector3 _lookDir;
	private Energy _energy;

	private bool _rollingPhysTalent = false;
	private bool _rollingWithEnemyTalent = false;

	private bool _isLastInSeries = false;
	private bool _isJump = false;

	private float _durationOfJump;
	private float _jumpCount = 0;
	private bool _afterJump;
	private float _afterJumpDelay = 1;
	//private Character _target;
	private Character _attachedTarget;
	private Animator _animator;

	protected override bool IsCanCast
	{
		get
		{
			if (GetTargetCharacter() != null) return Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius;
			else return true;
		}
	}

	protected override int AnimTriggerCastDelay => 0;
	protected override int AnimTriggerCast => iceRollingStart;

	public void IceRollingCast() => AnimStartCastCoroutine();
	public void IceRollingEnd() => AnimCastEnded();

	private void Start()
	{
		_animator = GetComponent<Animator>();
		_audioSource = GetComponent<AudioSource>();

		for (int i = 0; i < _playerLinks.Resources.Count; i++)
		{
			if (_playerLinks.Resources[i].Type == ResourceType.Energy)
				_energy = (Energy)_playerLinks.Resources[i];
		}
	}

	private void Update()
	{
		if (_afterJump)
		{
			TimerDelay();
		}
	}

	private float GetJumpRange()
	{
		float range = _jumprange;
		float energyCost = 1;
		for (int i = 0; i < 10; i++)
		{
			if (_energy.CurrentValue >= energyCost)
			{
				range += 0.2F;
				energyCost += 1;
			}
		}

		return range;
	}

	private bool CheckObstacleBetween(Vector3 start, Vector3 end, out Vector3 stopPosition, out Character characterHit)
	{
		Vector3 direction = (end - start).normalized;
		float distance = Vector3.Distance(start, end);

		RaycastHit[] hits = Physics.BoxCastAll(start, new Vector3(0.05f, 0.05f, 0.05f), direction, Quaternion.identity, distance, _obstacle);

		stopPosition = end;
		characterHit = null;

		foreach (RaycastHit hit in hits)
		{
			if (hit.collider.TryGetComponent(out Character character) && character != _playerLinks)
			{
				if (!_rollingWithEnemyTalent && character != GetTargetCharacter())
				{
					stopPosition = hit.point - direction;
					characterHit = character;
					return true;
				}
			}
			/*int objectLayer = hit.collider.gameObject.layer;
			int objectLayerMask = 1 << objectLayer;
			if ((_obstacle & objectLayerMask) > 0 )
			{
				stopPosition = hits[0].point - direction * 1.2f;
				return true;
			}*/
			//stopPosition = hits[0].point - direction * 1.2f;
			if (hit.collider.transform.root != transform.root)
			{
				Debug.Log(hit.collider.gameObject.name + " Stop point " + stopPosition);
				stopPosition = hit.point - direction * 1.2f;
				return true;
			}
		}

		return false;
	}

	private bool IsTargetInCloseProximity(Vector3 start, Vector3 direction, out Character characterHit)
	{
		Ray ray = new Ray(start, direction);
		RaycastHit[] hits = Physics.RaycastAll(ray, 1);

		characterHit = null;

		foreach (RaycastHit hit in hits)
		{
			if (hit.collider.TryGetComponent(out Character character) && character == characterHit && character != _playerLinks)
			{
				characterHit = character;
				return true;
			}
		}

		return false;
	}

	private void Jump2()
	{
		Hero.Move.CanMove = false;
		_isJump = true;

		_lookDir = (_mousePos - _playerLinks.transform.position).normalized;
		float baseRange = _rollingWithEnemyTalent ? 4f : 2f;
		float maxEnergy = Mathf.Min(_energy.CurrentValue, 10f);
		int energyBlocks = Mathf.FloorToInt(maxEnergy / 5f);
		float bonusRange = _rollingWithEnemyTalent ? energyBlocks * 2f : energyBlocks * 1f;

		Vector3 startPosition = _playerLinks.transform.position;
		Vector3 rawTargetPos = _mousePos;

		float distanceToClick = Vector3.Distance(startPosition, rawTargetPos);
		float finalRange = Mathf.Min(distanceToClick, GetJumpRange());

		float energyMy = finalRange / 5;

		if (_isLastInSeries && GetTargetCharacter() == null && _rollingWithEnemyTalent) finalRange *= 1.5f;

		Vector3 jumpPos = startPosition + _lookDir * finalRange;

		//float energyUsed = energyBlocks * 5f;
		//if (energyUsed > 0) _energy.CmdUse(energyUsed);
		_energy.CmdUse(energyMy);

		Vector3 stopPosition;
		Character characterHit;
		Character characterHitTarget;

		bool hit = CheckObstacleBetween(_playerLinks.transform.position, jumpPos, out stopPosition, out characterHit);
		bool hitTarget = IsTargetInCloseProximity(_playerLinks.transform.position, _lookDir, out characterHitTarget);

		Hero.Move.LookAtPosition(jumpPos);
		float actualDistance = Vector3.Distance(startPosition, stopPosition);

		//if (hit && characterHit != null) CmdPush(stopPosition, actualDistance);
		//else
		CmdPush(stopPosition, actualDistance);

		if (_rollingWithEnemyTalent && GetTargetCharacter() != null && hitTarget && characterHitTarget != null) CmdPushWithCharacter(stopPosition, characterHitTarget, actualDistance);

		if (_rollingPhysTalent)
		{
			_physicalAttack.TalentRollingPhys(_afterJump, finalRange);
			_afterJump = true;
		}


		if (!_hero.Abilities.SkillQueue.Skills.Contains(this))
		{
			ClearTarget();
			//_target = null;
			_mousePos = Vector3.positiveInfinity;
			_lookDir = Vector3.zero;
			//_jumpPos = Vector3.zero;
		}
		else
		{
			FindTargetCharacter();
			//_target = GetTarget().character;
			_mousePos = GetTargetCharacter() != null ? GetTargetCharacter().transform.position : GetMousePoint();
		}
		
	}

	private void Jump()
	{
		Hero.Move.CanMove = false;
		_isJump = true;
		float actualJumpRange = _jumprange;

		_lookDir = (_mousePos - _playerLinks.transform.position).normalized;
		Vector3 jumpPos = _lookDir * actualJumpRange + _playerLinks.transform.position;
		Vector3 stopPosition;
		Character characterHit;
		if (CheckObstacleBetween(_playerLinks.transform.position, jumpPos, out stopPosition, out characterHit))
		{
			_jumpCount = 5;
			CmdPush(stopPosition, Vector3.Distance(stopPosition, transform.position));
		}
		else
		{
			for (int i = 0; i < 10; i++)
			{
				_jumpCount += 1f;
				actualJumpRange += .2f;
				Vector3 jumpPos2 = _lookDir * actualJumpRange + _playerLinks.transform.position;
				if (_energy.CurrentValue >= 5 && !CheckObstacleBetween(_playerLinks.transform.position, jumpPos2, out stopPosition, out characterHit))
				{
					_energy.CmdUse(1);
					jumpPos = jumpPos2;
					//Debug.Log("Additional jump " + i);
				}
			}
			CmdPush(stopPosition, Vector3.Distance(jumpPos, transform.position));

			if (_rollingPhysTalent)
			{
				_physicalAttack.TalentRollingPhys(_afterJump, _jumpCount);
				_afterJump = true;
			}
		}

		if (!_hero.Abilities.SkillQueue.Skills.Contains(this))
		{
			ClearTarget();
			//_target = null;
			_mousePos = Vector3.positiveInfinity;
			_lookDir = Vector3.zero;
			//_jumpPos = Vector3.zero;
		}
		else
		{
			/*if(GetTarget().isCharater)
				_target = GetTarget().character;
			else*/
			//_target = null;
			_mousePos = GetTargetCharacter() != null ? GetTargetCharacter().transform.position : GetMousePoint();
		}
	}

	public override void LoadTargetData(TargetInfo targetInfo)
	{
		if (targetInfo != null && targetInfo.GetTargets() != null && targetInfo.GetTargets().Count > 0) if (targetInfo.GetTargets()[0] is Character character) 
				SetTarget(character);
		else SetTarget(ClosedTarget());
	}

	protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
	{
		while (float.IsPositiveInfinity(_mousePos.x))
		{
			if (GetMouseButton)
			{
				FindTargetCharacter();
				//_target = GetTarget().character;
				_mousePos = GetTargetCharacter() != null ? GetTargetCharacter().transform.position : GetMousePoint();
			}
			yield return null;
		}
    }

	protected override IEnumerator DynamicRendererJob(float time = 0.2f)
	{
		while (true)
		{
			yield return new WaitForSeconds(time);
			_skillRender.SetSizeBox(1, GetJumpRange());
		}
	}

	protected override IEnumerator CastJob()
	{
		if (!float.IsInfinity(_mousePos.x))
        {
			_isLastInSeries = _seriesOfStrikes.MakeHit(GetTargetCharacter(), AbilityForm.Physical, 1, 0, 0);
			Jump();
			yield return null;
		}
	}

	protected override void ClearData()
	{
		if (!_hero.Abilities.SkillQueue.Skills.Contains(this))
		{
			ClearTarget();
			//_target = null;
			_mousePos = Vector3.positiveInfinity;
		}
		else
		{
			_mousePos = GetMousePoint();
		}
		_isJump = false;
		Hero.Move.StopLookAt();
	}

	private void HandleJumpEnd()
	{
		if (_animator != null)
		{
			_animator.ResetTrigger(iceRollingStart);
			_animator.SetTrigger(iceRollingEnd);
		}
	}

	#region Talent
	public void TalentRollingPhys(bool value) => _rollingPhysTalent = value;
	public void RollingWithEnemyTalentActive(bool value) => _rollingWithEnemyTalent = value;
	#endregion

	private void TimerDelay()
	{
		_afterJumpDelay -= Time.deltaTime;
		if (_afterJumpDelay < 0)
		{
			_afterJumpDelay = 1;
			_afterJump = false;
			_physicalAttack.TalentRollingPhys(_afterJump, 0);
		}
	}

	private IEnumerator WaitForJumpEnd()
	{
		yield return new WaitForSeconds(_durationOfJump);
		RpcOnJumpEnd();

		if (_attachedTarget)
        {
			_attachedTarget.CharacterState.AddState(States.Frozen, 2f, 0f, _playerLinks.gameObject, Name);
			_attachedTarget.transform.SetParent(null);
			RpcReleaseTarget(_attachedTarget);
			_attachedTarget = null;
		}
	}

	private void OnCollisionEnter(Collision collision)
    {
		if (_rollingWithEnemyTalent && _isJump && collision.collider.TryGetComponent(out Character character) && character != _playerLinks && ((1 << character.gameObject.layer) & _targetsLayers) != 0)
		{
			Vector3 pushDir = (character.transform.position - _playerLinks.transform.position).normalized;
			pushDir.y = 0;
			Vector3 pushTarget = character.transform.position + pushDir * 2f;
			CmdKnockback(character, pushTarget);
		}
	}

	[Command]
	private void CmdPush(Vector3 force, float finalRange)
	{
		RpcPlayShotSound();
		_durationOfJump = finalRange * _durationOfJumpPerCell;
		force.y = 1;
		_playerLinks.Move.TargetRpcDoMove(force, _durationOfJump);
		StartCoroutine(WaitForJumpEnd());
	}

	[Command]
	private void CmdPushWithCharacter(Vector3 force, Character target, float finalRange)
	{
		RpcPlayShotSound();
		_durationOfJump = finalRange * _durationOfJumpPerCell;

		if (target.TryGetComponent(out MoveComponent move))
		{
			move.CanMove = false;
			_attachedTarget = target;
			_attachedTarget.transform.SetParent(_playerLinks.transform);
			RpcAttachTarget(_attachedTarget);
		}

		force.y = 1;
		_playerLinks.Move.TargetRpcDoMove(force, _durationOfJump);

		StartCoroutine(WaitForJumpEnd());
	}

	[Command]
	private void CmdKnockback(Character target, Vector3 force)
	{
		if (target.TryGetComponent(out MoveComponent move))
		{
			
			if (target.connectionToClient != null) move.TargetRpcDoMove(force, 0.05f);
			else move.RpcDoMove(force, 0.05f);
		}
	}

	[ClientRpc]
	private void RpcReleaseTarget(Character target)
	{
		target.Move.CanMove = true;
		target.transform.SetParent(null);

		if (target.TryGetComponent(out NavMeshAgent agent))
		{
			agent.enabled = true;
			agent.Warp(GetTargetCharacter().transform.position);
		}

		if (target.TryGetComponent(out Rigidbody rigidbody)) rigidbody.isKinematic = false;
	}

	[ClientRpc] private void RpcPlayShotSound() => _audioSource?.PlayOneShot(_audioClip);
	[ClientRpc] private void RpcOnJumpEnd() => HandleJumpEnd();

	[ClientRpc]
	private void RpcAttachTarget(Character target)
	{
		if (target.TryGetComponent(out MoveComponent move)) move.CanMove = false;
		if (target.TryGetComponent(out NavMeshAgent agent)) agent.enabled = false;

		if (target.TryGetComponent(out Rigidbody rb))
		{
			rb.isKinematic = true;
			rb.linearVelocity = Vector3.zero;
		}
		target.transform.SetParent(_playerLinks.transform);
	}
}