using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class BleedingSpell : Skill
{
    [SerializeField] private Character _playerLinks;
    [SerializeField] private float duration;
    [SerializeField] private float _baseDamage = 10;

    //private Character _target;
    private Vector3 _targetPoint = Vector3.positiveInfinity;

    protected override bool IsCanCast => GetTargetCharacter() != null && Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius;
    protected override int AnimTriggerCastDelay => Animator.StringToHash("SpellCastDelayAnimTrigger");
    protected override int AnimTriggerCast => 0;

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        Damage = duration * _baseDamage;

        var multiMagic = Hero.CharacterState.GetState(States.MultiMagic) as MultiMagic;

        while (GetTargetCharacter() == null && !_disactive)
        {
            if (GetMouseButton)
            {
                FindTargetCharacter();
                //_target = GetRaycastTarget(true);
                if (multiMagic != null) multiMagic.LastTarget = GetTargetCharacter();
            }
            yield return null;
        }

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.AddTarget(GetTargetCharacter());
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        Damage = _baseDamage;
        if (GetTargetCharacter() != null) CmdApplyAbsorptionState(GetTargetCharacter().gameObject);

        var multiMagic = Hero.CharacterState.GetState(States.MultiMagic) as MultiMagic;

        if (multiMagic != null)
        {
            foreach (var character in multiMagic.PopPendingTargets())
            {
                TryPayCost();
                CmdApplyAbsorptionState(character.gameObject);
            }
        }

        AfterCastJob();

        yield return null;
    }

    protected override void ClearData()
    {
        _targetPoint = Vector3.positiveInfinity;
        ClearTarget();
        //_target = null;
    }

    [Command]
    private void CmdApplyAbsorptionState(GameObject targetGameObject)
    {
        var targetCharacter = targetGameObject.GetComponent<Character>();
        if (targetCharacter != null)
        {
            targetCharacter.CharacterState.AddState(States.Bleeding, duration, Damage, _playerLinks.gameObject, name);
        }
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo.GetTargets().Count > 0) SetTarget(targetInfo.GetTargets()[0] as Character);
    }
}

