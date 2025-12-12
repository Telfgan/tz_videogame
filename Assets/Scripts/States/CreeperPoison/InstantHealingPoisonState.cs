using Mirror;
using System.Collections.Generic;

public class InstantHealingPoisonState : AbstractCharacterState
{
    /* For PoisonBall Ability */

    private Character _player;
    private HealingPoisonPerSecondState _healingPoisonPerSecondState;

    private int _maxStacks = 1;

    private float _baseHealingValue = 14.0f;

    private float _totalHealed;

    private float _timeBetweenHeal;
    private float _startTimeBetweenHeal = 1.0f;

    private float _duration;
    private float _baseDuration;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Healing };

    public override States State => States.InstantHealingPoison;
    public override StateType Type => StateType.Physical;
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        MaxStacksCount = _maxStacks;

        _characterState = character;

        _duration = durationToExit;
        _baseDuration = durationToExit;
        _player = personWhoMadeBuff;
    }

    public override void UpdateState()
    {
        MakeHeal();
    }

    public override void ExitState()
    {
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        return false;
    }

    [Server]
    private void MakeHeal()
    {
        if (_characterState.CheckForState(States.HealingPoisonPerSecond))
        {
            _healingPoisonPerSecondState = (HealingPoisonPerSecondState)_characterState.GetState(States.HealingPoisonPerSecond);
            float multiplierHealValue = _healingPoisonPerSecondState.TotalHealValue;
            _totalHealed = _baseHealingValue + multiplierHealValue;
        }
        else
        {
            _totalHealed = _baseHealingValue;
        }

        Heal heal = new Heal
        {
            Value = _totalHealed,
            DamageableSkill = null,
        };

        _characterState.Character.Health.Heal(ref heal, null);

        ExitState();
    }

}
