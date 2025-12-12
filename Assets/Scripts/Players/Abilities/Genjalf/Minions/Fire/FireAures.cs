using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Unity.Collections;
using UnityEngine;

public class FireAures : MonoBehaviour
{
    private void Start()
    {
        var chatacter = GetComponent<Character>();
        chatacter.CharacterState.CmdAddState(States.Burn, 0, 0, chatacter.gameObject, name);
    }
}

public class Burn : AuraState
{
    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Poison };
    private float _damage = 1;

    public override float Distance => 2;
    public override float EffectRate => 1f;
    public override LayerMask LayerMask => LayerMask.GetMask("Allies");
    public override States State => States.Burn;
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override List<StatusEffect> Effects => _effects;

    public override void EffectOnEnter(Character character)
    {

    }

    public override void EffectOnExit(Character character)
    {

    }

    public override void EffectOnStay(List<Character> characters)
    {
        foreach (Character character in characters)
        {
            if (character == _self)
                continue;

            Damage damage = new Damage
            {
                Value = _damage,
            };
            character.CmdTryTakeDamage(damage, null);
        }
    }
}

public class Burning : AbstractCharacterState
{
    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Poison };
    private float _damage = 1;
    private Character _character;
    private float _timeAfterLastEffect = 0;
    private float _effectRate = 1;
    private float _time;

    public override States State => States.Burning;

    public override StateType Type => StateType.Magic;

    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;

    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _time = durationToExit;
        _character = character.Character;
        Damage damage = new Damage
        {
            Value = _damage,
        };
        character.Character.TryTakeDamage(ref damage, null);
    }

    public override void ExitState()
    {
        _character.CharacterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        return false;
    }

    public override void UpdateState()
    {
        _time -= Time.deltaTime;
        if (_time <= 0)
        {
            ExitState();
        }

        _timeAfterLastEffect += Time.deltaTime;

        if (_effectRate > _timeAfterLastEffect)
            return;


        Damage damage = new Damage
        {
            Value = _damage,
        };
        _character.TryTakeDamage(ref damage, null);

        _timeAfterLastEffect = 0;
    }
}

