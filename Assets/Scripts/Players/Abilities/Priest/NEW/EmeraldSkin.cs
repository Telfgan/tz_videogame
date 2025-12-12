using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class EmeraldSkin : Skill
{
    [Header("Emerald Skin Settings")]
    [SerializeField] private float _buffDuration = 2f;
    
    //---------------- Talent 1 (Light Magic Boost)
    private int _lightMagicTalentBoostActiveToBuff = 0;

    protected override bool IsCanCast => CanCastCheck();

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => 0;

    public override void LoadTargetData(TargetInfo targetInfo)
    {

    }

    private bool CanCastCheck()
    {
        return !Hero.CharacterState.CheckForState(States.ReversePolarity);
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        yield return null;
    }

    protected override IEnumerator CastJob()
    {
        if (!CanCastCheck()) yield break;
        
        if (TryPayCost())
        {
            ApplyEmeraldSkinBuff();
        }

        yield return null;
    }
    
    //---------------- Talent 1 Logic: Physical Shield Boost ----------------
    public void EnableTalentLightMagicBoost(bool value)
    {
        _lightMagicTalentBoostActiveToBuff = value ? 1 : 0;
    }
    
    private void ApplyEmeraldSkinBuff()
    {
        CmdAddBuff(States.EmeraldSkin, _buffDuration, _lightMagicTalentBoostActiveToBuff, Hero.gameObject, Name);
    }

    [Command]
    private void CmdAddBuff(States state, float duration, float modifier, GameObject target, string skillName)
    {
        var characterState = target.GetComponent<CharacterState>();
        characterState.AddState(state, duration, modifier, target, skillName);
    }

    protected override void ClearData()
    {
    }
}