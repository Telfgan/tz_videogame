using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunMagicPassiveSkill : Skill, IPassiveSkill
{
    #region Skill
    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => 0;
    protected override bool IsCanCast => false;
    protected override IEnumerator CastJob() => null;
    protected override void ClearData() { }
    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved) => null;
    public override void LoadTargetData(TargetInfo targetInfo) => throw new NotImplementedException();
    #endregion

    [SerializeField] private Character playerCharacter;

    #region Talent
    private bool _isDamageDarkLightStun;
    private bool _isDamageDarkHealLightAddHealth;
    private bool _isFillingDestruction;

    public bool IsFillingDestruction { get => _isFillingDestruction; set => _isFillingDestruction = value; }

    public void DamageDarkLightStun(bool value, string text)
    {
        _isDamageDarkLightStun = value;
        AbilityInfoHero.FinalDescription = value ? AbilityInfoHero.Description + $" {text}" : AbilityInfoHero.Description;
    }

    public void DamageDarkHealLightAddHealth(bool value, string text)
    {
        _isDamageDarkHealLightAddHealth = value;
        AbilityInfoHero.FinalDescription = value ? AbilityInfoHero.Description + $" {text}" : AbilityInfoHero.Description;
    }
    public void FillingDestruction(bool value) => _isFillingDestruction = value;
    #endregion

    private void OnEnable()
    {
        if (playerCharacter?.DamageTracker != null)
        {
            playerCharacter.DamageTracker.OnDamageTracked += HandleDamageDealt;
            playerCharacter.DamageTracker.OnHealTracked += HandleHealDone;
        }
    }
    private void OnDisable()
    {
        if (playerCharacter?.DamageTracker != null)
        {
            playerCharacter.DamageTracker.OnDamageTracked -= HandleDamageDealt;
            playerCharacter.DamageTracker.OnHealTracked -= HandleHealDone;
        }
    }

    private void HandleDamageDealt(Damage damage, GameObject targetObject)
    {
        if (_isDamageDarkLightStun && (damage.School == Schools.Light || damage.School == Schools.Dark))
        {
            if (targetObject.TryGetComponent<Character>(out var target))
            {
                float chance = UnityEngine.Random.Range(0f, 100f);
                if (chance <= 30f)
                {
                    float stunDuration = damage.Value * 0.1f;

                    target.CharacterState.AddState(States.Stun, stunDuration, 0, playerCharacter.gameObject, nameof(StunMagicPassiveSkill));
                }
            }
        }

        if (_isDamageDarkHealLightAddHealth && damage.School == Schools.Dark)
        {
            float healAmount = damage.Value * 0.1f;

            var extraHeal = new Heal
            {
                Value = healAmount,
                DamageableSkill = this
            };

            ApplyHeal(extraHeal, Hero.gameObject, this, nameof(StunMagicPassiveSkill));
        }
    }

    private void HandleHealDone(Heal heal)
    {
        if (!_isDamageDarkHealLightAddHealth) return;
        if (heal.DamageableSkill == null) return;
        if (heal.DamageableSkill.School != Schools.Light) return;

        float healAmount = heal.Value * 0.1f;

        var extraHeal = new Heal
        {
            Value = healAmount,
            DamageableSkill = this
        };

        CmdApplyHeal(extraHeal, Hero.gameObject, this, nameof(StunMagicPassiveSkill));
    }
}
