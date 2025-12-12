using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Calmness : AbstractCharacterState
{
    private const float _manaRegenPercent = 0.005f;
    private const int _baseMaxStacks = 2;
    private int _lastTreesCount;
    private float _duration;
    private float _regenAmount;

    private Resource manaResource;
    private Coroutine _regenRoutine;

    private List<StatusEffect> _effects = new List<StatusEffect> { StatusEffect.Healing };
    public override States State => States.Calmness;
    public override StateType Type => StateType.Magic;
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _health = character.Character.Health;
        manaResource = character.Character.TryGetResource(ResourceType.Mana);
        _personWhoMadeBuff = personWhoMadeBuff;
        MaxStacksCount = _baseMaxStacks;
        CurrentStacksCount = 1;
        _duration = durationToExit;

        RecalcRegenAmount();
        if (character.isServer) _regenRoutine = character.StartCoroutine(RegenTick());
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;
        if (_duration <= 0)
        {
            ExitState();
        }
    }

    public override void ExitState()
    {
        CurrentStacksCount = 0;
        if (_regenRoutine != null) _characterState.StopCoroutine(_regenRoutine);
        _characterState.StateIcons.RemoveItemByState(State);
        _characterState.RemoveState(this);
    }

    public override bool Stack(float newDuration)
    {
        _duration = Mathf.Max(_duration, newDuration);
        Debug.Log(MaxStacksCount);

        if (CurrentStacksCount < MaxStacksCount) CurrentStacksCount++;

        RecalcRegenAmount();

        return true;
    }

    public void UpdateTreesCount(int newTreesCount)
    {
        _lastTreesCount = newTreesCount;
        MaxStacksCount = _baseMaxStacks + _lastTreesCount;

        if (CurrentStacksCount > MaxStacksCount) CurrentStacksCount = MaxStacksCount;

        RecalcRegenAmount();
    }

    public void ApplyRegen()
    {
        if (manaResource != null && _regenAmount > 0) manaResource.Add(_regenAmount);
    }

    private void RecalcRegenAmount()
    {
        if (manaResource != null) _regenAmount = manaResource.MaxValue * _manaRegenPercent * CurrentStacksCount;
    }

    private IEnumerator RegenTick()
    {
        var wait = new WaitForSeconds(1f);

        while (_duration > 0)
        {
            yield return wait;

            if (manaResource == null) continue;
            if (!_characterState.isServer) continue;

            float missing = manaResource.MaxValue - manaResource.CurrentValue;
            if (missing <= 0f) continue;

            float amount = Mathf.Min(_regenAmount, missing);
            manaResource.Add(amount);
        }
    }

}
