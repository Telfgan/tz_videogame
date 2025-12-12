using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class InnerDarkness : AbstractCharacterState
{
    private const float TimeDecreasePerStack = 2f;
    private float _durationRemaining;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Ability };

    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;
    public override States State => States.InnerDarkness;
    public override StateType Type => StateType.Magic;
    public override List<StatusEffect> Effects => _effects;
    public override float RemainingDuration => _durationRemaining;

    public InnerDarkness()
    {
        MaxStacksCount = 6;
        CurrentStacksCount = 1;
    }

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _personWhoMadeBuff = personWhoMadeBuff;
        _durationRemaining = durationToExit;
        var terrifyingElfAura = personWhoMadeBuff.GetComponent<TerrifyingElfAura>();


        if (personWhoMadeBuff != null && terrifyingElfAura.IsReductionRecharge)
        {
            SkillManager caster = personWhoMadeBuff.Abilities;
            foreach (Skill skill in caster.Abilities)
            {
                bool isDark = skill.School == Schools.Dark;
                bool isSpellish = skill.AbilityForm == AbilityForm.Magic || skill.AbilityForm == AbilityForm.Spell || skill.AbilityForm == AbilityForm.Both;

                if (isDark && isSpellish && !skill.IsCooldowned)
                {
                    float duration = skill.RemainingCooldownTime * 0.5f;
                    skill.DecreaseSetCooldown(duration);
                }
            }
        }
    }

    public override void UpdateState()
    {
        _durationRemaining -= Time.deltaTime;
        if (_durationRemaining <= 0) ExitState();
    }

    public override void ExitState()
    {
        _characterState.RemoveState(this);
        CurrentStacksCount = 1;
    }

    public override bool Stack(float time)
    {
        Debug.Log($"CurrentStacksCount: {CurrentStacksCount}");

        if(CurrentStacksCount < MaxStacksCount)
        {
            AddNewStack(time);
            return true;
        }

        else if (CurrentStacksCount == MaxStacksCount)
        {
            UpdateDurationForMaxStacks(time);
            return false;
        }

        return false;
    }

    private void AddNewStack(float time)
    {
        CurrentStacksCount++;

        if (CurrentStacksCount == MaxStacksCount) CmdStateFear();

        _durationRemaining = time - (CurrentStacksCount - 1) * TimeDecreasePerStack;
    }

    private void UpdateDurationForMaxStacks(float time)
    {
        _durationRemaining = time - (CurrentStacksCount - 1) * TimeDecreasePerStack;
        CmdStateFear();
        Debug.Log("обновление при максимальном стаке");
    }

    [Command] private void CmdStateFear() => ClientRpcStateFear();
    [ClientRpc] private void ClientRpcStateFear() { _characterState.AddStateLogic(States.Fear, Random.Range(0.7f, 1.4f), 0f, Schools.None, _personWhoMadeBuff.gameObject, null); }
}
