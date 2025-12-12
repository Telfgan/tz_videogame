using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class LightShield : AbstractCharacterState, IDamageable
{
    private BladeMailPriestTalent _bladeMailPriestTalent;
    private GameObject _lightShield;

    private float _damageAbsorbed;
    private float _maxAbsorption;
    private float _duration;
    private string _skillName;

    private bool _isBMTalentActive = false;

    public event Action<Damage, Skill> DamageTaken;

    public override States State => States.LightShield;
    public override StateType Type => StateType.Magic;
    public override List<StatusEffect> Effects => new List<StatusEffect>();
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;

    public Transform transform => throw new NotImplementedException();
    public GameObject gameObject => throw new NotImplementedException();

    public override void EnterState(CharacterState character, float durationToExit, float maxDamageAbsorbed, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _duration = durationToExit;
        _damageAbsorbed = 0;
        _maxAbsorption = maxDamageAbsorbed;
        _skillName = skillName;

        if (_characterState.StateEffects.LightShield != null)
        {
            _lightShield = _characterState.StateEffects.LightShield;
            _lightShield.SetActive(true);
        }

        SearchTalent();

        Debug.Log("Shield HP - " + _maxAbsorption);
        //DamageTaken += DamageEnemiesInRadius;

        if (_characterState.TryGetComponent<Health>(out var health))
        {
            health.AddShieldValues(_maxAbsorption);
            health.UpdateShieldValues(_damageAbsorbed, _maxAbsorption);
        }
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;

        if (_duration <= 0 || _damageAbsorbed >= _maxAbsorption)
        {
            ExitState();
        }
    }

    public override void ExitState()
    {
        Debug.Log("LightShield state exited.");
        //DamageTaken -= DamageEnemiesInRadius;

        if (_characterState.TryGetComponent<Health>(out var health))
        {
            health.ResetShieldValues();
        }

        _characterState.RemoveState(this);

        if (_lightShield != null)
            _lightShield.SetActive(false);
    }

    public override bool Stack(float time)
    {
        _duration = time;
        _damageAbsorbed = 0;

        if (_characterState.TryGetComponent<Health>(out var health))
        {
            health.AddShieldValues(_maxAbsorption);
            health.UpdateShieldValues(_damageAbsorbed, _maxAbsorption);
        }

        return false;
    }

    public bool TryTakeDamage(ref Damage damage, Skill skill)
    {
        float damageToAbsorb = Mathf.Min(_maxAbsorption - _damageAbsorbed, damage.Value);
        _damageAbsorbed += damageToAbsorb;
        damage.Value -= damageToAbsorb;

        _characterState.GetComponent<Character>().DamageTracker.AddDamage(damage, _characterState.gameObject, true);

        var tempDamage = new Damage
        {
            Form = damage.Form,
            PhysicAttackType = damage.PhysicAttackType,
            School = damage.School,
            Type = damage.Type,
            Value = damageToAbsorb,
        };

        DamageTaken?.Invoke(tempDamage, skill);

        if (_characterState.TryGetComponent<Health>(out var health))
        {
            health.UpdateShieldValues(_damageAbsorbed, _maxAbsorption);
        }

        if (_damageAbsorbed >= _maxAbsorption)
        {
            ExitState();
            return true;
        }

        return damage.Value == 0;
    }

    //private void DamageEnemiesInRadius(Damage damage, Skill skill)
    //{
    //    foreach (var obj in NetworkServer.spawned.Values)
    //    {
    //        if (!obj.TryGetComponent(out Character enemy)) continue;

    //        var distance = Vector3.Distance(_characterState.transform.position, enemy.transform.position);
    //        if (distance > 10f || distance <= 0.25f) continue;

    //        var tempDamage = new Damage
    //        {
    //            Form = damage.Form,
    //            PhysicAttackType = damage.PhysicAttackType,
    //            School = damage.School,
    //            Type = damage.Type,
    //            Value = damage.Value * 0.2f
    //        };

    //        enemy.Health.TryTakeDamage(ref tempDamage, null);
    //        enemy.DamageTracker.AddDamage(tempDamage, null);
    //    }
    //}

    public void ShowPhantomValue(Damage phantomValue)
    {
        throw new NotImplementedException();
    }

    private void SearchTalent()
    {
        foreach (var talent in _characterState.Character.Abilities.TalesntSystem.ActiveTalents)
        {
            if (talent is BladeMailPriestTalent bladeMailPriestTalent)
            {
                if (_bladeMailPriestTalent == null)
                {
                    _bladeMailPriestTalent = bladeMailPriestTalent;
                    _isBMTalentActive = _bladeMailPriestTalent.Data.IsOpen;
                }
            }
        }
    }
}