using System.Collections;
using UnityEngine;
using Mirror;
using System;

public class Kick_Scorpion : Skill
{
    [Header("Ability settings")]
    [SerializeField] private Character _playerLinks;
    [SerializeField] private PassiveCombo_Scorpion _comboCounter;
    [SerializeField] private ScorpionPassive scorpionPassive;
    [SerializeField] [Range(0, 100)] private float _minDamage = 10f;
    [SerializeField] [Range(0, 100)] private float _maxDamage = 15f;

    [Header("Talent Flags")]
    private bool isKick_ScorpionRowTalent;
    private bool isKick_ScorpionComboTalent;
    private bool isKick_ScorpionRowBonusTalent;

    [Header("Internal State")]
    [SerializeField] [Range(0f, 1f)] private float _baseDebuffChance = 0.3f;
    [SerializeField] [ReadOnly] private byte _hitsInRow = 1;

    private Coroutine _hitsInRowCoroutine;
    private Character _lastTarget = null;
    private Animator _animator;

    //private Character _target;
    private Character _runtimeTarget;

    private static readonly int KickTrigger = Animator.StringToHash("KickAA");

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => KickTrigger;

    protected override bool IsCanCast => GetTargetCharacter() != null && Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius && NoObstacles(GetTargetCharacter().transform.position, transform.position, _obstacle);

    public float DamageRange => UnityEngine.Random.Range(_minDamage, _maxDamage);

    private void Start() => _animator = GetComponent<Animator>();
    private void OnDisable() => OnSkillCanceled -= HandleSkillCanceled;
    private void OnEnable() => OnSkillCanceled += HandleSkillCanceled;

    private void HandleSkillCanceled()
    {
        ClearTarget();
        //_target = null;
        Hero.Move.StopLookAt();
        _hero.Move.CanMove = true;
    }

    public void Kick_ScorpionMoveFalse()
    {
        if (_hero == null || _hero.Move == null) return;

        var target = GetTargetCharacter() != null ? GetTargetCharacter() : _lastTarget;
        if (target == null)
        {
            _hero.Move.StopLookAt();
            return;
        }


        _hero.Move.StopMoveAndAnimationMove();
        _hero.Move.CanMove = false;

        Vector3 direction = target.transform.position - _hero.transform.position;
        bool badDirection = float.IsInfinity(target.transform.position.x) || direction.sqrMagnitude < 0.0001f;

        if (badDirection)
        {
            _hero.Move.StopLookAt();
            return;
        }

        _hero.Move.LookAtPosition(GetTargetCharacter().transform.position);
    }

    public void Kick_ScorpionMoveTrue()
    {
        _hero.Move.CanMove = true;
        Hero.Move.StopLookAt();
    }

    private bool IsTargetInRange()
    {
        return Vector3.Distance(_playerLinks.transform.position, GetTargetCharacter().transform.position) <= Radius;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        while (GetTargetCharacter() == null)
        {
            if (GetMouseButton)
            {
                //_target = GetRaycastTarget();
                FindTargetCharacter();
                if (GetTargetCharacter() != null)
					GetTargetCharacter().SelectedCircle.IsActive = true;
            }
            yield return null;
        }

        _hero.Move.LookAtTransform(GetTargetCharacter().transform);

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.Points.Add(GetTargetCharacter().transform.position);
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        if (GetTargetCharacter() == null) yield return null;
        if (!IsTargetInRange()) yield return null;

        _runtimeTarget = GetTargetCharacter();

        if (_lastTarget != null && _lastTarget != GetTargetCharacter())
            _comboCounter?.ResetCounter();

        if (_hitsInRowCoroutine != null)
            StopCoroutine(_hitsInRowCoroutine);

        _lastTarget = _runtimeTarget;

        ApplyAttackDamageKick();
    }

    private void ApplyAttackDamageKick()
    {
        if (_runtimeTarget == null) return;

        if (Vector2.Distance(_lastTarget.transform.position, _runtimeTarget.transform.position) > Radius)
            return;

        Damage damage = new Damage
        {
            Value = Buff.Damage.GetBuffedValue(DamageRange),
            Type = DamageType,
        };

        CmdApplyDamage(_runtimeTarget, damage);

        _runtimeTarget = null;
    }

    private IEnumerator HitsInRowTimer()
    {
        yield return new WaitForSeconds(2f);
        _hitsInRow = 1;
        _hitsInRowCoroutine = null;
    }

    private void AttackPassed(Character target)
    {
        _comboCounter.AddSkill(target, this);

        if (_hitsInRowCoroutine != null)
            StopCoroutine(_hitsInRowCoroutine);

        _hitsInRowCoroutine = StartCoroutine(HitsInRowTimer());

        var state = target.GetComponent<CharacterState>();
        float chance = 0f;

        if (isKick_ScorpionRowTalent)
        {
            if (scorpionPassive.IsAddStateUpdateChance)
            {
                if (state.CheckForState(States.DisappointmentState)) state?.AddState(States.Knockdown, 13f, 0, _hero.gameObject, name);
            }

            else
            {
                if (isKick_ScorpionRowBonusTalent)
                {
                    chance = _baseDebuffChance * Mathf.Pow(2, _hitsInRow - 1);

                    if (UnityEngine.Random.value <= Mathf.Clamp01(chance))
                    {
                        state?.AddState(States.Knockdown, 13f, 0, _hero.gameObject, name);
                        _hitsInRow = 1;
                    }

                    else _hitsInRow = (byte)Mathf.Min(_hitsInRow + 1, 4);
                }

                else
                {
                    chance = _baseDebuffChance;
                    if (UnityEngine.Random.value <= Mathf.Clamp01(chance)) state?.AddState(States.Knockdown, 13f, 0, _hero.gameObject, name);
                }
            }
        }

        else _hitsInRow = 1;

        if (isKick_ScorpionComboTalent && state != null)
        {
            int comboStacks = state.CheckStateStacks(States.ComboState);
            for (int i = 0; i < comboStacks; i++)
            {
                state.AddState(States.Knockdown, 6f, 0, _hero.gameObject, name);
            }
        }
    }

    [Command]
    private void CmdApplyDamage(Character targetObject, Damage damage)
    {
        if (targetObject == null) return;

        IDamageable targetHealth = targetObject.GetComponent<IDamageable>();
        if (targetHealth == null) return;

        bool isHit = targetHealth.TryTakeDamage(ref damage, this);
        Hero.DamageTracker.AddDamage(damage, targetObject.gameObject, isServerRequest: true);

        if (isHit) AttackPassed(targetObject);
    }

    public void Kick_ScorpionRowTalent(bool value)
    {
        isKick_ScorpionRowTalent = value;
    }

    public void Kick_ScorpionRowBonusTalent(bool value)
    {
        isKick_ScorpionRowBonusTalent = value;
    }

    public void Kick_ScorpionComboTalent(bool value)
    {
        isKick_ScorpionComboTalent = value;
    }

    public void Kick_ScorpionCast()
    {
        AnimStartCastCoroutine();
    }

    public void Kick_ScorpionEnded()
    {
        AnimCastEnded();
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo.GetTargets().Count > 0) SetTarget((Character)targetInfo.GetTargets()[0]);
    }

    protected override void ClearData()
    {

    }
}
