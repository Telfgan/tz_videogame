using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WombApplyStateInRadius : Skill, IPassiveSkill
{
    [SerializeField] private float _tick = 0.1f;
    [SerializeField] private float _radiusGrowthInterval = 1f;
    [SerializeField] private float _maxRadius = 6f;

    private readonly HashSet<Character> _inZoneCharacters = new();
    private readonly Dictionary<Character, Coroutine> _slimeCoroutines = new();
    private float _currentRadius = 0f;
    private Coroutine _mainRoutine;
    private Coroutine _radiusRoutine;

    public void OnEnable()
    {
        _mainRoutine = StartCoroutine(CheckZoneRoutine());
        _radiusRoutine = StartCoroutine(RadiusGrowthRoutine());
    }

    private void OnDisable()
    {
        if (_mainRoutine != null) StopCoroutine(_mainRoutine);
        if (_radiusRoutine != null) StopCoroutine(_radiusRoutine);
        ClearAllStates();
    }

    private IEnumerator RadiusGrowthRoutine()
    {
        WaitForSeconds wait = new(_radiusGrowthInterval);
        while (_currentRadius < _maxRadius)
        {
            _currentRadius += 1f;
            yield return wait;
        }
    }

    private IEnumerator CheckZoneRoutine()
    {
        WaitForSeconds wait = new(_tick);
        while (true)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, _currentRadius);
            HashSet<Character> current = new();

            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent(out Character target)) continue;

                var hasPsi = target.GetComponent<PsionicEnergySkill>() != null ||
                             (target.CharacterParent != null && target.CharacterParent.GetComponent<PsionicEnergySkill>() != null);
                if (!hasPsi) continue;

                current.Add(target);

                if (_inZoneCharacters.Add(target))
                {
                    AddHealingSlime(target);
                    var routine = StartCoroutine(ApplyHealingSlimeRoutine(target));
                    _slimeCoroutines[target] = routine;
                }
            }

            foreach (var character in _inZoneCharacters)
            {
                if (character == null || current.Contains(character)) continue;
                RemoveHealingSlime(character);
            }

            _inZoneCharacters.RemoveWhere(character => character == null || !current.Contains(character));

            yield return wait;
        }
    }

    private IEnumerator ApplyHealingSlimeRoutine(Character character)
    {
        WaitForSeconds wait = new(1f);

        while (_inZoneCharacters.Contains(character))
        {
            if (character.TryGetComponent(out CharacterState state))
            {
                if (state.GetState(States.HealingSlime) is HealingSlime slime)
                {
                    if (slime.CurrentStacksCount < slime.MaxStacksCount)
                        state.CmdAddState(States.HealingSlime, 9999f, 0f, gameObject, name);
                }
            }

            yield return wait;
        }
    }

    private void AddHealingSlime(Character character)
    {
        if (!character.TryGetComponent(out CharacterState state)) return;

        if (state.GetState(States.HealingSlime) is HealingSlime)
            state.CmdAddState(States.HealingSlime, 9999f, 0f, gameObject, name);
        else
            state.CmdAddState(States.HealingSlime, 9999f, 0f, gameObject, name);
    }

    private void RemoveHealingSlime(Character character)
    {
        if (_slimeCoroutines.TryGetValue(character, out Coroutine routine))
        {
            StopCoroutine(routine);
            _slimeCoroutines.Remove(character);
        }

        if (character.TryGetComponent(out CharacterState state))
        {
            if (state.GetState(States.HealingSlime) is HealingSlime healingSlime)
            {
                healingSlime.SwitchToFinite();
            }
        }
    }

    private void ClearAllStates()
    {
        foreach (var character in _inZoneCharacters)
        {
            RemoveHealingSlime(character);
        }

        _inZoneCharacters.Clear();
        _slimeCoroutines.Clear();
    }

    #region NotUsedSkillOverrides
    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => 0;
    public override void LoadTargetData(TargetInfo targetInfo) { }
    protected override IEnumerator CastJob() => null;
    protected override void ClearData() { }
    protected override IEnumerator PrepareJob(Action<TargetInfo> callback) => null;
    #endregion
}
