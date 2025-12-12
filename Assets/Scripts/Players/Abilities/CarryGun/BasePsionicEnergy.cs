using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BasePsionicEnergy : Resource, IDamageable
{
    [SerializeField] private Character _player;
    [SerializeField] private AttackingPsionicEnergy _attackingPsionicEnergy;
    [SerializeField] private Slider basePsionicsSlider;
    [SerializeField] private PsionicEnergySkill psionicEnergySkill;

    private const float BasePsionicaThreshold = 30f;
    private const float BaseSliderFillPercent = 0.3f;
    private const float RemainingSliderFillPercent = 0.7f;
    private const float DamageToPsiConversionRate = 0.2f;

    private float _psionicaDecayTime;
    private bool _isInternalPsiEnergy = false;
    private Coroutine _energyDecayCoroutine;

    public bool IsAttackingPsiEnergyActive => _attackingPsionicEnergy.IsAttackingPsiEnergy;

    public event Action<Damage, Skill> DamageTaken;
    public event Action<float> OnEnergyChanged;

    public PsionicEnergySkill PsionicEnergySkill { get => psionicEnergySkill; set => psionicEnergySkill = value; }
    public float PsionicaDecayTime { get => _psionicaDecayTime; set => _psionicaDecayTime = value; }

    private void Start()
    {
        _psionicaDecayTime = psionicEnergySkill.CooldownTime;
        if (_player != null)
        {
            MaxValue = _player.Data.GetAttributeValue(AttributeNames.Health);
            _player.Health.Shields.Add(this);
        }
    }

    private void Update()
    {
        UpdatePsionicaBar();
    }

    private void OnEnable()
    {
        if (_player.DamageTracker != null) _player.DamageTracker.OnDamageTracked += OnDamageDealt;
        if (_player.Health != null) _player.Health.OnBeforeDamage += psionicEnergySkill.HandleIncomingDamage;
    }

    private void OnDestroy()
    {
        if (_player != null && _player.DamageTracker != null) _player.DamageTracker.OnDamageTracked -= OnDamageDealt;
        if (_player.Health != null) _player.Health.OnBeforeDamage -= psionicEnergySkill.HandleIncomingDamage;
    }

    private void OnDamageDealt(Damage damage, GameObject target)
    {
        if (damage.Type == DamageType.Physical)
        {
            float energyGain = damage.Value * DamageToPsiConversionRate;
            Add(energyGain);
            CurrentValue = Mathf.Min(CurrentValue, MaxValue);
            RpcCoolDownPsionicEnegry();

            RpcOnEnergyChanged(CurrentValue);

            bool wasInternalEnergy = _isInternalPsiEnergy;
            _isInternalPsiEnergy = CurrentValue > 0;

            if (wasInternalEnergy != _isInternalPsiEnergy)
            {
                RpcInternalPsiEnergyChanged(_isInternalPsiEnergy);
            }

            if (_energyDecayCoroutine != null)
            {
                StopCoroutine(_energyDecayCoroutine);
            }
            _energyDecayCoroutine = StartCoroutine(EnergyDecayCoroutine());

            UpdatePsionicaBar();
        }
    }

    #region Test
    public void AddAndResetDecayCoolDownPsionicEnegry(float value)
    {
        Add(value);
        CurrentValue = Mathf.Min(CurrentValue, MaxValue);

        if (isServer)
        {
            RpcOnEnergyChanged(CurrentValue);

            bool wasInternalEnergy = _isInternalPsiEnergy;
            _isInternalPsiEnergy = CurrentValue > 0;

            if (wasInternalEnergy != _isInternalPsiEnergy)
            {
                RpcInternalPsiEnergyChanged(_isInternalPsiEnergy);
            }

            if (_energyDecayCoroutine != null)
            {
                StopCoroutine(_energyDecayCoroutine);
            }
            _energyDecayCoroutine = StartCoroutine(EnergyDecayCoroutine());
        }

        UpdatePsionicaBar();
    }
    #endregion

    public void AddAndResetDecay(float value)
    {
        Add(value);
        CurrentValue = Mathf.Min(CurrentValue, MaxValue);
        RpcCoolDownPsionicEnegry();

        if (isServer)
        {
            RpcOnEnergyChanged(CurrentValue);

            bool wasInternalEnergy = _isInternalPsiEnergy;
            _isInternalPsiEnergy = CurrentValue > 0;

            if (wasInternalEnergy != _isInternalPsiEnergy)
            {
                RpcInternalPsiEnergyChanged(_isInternalPsiEnergy);
            }

            if (_energyDecayCoroutine != null)
            {
                StopCoroutine(_energyDecayCoroutine);
            }
            _energyDecayCoroutine = StartCoroutine(EnergyDecayCoroutine());
        }

        UpdatePsionicaBar();
    }

    [ClientRpc] public void RpcCoolDownPsionicEnegry() => CoolDownPsionicEnegry();

    public void CoolDownPsionicEnegry() => psionicEnergySkill.IncreaseSetCooldownPassive(_psionicaDecayTime);

    public void UsePsiEnergy(float value)
    {
        TryUse(value);
        RpcOnEnergyChanged(CurrentValue);
        UpdatePsionicaBar();
    }

    private void UpdatePsionicaBar()
    {
        float normalizedValue = 0f;

        if (CurrentValue <= BasePsionicaThreshold)
        {
            normalizedValue = (CurrentValue / BasePsionicaThreshold) * BaseSliderFillPercent;
        }
        else
        {
            float remainingValue = (CurrentValue - BasePsionicaThreshold) / (MaxValue - BasePsionicaThreshold);
            normalizedValue = BaseSliderFillPercent + (remainingValue * RemainingSliderFillPercent);
        }

        basePsionicsSlider.value = normalizedValue;
    }

    private IEnumerator EnergyDecayCoroutine()
    {
        yield return new WaitForSeconds(_psionicaDecayTime);
        CurrentValue = 0;
        RpcOnEnergyChanged(CurrentValue);
        _isInternalPsiEnergy = false;
        UpdatePsionicaBar();
        RpcInternalPsiEnergyChanged(_isInternalPsiEnergy);
    }

    public bool TryTakeDamage(ref Damage damage, Skill skill)
    {
        if (damage.Value == 0) return true;

        if (CurrentValue > 0)
        {
            float absorbingDamage = Mathf.Min(CurrentValue, damage.Value);
            damage.Value -= absorbingDamage * 0.5f;
            UsePsiEnergy(absorbingDamage);

            _isInternalPsiEnergy = CurrentValue > 0;
            RpcInternalPsiEnergyChanged(_isInternalPsiEnergy);
            UpdatePsionicaBar();

            return true;
        }

        return false;
    }

    public void ConvertToAttackingEnergy(float amount)
    {
        float transferAmount = Mathf.Min(CurrentValue, amount);
        if (transferAmount > 0)
        {
            UsePsiEnergy(transferAmount);
            _attackingPsionicEnergy.ReceiveAttackingEnergy(transferAmount);
        }
    }

    [ClientRpc]
    private void RpcInternalPsiEnergyChanged(bool value)
    {
        _isInternalPsiEnergy = value;
    }

    [ClientRpc]
    private void RpcOnEnergyChanged(float value)
    {
        CurrentValue = value;
        OnEnergyChanged?.Invoke(value);
    }

    public void ShowPhantomValue(Damage phantomValue)
    {
        throw new NotImplementedException();
    }

    public override void Add(float value)
    {
        if (psionicEnergySkill == null || !psionicEnergySkill.IsPsiEnergyActive) return;

        base.Add(value);
    }
}