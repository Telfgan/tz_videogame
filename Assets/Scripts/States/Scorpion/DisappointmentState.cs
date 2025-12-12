using System.Collections.Generic;
using UnityEngine;

public class DisappointmentState : AbstractCharacterState
{
    private float _duration;
    private float _baseDuration;
    private float _damageToExit;
    private float _damageOnStart = 0;
    private Animator _animator;
    private AnimatorStateInfo _currentState;
    private List<StatusEffect> _effects = new List<StatusEffect> { StatusEffect.Move, StatusEffect.Ability };
    
    public override States State => States.DisappointmentState;
    public override StateType Type => StateType.Physical;
    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _duration = durationToExit;
        _baseDuration = durationToExit;
        _damageToExit = damageToExit == 0 ? 10000 : damageToExit;
        _damageOnStart = _characterState.Character.Health.SumDamageTaken;

        _characterState.Character.Move.CanMove = false;
        _characterState.Character.Move.LookAtTransform(_characterState.transform);

        if (_characterState.Character.TryGetComponent(out SkillManager abilities))
        {
            _abilities = abilities;
            foreach (var skill in _abilities.Abilities)
            {
                skill.Disactive = true;
            }
        }
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;
        if (_characterState.Character.Health.SumDamageTaken - _damageOnStart >= _damageToExit || _duration <= 0)
        {
            ExitState();
        }
    }

    public override void ExitState()
    {
        _characterState.RemoveState(this);

        if (!_characterState.Check(StatusEffect.Move))
        {
            _characterState.Character.Move.CanMove = true;
            _characterState.Character.Move.StopLookAt();
        }

        if (!_characterState.Check(StatusEffect.Ability) && _abilities != null)
        {
            foreach (var skill in _abilities.Abilities)
            {
                skill.Disactive = false;
            }
        }
    }

    public override bool Stack(float time)
    {
        _duration = _baseDuration;
        return true;
    }
}
