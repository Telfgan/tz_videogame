using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Fear : AbstractCharacterState
{
    private float _duration;
    private float _baseDuration;
    private Character _source;
    private bool _previousIsSelect;
    private Coroutine _moveCoroutine;
    private SkillManager _skillManager;
    private List<Skill> _disabledSkills = new List<Skill>();

    public override States State => States.Fear;
    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;
    public override StateType Type => StateType.Immaterial;
    public override List<StatusEffect> Effects => new List<StatusEffect>();

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _source = personWhoMadeBuff;
        _duration = durationToExit;
        _baseDuration = durationToExit;
        MaxStacksCount = 1;

        MoveComponent moveComponent = _characterState.Character.Move;
        _skillManager = _characterState.Character.Abilities;

        if (moveComponent != null)
        {
            _previousIsSelect = moveComponent.IsSelect;

            moveComponent.StopLookAt();
            moveComponent.IsSelect = false;
            moveComponent.IsMoving = true;

            if (_moveCoroutine != null) _characterState.StopCoroutine(_moveCoroutine);

            _moveCoroutine = _characterState.StartCoroutine(MoveAwayCoroutine(moveComponent));
        }

        Debug.Log("Страх");
    }

    public override void UpdateState()
    {
        if (_skillManager != null)
        {
            foreach (var skill in _skillManager.Abilities)
            {
                if (!skill.Disactive)
                {
                    skill.Disactive = true;
                    _disabledSkills.Add(skill);
                }
            }
        }

        _duration -= Time.deltaTime;
        if (_duration <= 0f)
        {
            ExitState();
        }
    }

    public override void ExitState()
    {
        if (_moveCoroutine != null)
        {
            _characterState.StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }

        MoveComponent moveComp = _characterState.Character.Move;
        if (moveComp != null)
        {
            moveComp.IsSelect = _previousIsSelect;
            moveComp.IsMoving = false;
            moveComp.ExternalMoveDirection = Vector3.zero;
            moveComp.SetDefaultSpeed();
            moveComp.StopLookAt();
            moveComp.Rigidbody.linearVelocity = Vector3.zero;
            moveComp.SetAnimationMovement(Vector3.zero);
        }

        foreach (var skill in _disabledSkills)
        {
            skill.Disactive = false;
        }
        _disabledSkills.Clear();
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        return false;
    }

    private void InitializeFirstStack()
    {
        _duration = _baseDuration;
        CurrentStacksCount++;
    }

    private IEnumerator MoveAwayCoroutine(MoveComponent moveComp)
    {
        if (_source == null || _characterState == null || moveComp == null) yield break;

        Rigidbody rb = moveComp.Rigidbody;
        if (rb == null) yield break;

        Vector3 fleeDir = _source ? (moveComp.transform.position - _source.transform.position).normalized : Random.insideUnitSphere.normalized;

        fleeDir.y = 0;

        Vector3 fleeDirection = (moveComp.transform.position - _source.transform.position).normalized;
        fleeDirection = Quaternion.Euler(0, Random.Range(-45f, 45f), 0) * fleeDirection;

        moveComp.SetAnimationMovement(Vector3.zero);

        float changeDirectionInterval = Random.Range(0.5f, 1.5f);
        float timeSinceLastChange = 0f;

        while (_duration > 0f)
        {
            _duration -= Time.deltaTime;
            timeSinceLastChange += Time.deltaTime;

            if (_source) fleeDir = (moveComp.transform.position - _source.transform.position).normalized;

            if (timeSinceLastChange >= changeDirectionInterval)
            {
                timeSinceLastChange = 0f;
                changeDirectionInterval = Random.Range(0.5f, 1.5f);
                fleeDirection = Quaternion.Euler(0, Random.Range(-60f, 60f), 0) * fleeDirection;
            }

            Vector3 newDirection = Vector3.Lerp(fleeDirection, (moveComp.transform.position - _source.transform.position).normalized, Time.deltaTime * 1.5f).normalized;
            moveComp.Rigidbody.linearVelocity = newDirection * moveComp.CurrentSpeed;

            if (moveComp.Rigidbody.linearVelocity.magnitude > 0.1f)
            {
                moveComp.transform.rotation = Quaternion.Slerp(
                    moveComp.transform.rotation,
                    Quaternion.LookRotation(moveComp.Rigidbody.linearVelocity.normalized),
                    Time.deltaTime * 5f
                );
            }

            moveComp.SetAnimationMovement(moveComp.Rigidbody.linearVelocity);
            yield return null;
        }

        moveComp.SetAnimationMovement(Vector3.zero);
        rb.linearVelocity = Vector3.zero;
    }
}