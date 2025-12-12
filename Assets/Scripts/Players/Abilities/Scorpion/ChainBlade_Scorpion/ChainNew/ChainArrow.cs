using Mirror;
using System.Collections;
using UnityEngine;
using System;

public class ChainArrow : Projectiles
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float speedModifier = 1.2f;
    [SerializeField] private float speedWithTarget = 4f;
    [SerializeField] private float stopDistance = 1.5f;
    [SerializeField] private LayerMask targetsLayer;
    [SerializeField] private Transform chainPoint;

    private Transform _playerTransform;
    private Vector3 _targetPoint;
    private float _maxDistance;
    private float _damage;

    private Coroutine _flyCoroutine;
    private Coroutine _returnCoroutine;
    private bool _isReturning = false;

    private Character _hookedTarget;
    private MoveComponent _hookedMove;

    private void OnDestroy()
    {
        if (_skill is ChainBlade chain) chain.ChainBladeCastEnd();
        MoveReset();
    }

    public void InitArrow(Vector3 targetPoint, Transform playerTransform, float maxDistance, float damage)
    {
        _targetPoint = targetPoint;
        _playerTransform = playerTransform;
        _maxDistance = maxDistance;
        _damage = damage;

        lineRenderer.positionCount = 2;
        _startPoint = transform.position;

        _flyCoroutine = StartCoroutine(FlyCoroutine());
    }

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        if (_isReturning) return;

        if (other.gameObject == _dad.gameObject) return;
        if (((1 << other.gameObject.layer) & _skill.TargetsLayers.value) == 0) return;

        if (other.TryGetComponent<Character>(out Character character))
        {
            AttachToTarget(character);
            AddSkillCombo(character);
            AddState(character);
            ApplyDamage(_damage, DamageType.Physical, character.gameObject);
        }
    }


    private IEnumerator FlyCoroutine()
    {
        Vector3 direction = (_targetPoint - transform.position).normalized;
        _rb.linearVelocity = Vector3.zero;
        _rb.AddForce(direction * speed, ForceMode.VelocityChange);

        float speedReturn = 0;

        while (!_isReturning)
        {
            UpdateLine();
            RotateArrow(direction);

            if (Vector3.Distance(_startPoint, transform.position) >= _maxDistance || _hookedTarget != null)
            {
                if (_hookedTarget == null) speedReturn = speed * speedModifier;
                else speedReturn = speedWithTarget;
                break;
            }

            yield return null;
        }

        StartReturn(speedReturn);
    }

    private void AttachToTarget(Character character)
    {
        _hookedTarget = character;
        _hookedMove = character.GetComponent<MoveComponent>();

        if (_hookedMove != null)
            _hookedMove.CanMove = false;

        transform.SetParent(character.transform);
        transform.localPosition = new Vector3(0f, 0.5f, 0f);
        _rb.linearVelocity = Vector3.zero;
        _rb.isKinematic = true;

        RpcAttachToTarget(character);
    }

    [ClientRpc]
    private void RpcAttachToTarget(Character character)
    {
        _hookedTarget = character;
        _hookedMove = character.GetComponent<MoveComponent>();

        if (_hookedMove != null)
            _hookedMove.CanMove = false;

        transform.SetParent(character.transform);
        transform.localPosition = new Vector3(0f, 0.5f, 0f);
        _rb.linearVelocity = Vector3.zero;
        _rb.isKinematic = true;
    }

    private IEnumerator ReturnCoroutine(float speed)
    {
        _rb.linearVelocity = Vector3.zero;

        if (_hookedTarget != null)
        {
            while (Vector3.Distance(_hookedTarget.transform.position, _playerTransform.position) > stopDistance)
            {
                if (_hookedMove != null)
                {
                    Vector3 targetPosition = _playerTransform.position;
                    targetPosition.y = 1.0f;

                    if (_hookedMove.connectionToClient != null)
                        _hookedMove.TargetRpcDoMove(targetPosition, stopDistance);
                    else
                        _hookedMove.TestDoMove(targetPosition, stopDistance); // �����, ��� �������� �� ����� ������� � Character, �� ������������������ �� ����
                }

                UpdateLine();
                yield return null;
            }

            ReleaseTarget();
        }

        else
        {
            Vector3 dir = (_playerTransform.position - transform.position).normalized;
            _rb.isKinematic = false;
            _rb.AddForce(dir * speed, ForceMode.VelocityChange);

            while (Vector3.Distance(transform.position, _playerTransform.position) > stopDistance)
            {
                UpdateLine();
                yield return null;
            }
        }

        if (isServer) NetworkServer.Destroy(gameObject);
    }

    private void StartReturn(float speed)
    {
        if (_isReturning) return;
        _isReturning = true;

        if (_flyCoroutine != null)
            StopCoroutine(_flyCoroutine);

        if (_skill is ChainBlade chain) chain.ChainBladeEnd();

        Debug.Log($"StartReturn Speed: {speed}");
        _returnCoroutine = StartCoroutine(ReturnCoroutine(speed));
    }

    private void UpdateLine()
    {
        if (_playerTransform == null || chainPoint == null || lineRenderer == null) return;

        lineRenderer.SetPosition(0, _playerTransform.position);
        lineRenderer.SetPosition(1, chainPoint.position);
    }

    private void RotateArrow(Vector3 direction)
    {
        if (direction == Vector3.zero) return;
        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(-90, 0, 0);
    }

    private void ApplyDamage(float damage, DamageType damageType, GameObject target)
    {
        Damage _damage = new Damage
        {
            Value = damage,
            Type = damageType
        };

        _skill.ApplyDamage(_damage, target);
    }

    private void MoveReset()
    {
        _dad.Move.CanMove = true;
    }

    private void AddSkillCombo(Character character)
    {
        if (character == null) return;
         
        if (_skill is ChainBlade skill) 
        {
            skill.ComboCounter.AddSkill(character, skill);
        }
    }

    private void AddState(Character character)
    {
        if (character == null) return;

        float pullDistance = Vector3.Distance(_playerTransform.position, character.transform.position);

        if (pullDistance > 1f)
        {
            float duration = 1f;

            if (_skill is ChainBlade skill) if (skill.ComboCounter.IsFinalComboSkill(character, skill)) duration += 2f;

            int comboStacks = character.CharacterState.CheckStateStacks(States.ComboState);
            duration += comboStacks;

            character.CharacterState.AddState(States.DisappointmentState, duration, 0f, _dad.gameObject, _skill.name);
        }
    }

    private void ReleaseTarget()
    {
        if (_hookedMove != null)
            _hookedMove.CanMove = true;

        _hookedTarget = null;
        _hookedMove = null;
        _isReturning = false;

        transform.SetParent(null);
        _rb.isKinematic = false;

        if (isServer) RpcReleaseTarget();
    }

    [ClientRpc]
    private void RpcReleaseTarget()
    {
        if (_hookedMove != null)
            _hookedMove.CanMove = true;

        _hookedTarget = null;
        _hookedMove = null;
        _isReturning = false;

        transform.SetParent(null);
        _rb.isKinematic = false;
    }

    private Vector3 _startPoint;
}
