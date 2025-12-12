using Mirror;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class CleavingBlade_Scorpion : Skill
{
    [Header("Ability settings")]
    [SerializeField] private PassiveCombo_Scorpion _comboCounter;
    [SerializeField] private ScorpionPassive scorpionPassive;
    [SerializeField] [Range(0, 100)] private float _minDamage = 18f;
    [SerializeField] [Range(0, 100)] private float _maxDamage = 26f;
    [SerializeField] private GameObject blade;

    [SyncVar] private int _counter = 1;
    //private Character _target;
    private Character _runtimeTarget;

    private bool isCleavingBlade_ScorpionSecondTalent;

    public float DamageRange => Random.Range(_minDamage, _maxDamage);

    protected override bool IsCanCast => GetTargetCharacter() != null && Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius;
    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => Animator.StringToHash("Cast Blade");

    private void OnDisable() => OnSkillCanceled -= HandleSkillCanceled;
    private void OnEnable() => OnSkillCanceled += HandleSkillCanceled;

    private void HandleSkillCanceled() => ClearTarget();

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        SetTarget((Character)targetInfo.GetTargets()[0]);
    }

    private void AttackPassed(bool shouldIncreaseCounter, Character target)
    {
        _comboCounter.AddSkill(target, this);

        if (_comboCounter.IsFinalComboSkill(target, this))
        {
            CharacterState state = target.GetComponent<CharacterState>();

            if (state != null)
            {
                state.AddState(States.Bleeding, 6f, 0, _hero.gameObject, name);

                int comboStacks = state.CheckStateStacks(States.ComboState);

                for (int i = 0; i < comboStacks; i++) state.AddState(States.Bleeding, 6f, 0, _hero.gameObject, name);
            }
        }

        if (shouldIncreaseCounter)
        {
            _counter = _counter == 3 ? 1 : _counter + 1;
        }
        ClearTarget();
        //_target = null;
    }

    private void AttackMissed()
    {
        Debug.LogWarning("CleavingBlade_Scorpion .AttackMissed - Промах");
        _counter = 1;
        _comboCounter.ResetCounter();

        //_target = null;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        while (GetTargetCharacter() == null)
        {
            if (GetMouseButton)
            {
                FindTargetCharacter();
                //_target = GetRaycastTarget(true);
            }
            yield return null;
        }

        TargetInfo targetInfo = new();
        targetInfo.AddTarget(GetTargetCharacter());
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        _runtimeTarget = GetTargetCharacter();
        TryAttack(true, 1f);
        yield return null;
    }

    private void SpeedAnimBlade_Scorpion()
    {
        float speed = 1f;

        if (isCleavingBlade_ScorpionSecondTalent && _counter == 2) speed = 0.8f;

        _hero.Animator.SetFloat("CastChainBladeSpeed", speed);
    }

    private void TryAttack(bool shouldIncreaseCounter, float damageMultiplier)
    {
        if (_runtimeTarget != null && Vector2.Distance(transform.position, _runtimeTarget.transform.position) <= Radius)
        {
            Damage damage = new Damage
            {
                Value = Buff.Damage.GetBuffedValue(DamageRange * damageMultiplier),
                Type = DamageType,
            };

            CmdAttack(damage, _runtimeTarget, shouldIncreaseCounter);

            _runtimeTarget = null;
        }
    }

    [Command]
    private void CmdAttack(Damage damage, Character hp, bool shouldIncreaseCounter)
    {
        if (_tempTargetForDamage != hp.transform)
        {
            _tempTargetForDamage = hp.transform;
            _tempForDamage = hp.GetComponent<IDamageable>();
        }

        bool result = _tempForDamage.TryTakeDamage(ref damage, this);
        AttackPassed(shouldIncreaseCounter, hp);
    }

    protected override void ClearData()
    {

    }

    public void BladeActive()
    {
        blade.SetActive(true);
        SpeedAnimBlade_Scorpion();
    }

    public void CleavingBlade_ScorpionCast()
    {
        AnimStartCastCoroutine();
    }

    public void CleavingBlade_ScorpionEnd()
    {
        AnimCastEnded();
        blade.SetActive(false);
    }

    public void CleavingBlade_ScorpionSecondTalent(bool value)
    {
        isCleavingBlade_ScorpionSecondTalent = value;
    }
}
