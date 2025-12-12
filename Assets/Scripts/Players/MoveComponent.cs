using DG.Tweening;
using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class MoveComponent : NetworkBehaviour
{
    [SerializeField, Range(0, 0.5f)] private float _smoothTime = 0.15f;
	[SerializeField] protected float _currentSpeed = 5;
    [SerializeField] protected float _rotationDefaultSpeed = 1000;
    [SerializeField] protected Animator _anim;
	[SerializeField] protected FlyChecker _flyChecker;
	[SerializeField] private AudioSource moveAudioSource;
	[SerializeField] private AudioClip[] moveClips;

	protected float _animMultiplier;

	public Vector3 MoveDirection = Vector3.zero;
	public Vector3 ExternalMoveDirection = Vector3.zero;

	public bool CanMove = false;
	public bool CanMoveState = false;
	public bool IsMoving = false;
	public bool IsSelect = false;

	private Rigidbody _rigidbody;
	private Vector3 _offset = Vector3.zero;

	private bool _isHero = false;
	private bool _isMoveBlocked = false;

	private float _defaultSpeed = 5;
	private Camera _camera;

	private bool _isLookAtCursor = true;
	private Vector3 _dir;
	private Vector3 _currentVelocity;
	private Vector3 _currentVelocityTemp;
	private Coroutine _lookAtTransformJob;

	private bool _isFly;

	public bool IsFly => _isFly;
	public bool IsLookAtCursor { get => _isLookAtCursor; set => _isLookAtCursor = value; }
	public float DefaultSpeed => _defaultSpeed;
	public float CurrentSpeed => _currentSpeed;

	public Rigidbody Rigidbody => _rigidbody;

	public bool IsMoveBlocked { get => _isMoveBlocked; set => _isMoveBlocked = value; }
    public float CurrentRotationSpeed { get => _rotationDefaultSpeed + RotateModifier; }
    public float RotateModifier { get; set; }

    protected override void OnValidate()
    {
        base.OnValidate();

		if(_flyChecker == null)
			_flyChecker = GetComponentInChildren<FlyChecker>();
	}

    public void SetOffset(Vector2 offset)
	{
		_offset = offset;
	}

	public void Initialize(float speed, Rigidbody rb , bool isHero = false)
	{
		_defaultSpeed = speed;

		_rigidbody = rb;
		
		SetDefaultSpeed();

		MoveDirection = Vector2.zero;

		CanMove = true;
		_isHero = isHero;

		InputHandler.OnPlayerMove += OnMove;
		_flyChecker.OffedGround += OnOffedGround;
		_flyChecker.ReachGround += OnReachGround;
	}

    private void OnDestroy()
    {
		if (InputHandler.OnPlayerMove != null) InputHandler.OnPlayerMove -= OnMove;

		if (_flyChecker != null)
		{
			_flyChecker.OffedGround -= OnOffedGround;
			_flyChecker.ReachGround -= OnReachGround;
		}
	}

	public void LookAtPosition(Vector3 position)
	{
		_isLookAtCursor = false;

		if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z) ||
	  position == Vector3.positiveInfinity || position == Vector3.negativeInfinity ||
	  Vector3.Distance(transform.position, position) < Mathf.Epsilon) return;

		Vector3 direction = position - transform.position;

		if (direction.sqrMagnitude < Mathf.Epsilon) return;

		var transformRotate = transform.eulerAngles;
		transform.LookAt(position);
		transform.eulerAngles = new Vector3(transformRotate.x, transform.eulerAngles.y, transformRotate.z);
	}

	public void LookAtTransform(Transform transform)
    {
		_isLookAtCursor = false;

		if (_lookAtTransformJob != null)
			StopCoroutine(_lookAtTransformJob);

		_lookAtTransformJob = StartCoroutine(LookAtTransformCoroutine(transform));
    }

	public void StopLookAt()
    {
		if(_lookAtTransformJob != null)
			StopCoroutine(_lookAtTransformJob);

		_isLookAtCursor = true;
	}

	public void StopMoveAndAnimationMove()
	{
		if (_rigidbody == null) return;

		_rigidbody.linearVelocity = Vector3.zero;
		_rigidbody.angularVelocity = Vector3.zero;

		var agent = GetComponent<NavMeshAgent>();
		if (agent && agent.enabled) agent.ResetPath();

		if (_anim != null)
		{
			_anim.SetFloat(HashAnimPlayer.VelocityX, 0);
			_anim.SetFloat(HashAnimPlayer.VelocityZ, 0);
		}
	}

	public void ChangeMoveSpeed(float value)
	{
		_currentSpeed *= value;
	}
	public void SetMoveSpeed(float speed)
	{
		_currentSpeed = speed;
	}
	public void SetDefaultSpeed()
	{
		_currentSpeed = _defaultSpeed;
	}

	public void DoMove(Vector3 vector3, float duration)
	{
		CanMove = false;
		_rigidbody.DOMove(vector3, duration).OnComplete(() =>
		{
			CanMove = true;
		});
	}

	private void OnReachGround()
	{
		_isFly = false;
	}

	private void OnOffedGround()
	{
		_isFly = true;
	}

	[ClientCallback]
	void Update()
	{
		if (isOwned == false)
			return;

		if (_camera == null)
			_camera = Camera.main;

		Move();
		RotateAtCursor();
	}

	protected virtual void Move()
    {
		if ((!CanMove && !CanMoveState) || _rigidbody == null || IsMoveBlocked == true)
		{

			if (_rigidbody != null)
			{
                _rigidbody.linearVelocity = Vector3.zero;
                _currentVelocityTemp = Vector3.zero;
                _currentVelocity = Vector3.zero;
            } 

			return;
		}

		if (IsSelect == false)
		{
			_dir = Vector2.zero;
		}

		if (_camera == null)
			return;

		_currentVelocity = Vector3.SmoothDamp(_currentVelocity, _dir, ref _currentVelocityTemp, _smoothTime); // Move from camera

		var camDir = _camera.transform.TransformDirection(_currentVelocity);

		camDir = Quaternion.AngleAxis(-_camera.transform.eulerAngles.x, _camera.transform.TransformVector(Vector3.right)) * camDir;

		_rigidbody.linearVelocity = new Vector3(camDir.x * _currentSpeed, _rigidbody.linearVelocity.y, camDir.z * _currentSpeed);

		if (_rigidbody.linearVelocity.magnitude > 0.5f && moveAudioSource != null && !moveAudioSource.isPlaying) PlayMove();

		if (_anim != null && CanMove)
		{
			var animDir = transform.InverseTransformPoint(transform.position + camDir);
			_animMultiplier = 0.1f * _rigidbody.linearVelocity.magnitude + 0.5f;
			_anim.SetFloat(HashAnimPlayer.VelocityZ, animDir.z * _animMultiplier);
			_anim.SetFloat(HashAnimPlayer.VelocityX, animDir.x * _animMultiplier);
		}
	}

	protected virtual void RotateAtCursor()
    {
		if (GetComponent<MinionMove>()) return;

		if (IsSelect == true && _isLookAtCursor == true)
		{
			Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit))
			{
				Vector3 targetPosition = hit.point;
				Vector3 direction = targetPosition - transform.position;
				direction.y = 0;

				if (direction.sqrMagnitude > 0.001f)
				{
					Quaternion targetRotation = Quaternion.LookRotation(direction);
					float angleToTarget = Quaternion.Angle(transform.rotation, targetRotation);

					if (angleToTarget < 0.1f)
					{
						_rigidbody.MoveRotation(targetRotation);
					}
					else
					{
                        Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, CurrentRotationSpeed * Time.deltaTime);
                        _rigidbody.MoveRotation(newRotation);
                    }
				}
			}
		}
	}

	public void SetAnimationMovement(Vector3 direction)
    {
        Vector3 localDir = transform.InverseTransformDirection(direction);

		_animMultiplier = 0.1f * _rigidbody.linearVelocity.magnitude + 0.5f;

		_anim.SetFloat(HashAnimPlayer.VelocityZ, localDir.z * _animMultiplier);
        _anim.SetFloat(HashAnimPlayer.VelocityX, localDir.x * _animMultiplier);
    }

    private void OnMove(Vector2 dir)
    {
		if (IsSelect)
			_dir = new Vector3(dir.x, 0, dir.y);
	}

	public void TeleportToPositionSmooth(Vector3 position, float duration)
	{
		if (isOwned)
		{
			CanMove = false;
			_rigidbody.DOMove(position, duration).OnComplete(() =>
			{
				CanMove = true;
			});
		}
		else if (isServer)
		{
			TargetRpcTeleportToPositionSmooth(connectionToClient, position, duration);
		}
	}

	private IEnumerator LookAtTransformCoroutine(Transform transform)
    {
		while (!_isLookAtCursor)
        {
			if (transform != null) LookAtPosition(transform.position);
			else StopLookAt();
			yield return null;
		}
    }

	private IEnumerator MoveTowardsCoroutine(Vector3 targetPosition, float speed, Action onComplete = null)
	{
		while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
		{
			Vector3 direction = (targetPosition - transform.position).normalized;
			transform.position += direction * speed * Time.deltaTime;
			yield return null;
		}

		onComplete?.Invoke();
	}

	private IEnumerator DoPushWithAgent(Vector3 targetPos, float duration)
	{
		var agent = GetComponent<NavMeshAgent>();

		if (agent != null && agent.enabled)
			agent.enabled = false;

		CanMove = false;
		_rigidbody.DOKill();

		yield return _rigidbody.DOMove(targetPos, duration)
			.SetEase(Ease.Linear)
			.WaitForCompletion();

		if (agent != null)
			agent.enabled = true;

		CanMove = true;
	}

	public void MoveTowards(Vector3 targetPosition, float speed, Action onComplete = null)
	{
		if (!isServer) return;

		TargetRpcMoveTowards(connectionToClient, targetPosition, speed);
	}

	public void PlayMove()
	{
		if (!isOwned) return;
		if (moveClips.Length == 0 || moveAudioSource == null) return;

		if (_rigidbody.linearVelocity.magnitude <= 0.1f)
		{
			if (moveAudioSource.isPlaying) moveAudioSource.Stop();
			return;
		}

		int index = UnityEngine.Random.Range(0, moveClips.Length);
		moveAudioSource.PlayOneShot(moveClips[index]);
	}

	[TargetRpc]
	public void TargetRpcAddForce(Vector3 vector3)
    {
		_rigidbody.AddForce(vector3);
	}

	[TargetRpc]
	public void TargetRpcAddTransformPosition(Vector3 vector3)
    {
		transform.position += vector3;
	}

	[TargetRpc]
	public void TargetRpcSetTransformPosition(Vector3 vector3)
    {
		transform.position = vector3;
	}

	[TargetRpc]
	public void TargetRpcDoMove(Vector3 vector3, float duration)
	{
		//Debug.Log("DoMove " + vector3, this);
		CanMove = false;
		_rigidbody.DOMove(vector3, duration).OnComplete(() =>
		{
			CanMove = true;
		});
	}

	public void TargetRpcDoMoveNavMeshAgent(Vector3 postion)
    {
		var agent = GetComponent<NavMeshAgent>();
		agent.enabled = false;

		CanMove = false;
		Rigidbody.DOMove(postion, 0.5f).OnComplete(() =>
		{
			CanMove = true;
			agent.enabled = true;
		});
	}

	[TargetRpc]
	private void TargetRpcTeleportToPositionSmooth(NetworkConnection target, Vector3 position, float duration)
	{
		CanMove = false;
		_rigidbody.DOMove(position, duration).OnComplete(() =>
		{
			CanMove = true;
		});
	}

	[TargetRpc]
	private void TargetRpcMoveTowards(NetworkConnection target, Vector3 targetPosition, float speed)
	{
		StartCoroutine(MoveTowardsCoroutine(targetPosition, speed));
	}

	[TargetRpc]
	public void TargetRpcDoPush(Vector3 targetPos, float duration)
	{
		StartCoroutine(DoPushWithAgent(targetPos, duration));
	}

	#region Test
	[Command]
	public void CmdAddTransformPosition(Vector3 vector3)
    {
		RpcAddTransformPosition(vector3);
	}


	[Command]
	public void CmdDoMove(Vector3 vector3, float duration)
	{
		RpcDoMove( vector3, duration);
	}

	[ClientRpc]
	public void RpcDoPush(Vector3 targetPos, float duration)
	{
		StartCoroutine(DoPushWithAgent(targetPos, duration));
	}

	//[ClientRpc]
	public void RpcAddTransformPosition(Vector3 vector3)
	{
		transform.position += vector3;
	}

	public void TestDoMove(Vector3 targetPosition, float maxDistance)
	{
		CanMove = false;

		Tween moveTween = null;

	     moveTween = _rigidbody.DOMove(targetPosition, 1f)
			.SetEase(Ease.Linear)
			.OnUpdate(() =>
			{
				if (Vector3.Distance(transform.position, targetPosition) <= maxDistance)
				{
					moveTween.Kill();
			}
			})
			.OnKill(() =>
			{
				CanMove = true;
			});
	}

	[ClientRpc]
	public void RpcDoMove(Vector3 vector3, float duration)
	{
		Debug.Log("DoMove " + vector3, this);
		DoMove(vector3, duration);
	}
	#endregion
}