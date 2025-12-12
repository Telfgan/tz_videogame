using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class Conversion : Skill
{
    [SerializeField] private BasePsionicEnergy _psionicEnergy;
    [SerializeField] private AttackingPsionicEnergy _attackingPsionicEnergy;

    protected override int AnimTriggerCast => 0;
    protected override int AnimTriggerCastDelay => 0;
    protected override bool IsCanCast => _psionicEnergy.CurrentValue > 0;

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        throw new NotImplementedException(); 
    }

    protected override void ClearData()
    {
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> targetDataSavedCallback)
    {
        yield break;
    }

    protected override IEnumerator CastJob()
    {
        if (_psionicEnergy != null && _attackingPsionicEnergy != null)
        {
            if (_psionicEnergy.CurrentValue > 0)
            {
                CmdConvertEnergy();
            }
        }

        var lastSkill = Hero.Abilities.LastCastedSkill;
        if (lastSkill.AutoAttack == AutoAttack.autoAttack) lastSkill.TryPreparing();

        yield break;
    }

    [Command]
    private void CmdConvertEnergy()
    {
        _psionicEnergy.ConvertToAttackingEnergy(_attackingPsionicEnergy.MaxAttackingPsiEnergy);
    }
}
