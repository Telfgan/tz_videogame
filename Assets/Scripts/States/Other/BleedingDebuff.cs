using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedingDebuff : AbstractCharacterState
{
    private float _duration;
    private float _baseDuration;
    private float timer = 0;
    public override States State => States.Bleeding;
    
    public override StateType Type => StateType.Physical;
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override List<StatusEffect> Effects => throw new System.NotImplementedException();

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        Debug.Log("Entering KnockdownDebuff State");
        _characterState = character;

        //effects.Add(StatusEffect.Others);

        _duration = durationToExit;
        _baseDuration = durationToExit;
    }

    public override void ExitState()
    {
        Debug.Log("Exiting KnockdownDebuff State");

        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        _duration = _baseDuration;
        return true;
    }

    public override void UpdateState()
    {
        Debug.Log("Updating KnockdownDebuff State");
        _duration -= Time.deltaTime;

        timer += Time.deltaTime;

        if (timer >= 1f)
        {
            DealDamage();
            timer = 0f;
        }

        if (_duration < 0)
        {
            ExitState();
        }

    }

    private void DealDamage()
    {
        Debug.LogWarning("Bleeding .DealDamage()");
        //_characterState.GetComponent<HealthComponent>().CmdTryTakeDamage(Random.Range(1, 3), DamageType.Magical, AttackRangeType.MeleeAttack);
    }
}
