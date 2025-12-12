using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConsumeCombo_Scorpion : Skill
{
    private List<Character> _comboTargetsQueue = new List<Character>();

    public int AvailablePoints => _comboTargetsQueue.Sum(target =>
    {
        var state = target.CharacterState.GetState(States.ComboState) as ComboState;
        return state?.CurrentStacksCount ?? 0;
    });

    protected override bool IsCanCast => true;
    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => 0;

    private bool isConsumeCombo_ScorpionPhysicStateClear;

    public void ApplyComboEffect(Transform enemy)
    {
        if (!isServer) return;
        if (enemy == null) return;

        var targetCharacter = enemy.GetComponent<Character>();
        if (targetCharacter == null) return;

        var stateManager = targetCharacter.CharacterState;
        if (stateManager == null) return;

        var comboState = stateManager.GetState(States.ComboState) as ComboState;
        if (comboState == null || comboState.CurrentStacksCount <= 0)
        {
            if (!_comboTargetsQueue.Contains(targetCharacter))
                _comboTargetsQueue.Add(targetCharacter);
        }

        stateManager.AddState(States.ComboState, float.PositiveInfinity, 0f, _hero.gameObject, nameof(ConsumeCombo_Scorpion));
    }

    public int PayComboPoints(int amount, Character specificTarget = null)
    {
        int pointsConsumed = 0;

        if (specificTarget != null)
        {
            pointsConsumed = ConsumePointsFromTarget(specificTarget, amount);
        }
        else
        {
            pointsConsumed = ConsumePointsFromQueue(amount);
        }

        return pointsConsumed;
    }

    private int ConsumePointsFromTarget(Character target, int amount)
    {
        if (target == null) return 0;

        var state = target.CharacterState.GetState(States.ComboState) as ComboState;
        if (state == null) return 0;

        int availablePoints = state.CurrentStacksCount;
        int pointsToConsume = Mathf.Clamp(amount, 0, availablePoints);

        for (int i = 0; i < pointsToConsume; i++)
        {
            bool reduced = state.Stack(-1);
            if (reduced && isConsumeCombo_ScorpionPhysicStateClear)
            {
                _hero.CharacterState.DispelStates(StateType.Physical, true, true);
            }

            if (!reduced)
            {
                target.CharacterState.RemoveState(state);
                _comboTargetsQueue.Remove(target);
                break;
            }
        }

        return pointsToConsume;
    }

    private int ConsumePointsFromQueue(int amount)
    {
        int pointsToConsume = 0;

        while (amount > 0 && _comboTargetsQueue.Count > 0)
        {
            var lastTarget = _comboTargetsQueue[_comboTargetsQueue.Count - 1];
            var state = lastTarget.CharacterState.GetState(States.ComboState) as ComboState;

            if (state == null)
            {
                _comboTargetsQueue.RemoveAt(_comboTargetsQueue.Count - 1);
                continue;
            }

            bool reduced = state.Stack(-1);
            pointsToConsume++;
            amount--;

            if (reduced && isConsumeCombo_ScorpionPhysicStateClear)
            {
                _hero.CharacterState.DispelStates(StateType.Physical, true, true);
            }

            if (!reduced)
            {
                lastTarget.CharacterState.RemoveState(state);
                _comboTargetsQueue.RemoveAt(_comboTargetsQueue.Count - 1);
            }
        }

        return pointsToConsume;
    }

    public void ConsumeCombo_ScorpionPhysicStateClearTalent(bool value)
    {
        isConsumeCombo_ScorpionPhysicStateClear = value;
    }

    public void TryConsumeComboAroundSelf()
    {
        if (!isConsumeCombo_ScorpionPhysicStateClear || !isServer) return;

        List<Character> targetsInRadius = Physics.OverlapSphere(transform.position, Radius, TargetsLayers)
            .Select(c => c.GetComponent<Character>())
            .Where(c => c != null && c.CharacterState.CheckForState(States.ComboState))
            .ToList();

        foreach (var target in targetsInRadius)
        {
            var state = target.CharacterState.GetState(States.ComboState) as ComboState;
            if (state == null || state.CurrentStacksCount <= 0) continue;

            bool reduced = state.Stack(-1);
            if (reduced)
            {
                if (isConsumeCombo_ScorpionPhysicStateClear)
                {
                    _hero.CharacterState.DispelStates(StateType.Physical, true, true);
                }

                if (state.CurrentStacksCount <= 0)
                {
                    target.CharacterState.RemoveState(state);
                }
            }
        }
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved) => null;
    protected override IEnumerator CastJob() => null;
    protected override void ClearData() { }
    public override void LoadTargetData(TargetInfo targetInfo) => throw new NotImplementedException();
}