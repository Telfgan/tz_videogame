using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class SpeedOfReptile : Skill
{
    [SerializeField] private Character _player;
    [SerializeField] private CreeperStrike _creeperStrike;
    [SerializeField] private SpeedOfReptileTalent _speedOfReptileTalent;

    [SerializeField]private float _duration = 3f;

    private float _increaseMoveSpeed = 2f;
    private float _increaseAttackSpeed = 2f;
    private float _increaseEvasion = 2f;

    protected override int AnimTriggerCast => 0;
    protected override int AnimTriggerCastDelay => 0;
    protected override bool IsCanCast => _speedOfReptileTalent.Data.IsOpen;

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        yield return null;
    }

    protected override IEnumerator CastJob()
    {
        IncreaseValues();
        yield return null;
    }

    protected override void ClearData()
    {
    }

    private void IncreaseValues()
    {
        _creeperStrike.Buff.AttackSpeed.ReductionPercentage(_increaseAttackSpeed);
        CmdIncreaseValues();

        Invoke("ResetValues", _duration);
    }

    private void ResetValues()
    {
        _creeperStrike.Buff.AttackSpeed.IncreasePercentage(_increaseAttackSpeed);
        CmdResetValues();
    }

    [Command]
    private void CmdIncreaseValues()
    {
        _player.Health.ResistMagDamage *= _increaseEvasion;
        _player.Health.EvadeMeleeDamage *= _increaseEvasion; 
        _player.Health.EvadeRangeDamage *= _increaseEvasion; 

        _player.Move.ChangeMoveSpeed(_increaseMoveSpeed);
    }

    [Command]
    private void CmdResetValues()
    {
        _player.Health.ResistMagDamage /= _increaseEvasion;
        _player.Health.EvadeMeleeDamage /= _increaseEvasion;
        _player.Health.EvadeRangeDamage /= _increaseEvasion;

        _player.Move.SetDefaultSpeed();
    }

}
