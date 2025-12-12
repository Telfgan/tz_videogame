using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class DoubleCheliceraStrike : Skill
{
    [SerializeField] private Character _player;
    [SerializeField] private CheliceraStrike cheliceraStrike;
    [SerializeField] private CooldownEnergy cooldownEnergy;
    [SerializeField] private float _cheliceraStrikeBaseDamage;
    [SerializeField] private float _damageMultiplier = 0.75f * 2f;
    [SerializeField] private float _stunDuration = 1f;
    [SerializeField] private float _stunDurationWithJumpBack = 2f;
    [SerializeField] private float cooldownEnergyCost = 5;

    //private IDamageable _target;
    //private Character _runtimeTarget;

    private static readonly int DoubleCheliceraStrikeAnimTrigger = Animator.StringToHash("DoubleCheliceraStrikeAnimation");

    protected override int AnimTriggerCast => DoubleCheliceraStrikeAnimTrigger;
    protected override int AnimTriggerCastDelay => 0;

    protected override bool IsCanCast => IsTargetInRange() && cooldownEnergy.CurrentValue >= cooldownEnergyCost;

    private void OnEnable()
    {
        _cheliceraStrikeBaseDamage = cheliceraStrike.Damage;
        OnSkillCanceled += HandleSkillCanceled;
    }

    private void OnDestroy() => OnSkillCanceled -= HandleSkillCanceled;

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        //_runtimeTarget = null;

        while (GetTargetCharacter() == null)
        {
            if (GetMouseButton)
            {
                FindTargetCharacter();
                //_target = GetRaycastTarget();

                if (GetTargetCharacter() != null)
                {
                    if (GetTargetCharacter() is Character characterTarget)
                    {
                        //_runtimeTarget = characterTarget;
                        characterTarget.SelectedCircle.IsActive = true;
                    }
                }
            }

            _isCanCancle = false;

            yield return null;
        }

        _player.Move.CanMove = false;
        TargetInfo targetInfo = new TargetInfo();
        targetInfo.AddTarget(GetTargetCharacter());
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        if (GetTargetCharacter() == null) yield return null;

        DealDoubleCheliceraStrikeDamage(GetTargetCharacter());

        cooldownEnergy.CastCooldownEnergySkill(cooldownEnergyCost, this);

        yield return null;
    }

    private bool IsTargetInRange()
    {
        return GetTargetCharacter() != null &&
            Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius &&
            NoObstacles(GetTargetCharacter().transform.position, transform.position, _obstacle);
    }

    private void HandleSkillCanceled()
    {
        ClearTarget();
        //_target = null;
        _isCanCancle = true;
    }

    private void DealDoubleCheliceraStrikeDamage(IDamageable targetCharacter)
    {
        float totalDamage = _cheliceraStrikeBaseDamage * _damageMultiplier;

        Damage damage = new Damage
        {
            Value = totalDamage,
            Type = DamageType.Physical,
            PhysicAttackType = AttackRangeType.MeleeAttack
        };

        CmdApplyDamage(damage, targetCharacter.gameObject);
        if (targetCharacter is Character character) CmdApplyStun(character);
    }

    public void DoubleCheliceraStrikeCast()
    {
        AnimStartCastCoroutine();
    }

    public void DoubleCheliceraStrikeEnded()
    {
        _isCanCancle = true;
        AnimCastEnded();
    }

    [Command]
    private void CmdApplyStun(Character target)
    {
        var lastSkill = _player.Abilities.LastCastedSkill;

        if ((lastSkill is JumpBack))  target.CharacterState.AddState(States.Stun, _stunDurationWithJumpBack, 0, _player.gameObject, null);
        else target.CharacterState.AddState(States.Stun, _stunDuration, 0, _player.gameObject, null);
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo.GetTargets().Count > 0) SetTarget((Character)targetInfo.GetTargets()[0]);
        _isCanCancle = false;
    }

    protected override void ClearData()
    {
        ClearTarget();
        //_target = null;
    }
}