using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class BindingPoisonState : AbstractCharacterState
{
    private SkillManager _skillManager;

    private int _maxStacks = 1;

    private float _duration;
    private float _baseDuration;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Poison };
    public int CurrentStacks { get => CurrentStacksCount; set => CurrentStacksCount = value; }
    public float StacksDuration { get => _duration; }

    public override States State => States.BindingPoison;
    public override StateType Type => StateType.Physical;
    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;

    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        Debug.Log("BindingPoisonState / EnterState");
        _characterState = character;

        _skillManager = _characterState.Character.Abilities;

        _duration = durationToExit;
        _baseDuration = durationToExit;
        MaxStacksCount = _maxStacks;

        if (CurrentStacksCount < MaxStacksCount)
        {
            AddStacks();
        }

        BlockingOrCancleingAbility();
    }

    public override void UpdateState()
    {
        if (CurrentStacksCount <= 0)
        {
            ExitState();
        }

        //Debug.Log($"BindingPoisonState / UpdateState / CharacterManager = {_skillManager}");
        if (_duration < 0)
        {
            ExitState();
        }

    }

    public override void ExitState()
    {
        //Debug.Log($"BindingPoisonState / ExitState / CharacterManager = {_skillManager}");
        ResetValues();

        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        //Debug.Log($"BindingPoisonState / Stack / CharacterManager = {_skillManager}");
        if (CurrentStacksCount < MaxStacksCount)
        {
            AddStacks();
            return true;
        }
        else
        {
            _duration = _baseDuration;
            return true;
        }
    }

    public void AddStacks()
    {
        if (CurrentStacksCount < MaxStacksCount)
        {
            CurrentStacksCount++;
            //Debug.Log("if / CurrentStackPoisonBone in AddStacks == " + _currentStacks);
            _duration = _baseDuration;
        }
        else
        {
            //Debug.Log("else / CurrentStackPoisonBone in AddStacks == " + _currentStacks);
            _duration = _baseDuration;
        }
    }

    [TargetRpc]
    private void BlockingOrCancleingAbility()
    {
        _skillManager.SkillQueue.TryCancel(true);

        if (!_skillManager.SkillQueue.TryCancel(true))
        {
            _skillManager.SkillQueue.SkillAdded += OnSkillAdded;
        }
        ExitState();
    }

    private void OnSkillAdded(Skill skill)
    {
        _skillManager.SkillQueue.TryCancel(true);

        _skillManager.SkillQueue.SkillAdded -= OnSkillAdded;
    }

    private void ResetValues()
    {
        CurrentStacksCount = 0;
        _baseDuration = 0;
        _duration = 0;
    }
}
