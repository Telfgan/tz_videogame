using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Resource, IDamageable, IHealingable
{
    [SerializeField] private Animator _animator;
    [SerializeField] private NetworkAnimator _netAnimator;
    [SerializeField] private Bar bar;

    [SyncVar(hook = nameof(HookEvadeMeleeDamageChanged))] protected float _evadeMeleeDamage;
    [SyncVar(hook = nameof(HookEvadeRangeDamageChanged))] protected float _evadeRangeDamage;
    [SyncVar(hook = nameof(HookEvadeMagDamageChanged))] protected float _resistMagDamage;
    [SyncVar(hook = nameof(HookDefPhysDamageChanged))] protected float _defPhysDamage;
    [SyncVar(hook = nameof(HookDefMagDamageChanged))] protected float _defMagDamage;

    private List<IDamageable> _shields = new List<IDamageable>();
	[SyncVar] private float _sumDamageTaken = 0;
    private Coroutine _dOTDamageAnimJob;
    private float _dOTDamageAnimDuration = 0.1f;
    private float _totalMaxAbsorption = 0;
    private float _blockChance;
    private bool _isDot = false;

    public Bar barCharacter { get => bar; }
    public float BlockChance { get => _blockChance; set => _blockChance = value; }
    //public float SumDamageTaken { get { Debug.Log("Sum dmg " + _sumDamageTaken); return _sumDamageTaken; }} //=> _sumDamageTaken; }
    public float SumDamageTaken { get => _sumDamageTaken; }
    public float EvadeMeleeDamage { get => _evadeMeleeDamage; set => _evadeMeleeDamage = value; }
    public float EvadeRangeDamage { get => _evadeRangeDamage; set => _evadeRangeDamage = value; }
    public float ResistMagDamage { get => _resistMagDamage; set => _resistMagDamage = value; }
    public float DefPhysDamage { get => _defPhysDamage; set => _defPhysDamage = value; }
    public float DefMagDamage { get => _defMagDamage; set => _defMagDamage = value; }
    public float TotalMaxAbsorption { get => _totalMaxAbsorption; set => _totalMaxAbsorption = value; }
    public List<IDamageable> Shields { get => _shields; }

    public event Action Evaded;
    public event Action Block;
    public event Action<float , Skill , string> HealTaked;
    public event Action<Damage, Skill> DamageTaken;
    public event Action Died;
    public event Action<float, float> OnShieldValuesChanged;
    public event Action<float> OnShieldAdd;
    public event Action ShieldDeactivated;

    public event Action<float, DamageType, Skill> ShieldDamageTaken;
    public event Action<Damage, Skill> OnBeforeTakeDamage; //Test

    public delegate void BeforeDamageDelegate(ref Damage damage, Skill skill);
    public event BeforeDamageDelegate OnBeforeDamage;

    public event Action<float, float> EvadeMeleeDamageChanged;
    public event Action<float, float> EvadeRangeDamageChanged;
    public event Action<float, float> EvadeMagDamageChanged;
    public event Action<float, float> DefPhysDamageChanged;
    public event Action<float, float> DefMagDamageChanged;

    public bool IsDot { get => _isDot; set => _isDot = value; }

    public override void Initialize(float health, float hpRegen, float hpRegenDelay, CharacterData data)
    {
        base.Initialize(health, hpRegen, hpRegenDelay, data);

        _defPhysDamage = data.GetAttributeValue(AttributeNames.PhysicResist);
        _defMagDamage = data.GetAttributeValue(AttributeNames.MagicResist);
        _resistMagDamage = data.GetAttributeValue(AttributeNames.MagicEvade);
        _evadeMeleeDamage = data.GetAttributeValue(AttributeNames.MeleeEvade);
        _evadeRangeDamage = data.GetAttributeValue(AttributeNames.RangeEvade);
    }

    public bool TryTakeDamage(ref Damage damage, Skill skill)
    {
        OnBeforeTakeDamage?.Invoke(damage, skill);
        OnBeforeDamage?.Invoke(ref damage, skill); //Test: we transmit incoming damage before it is inflicted by the enem

        if (TryEvade(damage.Type, damage.PhysicAttackType))
        {
            ClientRpcEvade();
            return false;
        }

        if (UnityEngine.Random.Range(0f, 100f) <= _blockChance)
        {
            Block?.Invoke();
            return false;
        }

        Defence(ref damage);

        // Test: If the state has a damage modification, it increases the damage.
        if (skill != null && skill.Hero != null)
        {
            foreach (var state in skill.Hero.CharacterState.CurrentStates)
            {
                if (state is IDamageGivenModifier modifier) damage.Value = modifier.ModifyOutgoingDamage(damage);
            }
        }

        UseShields(ref damage, skill);

        if (damage.Value == 0)
            return true;

        if (!TryUse(damage.Value))
        {
            if (isServer)
            {
                Died?.Invoke();
                ClientRpcDied();
            }
            return true;
        }
        ClientRpcDamage(damage, skill);
        _sumDamageTaken += damage.Value;
        //Debug.Log("Sum damage update " + _sumDamageTaken);
        return true;
    }

    [Command(requiresAuthority = false)]
    public void CmdTryTakeDamage(Damage damage, GameObject skillCanBeNull)
    {
        TryTakeDamage(ref damage, null);
    }

    public void Heal(ref Heal heal, string sourceName, Skill skill = null)
    {
        ClientRpcHealTaked(heal.Value, skill, sourceName);
        Add(heal.Value);
    }

    public void SetEvadeMagic(float value)
    {
        _resistMagDamage = value;
    }

    public void SetEvadeMagicDecrease(float value)
    {
        _resistMagDamage *= 1 - (value / 100);
    }

    public void SetPhysicDef(float value)
    {
        _defPhysDamage = value;
    }

    public void SetMagicDef(float value)
    {
        _defMagDamage = value;
    }

    public void SetEvadeAll(float value)
    {
		//Debug.Log("EVADEBOOST " + value);
		_defPhysDamage += value;
        _defMagDamage += value;
        _resistMagDamage += value;
        _evadeMeleeDamage += value;
        _evadeRangeDamage += value;
    }

    public void SetEvadePhys(float value)
    {
        _evadeMeleeDamage = value;
        _evadeRangeDamage = value;
    }

    public void SetHp(float current, float max)
    {
        CurrentValue = current;
        MaxValue = max;
    }

    #region HookMethods

    protected virtual void HookEvadeMeleeDamageChanged(float oldValue, float newValue)
    {
        EvadeMeleeDamageChanged?.Invoke(oldValue, newValue);
    }

    protected virtual void HookEvadeRangeDamageChanged(float oldValue, float newValue)
    {
        EvadeRangeDamageChanged?.Invoke(oldValue, newValue);
    }

    protected virtual void HookEvadeMagDamageChanged(float oldValue, float newValue)
    {
        EvadeMagDamageChanged?.Invoke(oldValue, newValue);
    }

    protected virtual void HookDefPhysDamageChanged(float oldValue, float newValue)
    {
        DefPhysDamageChanged?.Invoke(oldValue, newValue);
    }

    protected virtual void HookDefMagDamageChanged(float oldValue, float newValue)
    {
        DefMagDamageChanged?.Invoke(oldValue, newValue);
    }

    #endregion

    public bool TryEvade(DamageType damageType, AttackRangeType attackRangeType)
    {
        switch (damageType)
        {
            case DamageType.Magical:

                if (UnityEngine.Random.Range(0, 100) <= _resistMagDamage)
                    return true;
                else
                    return false;

                break;

            case DamageType.Physical:
                switch (attackRangeType)
                {
                    case AttackRangeType.MeleeAttack:

                        if (UnityEngine.Random.Range(0, 100) <= _evadeMeleeDamage)
                            return true;

                        else
                            return false;

                        break;

                    case AttackRangeType.RangeAttack:

                        if (UnityEngine.Random.Range(0, 100) <= _evadeRangeDamage)
                            return true;
                        else
                            return false;

                        break;

                    default:
                        break;
                }
                break;

            case DamageType.Both:
                break;

            default:
                return false;
                break;
        }

        return false;
    }

    protected void UseShields(ref Damage damage, Skill skill)
    {
        for (int i = 0; i < _shields.Count; i++)
        {
            if (_shields[i] != null)
            {
                _shields[i].TryTakeDamage(ref damage, skill);

                if (damage.Value == 0)
                {
                    break;
                }
            }
            _shields.RemoveAt(i);
            i--;
        }
    }

    private void Defence(ref Damage damage)
    {
        if (damage.Type == DamageType.Physical)
        {
            damage.Value *= 1 - (_defPhysDamage / 100.0f);
        }
        else if (damage.Type == DamageType.Magical)
        {
            damage.Value *= 1 - (_defMagDamage / 100.0f);
        }
    }

    public void UpdateShieldValues(float absorbed, float maxAbsorption)
    {
        if (isServer)
            ClientRpcUpdateShieldValues(absorbed, maxAbsorption);
    }

    public void AddShieldValues(float maxAbsorption)
    {
        if (isServer)
            ClientRpcAddShieldValues(maxAbsorption);
    }

    public void ResetShieldValues()
    {
        ShieldDeactivated?.Invoke();
    }

    [ClientRpc]
    public void ClientRpcInvokeShieldDamageTaken(float value, DamageType damageType, Skill skill)
    {
        ShieldDamageTaken?.Invoke(value, damageType, skill);
    }

    [ClientRpc]
    public void ClientRpcUpdateShieldValues(float absorbed, float maxAbsorption)
    {
        OnShieldValuesChanged?.Invoke(absorbed, maxAbsorption);
    }

    [ClientRpc]
    public void ClientRpcAddShieldValues(float maxAbsorption)
    {
        OnShieldAdd?.Invoke(maxAbsorption);
    }

    [ClientRpc]
    private void ClientRpcDamage(Damage damage, Skill skill)
    {
        DamageTaken?.Invoke(damage, skill);
        _animator.SetTrigger(HashAnimPlayer.TakeDamage);
    }


    [ClientRpc]
    private void ClientRpcEvade()
    {
        Evaded?.Invoke();
        _animator.SetTrigger(HashAnimPlayer.Evade);
    }

    [ClientRpc]
    private void ClientRpcHealTaked(float healTaken, Skill skill, string sourceName)
    {
        HealTaked?.Invoke(healTaken, skill, sourceName);
    }

    [ClientRpc]
    private void ClientRpcDied()
    {
        //Died?.Invoke();
        _animator.SetBool(HashAnimPlayer.IsDead, true);
    }

	public void ShowPhantomValue(Damage phantomValue)
	{
        float curDamage = phantomValue.Value;
        if(phantomValue.Type == DamageType.Physical)
        {
            curDamage *= 1 - (_defPhysDamage / 100.0f);
        }
        if(phantomValue.Type == DamageType.Magical)
        {
            curDamage *= 1 - (_defMagDamage / 100.0f);
        }

		PhantomValueShow(curDamage);
	}

    [Command] public void CmdSetBlockChance(float chance) => _blockChance = chance;
    [Command] public void CmdResetBlockChance() => ResetBlockChance();

    public void ResetBlockChance() => _blockChance = 0;

    public void IncreaseRegen(float percentValue)
    {
        _regenerationValue *= percentValue;
    }

    public void DecreaseRegen(float  percentageValue) 
    {
        _regenerationValue /= percentageValue;
    }
}