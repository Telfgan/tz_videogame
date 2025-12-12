using Mirror;
using System.Collections;
using UnityEngine;
using System;

public class BlessingFilth : Skill
{
    [SerializeField] private float _durationBaffState = 60f;

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => 0;
    protected override bool IsCanCast => true;

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        bool targetSelected = false;

        while (!targetSelected)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Character selectedTarget = GetRaycastTarget(isCanTargetHimself: true);

                //if (selectedTarget != null)
                //{
                //    _tempTargetForDamage = selectedTarget.transform;
                //    targetSelected = true;
                //}
            }

            yield return null;
        }
    }

    protected override IEnumerator CastJob()
    {
        if (_tempTargetForDamage == null) yield break;
        Character targetCharacter = _tempTargetForDamage.GetComponent<Character>();
        if (targetCharacter == null) yield break;

        ApplyBaffState(targetCharacter);

        yield break;
    }

    [Command]
    private void ApplyBaffState(Character target)
    {
        if (target == null) return;

        target.CharacterState.AddState(States.BaffState, _durationBaffState, 0, _hero.gameObject, this.Name);
    }

    protected override void ClearData()
    {
        _tempTargetForDamage = null;
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        throw new NotImplementedException();
    }
}
