using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class MetabolismReptile : Skill
{
    [SerializeField] private Character _player;
    [SerializeField] private MetabolismReptileTalent _metabolismReptileTalent; 

    [SerializeField] private PoisonBall _poisonBall;
    [SerializeField] private SpitPoison _spitPoison; 
 
    [SerializeField] private float _duration = 3f;

    private float _originalHpRegen;
    private float _increaseHealthRegen = 2f;
    private float _increaseCastTime = 2f;
    private float _increaseCooldownTime = 2f;

    protected override int AnimTriggerCast => 0;
    protected override int AnimTriggerCastDelay => 0;

    protected override bool IsCanCast => _metabolismReptileTalent.Data.IsOpen;

    private void Start()
    {
        _originalHpRegen = _player.Health.RegenerationValue;
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> targetDataSavedCallback)
    {
        yield return null;
    }

    protected override IEnumerator CastJob()
    {
        ApplyBuff();

        yield return null;
    }

    protected override void ClearData()
    {
    }

    private void ApplyBuff()
    {
        CmdIncreaseHealthRegen(_player.gameObject, _originalHpRegen, _increaseHealthRegen);

        ReductionCooldownAndCastTimeSpells();

        Invoke("RemoveBuff", _duration);
    }

    private void RemoveBuff()
    {
        CmdRemoveHpRegen(_player.gameObject, _originalHpRegen);

        ResetCastTimeToBase();
    }

    private void ReductionCooldownAndCastTimeSpells()
    {
        float newRemainingCooldownForSpitPoison = _spitPoison.CooldownTime / _increaseCooldownTime;
        _spitPoison.ReductionSetCooldown(newRemainingCooldownForSpitPoison);

        //Сделать потом уменьшение кулдаунов зарядов для PoisonBall

        _poisonBall.Buff.CastSpeed.ReductionPercentage(_increaseCastTime);
        _spitPoison.Buff.CastSpeed.ReductionPercentage(_increaseCastTime);
    }

    private void ResetCastTimeToBase()
    {
        _poisonBall.Buff.CastSpeed.IncreasePercentage(_increaseCastTime);
        _spitPoison.Buff.CastSpeed.IncreasePercentage(_increaseCastTime);
    }

    #region CommandMethods

    [Command]
    private void CmdIncreaseHealthRegen(GameObject player, float originalHpRegen, float increaseHealthRegen)
    {
        Character playerCharacter = player.GetComponent<Character>();

        float increasedHpRegen = originalHpRegen * increaseHealthRegen;
        playerCharacter.Health.RegenerationValue = increasedHpRegen;
    }

    [Command]
    private void CmdRemoveHpRegen(GameObject player, float originalHealthRegen)
    {

        Character playerCharacter = player.GetComponent<Character>();

        playerCharacter.Health.RegenerationValue = originalHealthRegen;
    }

    #endregion
}
