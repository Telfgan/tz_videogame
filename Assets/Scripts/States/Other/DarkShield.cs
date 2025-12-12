using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkShield : AbstractCharacterState
{
    private float _damageDebuffDelay = 0.2f;
    private float _maxDamagePerTick;
    private float _duration;
    private Health _healthComponent;
    private GameObject _darkShield;

    private Coroutine _damageCoroutine;

    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override States State => States.DarkShield;
    public override StateType Type => StateType.Immaterial;
    public override List<StatusEffect> Effects => new List<StatusEffect>();

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _duration = durationToExit;
        _maxDamagePerTick = damageToExit;

        _healthComponent = character.GetComponent<Health>();
        if (_healthComponent != null)
        {
            _healthComponent.DamageTaken += HandleDamageTaken;
        }

        if (_characterState.StateEffects.DarkShield != null)
        {
            _darkShield = _characterState.StateEffects.DarkShield;
            _darkShield.SetActive(true);
        }
    }

    public override void ExitState()
    {
        if (_healthComponent != null)
        {
            if (_damageCoroutine != null)
            {
                _healthComponent.StopCoroutine(_damageCoroutine);
                _damageCoroutine = null;
            }

            _healthComponent.DamageTaken -= HandleDamageTaken;
        }

        if (_darkShield != null) _darkShield.SetActive(false);
        _characterState.RemoveState(this);
    }

    private void HandleDamageTaken(Damage damage, Skill skill)
    {
        if (_healthComponent == null || skill == null) return;

        if (_damageCoroutine != null)
        {
            _healthComponent.StopCoroutine(_damageCoroutine);
            _damageCoroutine = null;
        }

        _damageCoroutine = _healthComponent.StartCoroutine(ApplyDelayedDamage(damage.Value));
    }

    private IEnumerator ApplyDelayedDamage(float damage)
    {
        yield return new WaitForSeconds(_damageDebuffDelay);

        var damageToApply = Mathf.Min(damage, _maxDamagePerTick);
        var damageToTake = new Damage { Value = damageToApply };

        _healthComponent.CmdTryTakeDamage(damageToTake, null);
        _healthComponent.GetComponent<Character>().DamageTracker.AddDamage(damageToTake, null, isServerRequest: true);
    }

    public override bool Stack(float time)
    {
        _duration = time;
        return true;
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;
        if (_duration <= 0)
        {
            ExitState();
        }
    }
}