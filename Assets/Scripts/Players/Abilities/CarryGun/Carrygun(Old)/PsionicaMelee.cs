using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PsionicaMelee : Resource
{
    public Slider basePsionicsSlider;
    public float AbsorptionChance = 0.1f;
    private HeroComponent _hero;

    private const float BasePsionicaThreshold = 30f;
    private const float BaseSliderFillPercent = 0.3f;
    private const float RemainingSliderFillPercent = 0.7f;

    private float timer = 0f;
    private bool isTimerActive = false;
    private const float PsionicaDecayTime = 12f;
    private const float MaxEnergyThreshold = 100f;

    public float Psionica { get => CurrentValue; set => CurrentValue = value; }

    private void Start()
    {
        _hero = GetComponent<HeroComponent>();

        if (_hero != null)
        {
            MaxValue = _hero.Data.GetAttributeValue(AttributeNames.Health);

            if (_hero.DamageTracker != null)
            {
                _hero.DamageTracker.OnDamageTracked += OnDamageDealt;
            }
        }
    }

    private void OnDestroy()
    {
        if (_hero != null && _hero.DamageTracker != null)
        {
            _hero.DamageTracker.OnDamageTracked -= OnDamageDealt;
        }
    }

    private void Update()
    {
        UpdatePsionicaBar();

        if (isTimerActive)
        {
            timer += Time.deltaTime;
            if (timer >= PsionicaDecayTime)
            {
                ResetPsionica();
            }
        }
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

    private void OnDamageDealt(Damage damage, GameObject target)
    {
        if (damage.Type == DamageType.Physical)
        {
            Add(damage.Value);
            CurrentValue = Mathf.Min(CurrentValue, MaxValue);

            if (CurrentValue >= MaxEnergyThreshold)
            {
                isTimerActive = true;
                timer = 0f;
            }

            UpdatePsionicaBar();
        }
    }

    public void UsePsionica(float value)
    {
        TryUse(value);
        UpdatePsionicaBar();
    }

    public void PsionicaAbsorption(ref float modifiedDamage)
    {
        if (CurrentValue > 0)
        {
            float absorptionAmount = Mathf.Min(CurrentValue, modifiedDamage);
            UsePsionica(absorptionAmount);
            modifiedDamage = (modifiedDamage - absorptionAmount) + absorptionAmount - ((modifiedDamage - absorptionAmount) + absorptionAmount) * AbsorptionChance;
        }
    }

    private void ResetPsionica()
    {
        CurrentValue = 0;
        isTimerActive = false;
        timer = 0f;
        UpdatePsionicaBar();
    }

    #region Old
    public void MakePsionica(float damageValue)
    {

        Psionica += damageValue;
        //float health = GetComponent<HealthComponent>()._currentHealth;
        //Psionica = Mathf.Min(Psionica, health);
    }
    #endregion
}
