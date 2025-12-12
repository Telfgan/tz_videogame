using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class NinjaResources : Skill, IPassiveSkill
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

    #region Talent
    private bool _isIceRuneTalent;
    private bool _isHardenedFleshTalent;

    public void EnergyToRestore(bool value, string text)
    {
        _isIceRuneTalent = value;
        AbilityInfoHero.FinalDescription = value ? AbilityInfoHero.Description + $" {text}" : AbilityInfoHero.Description;
    }

    public void HardenedFleshTalent(bool value, string text)
    {
        _isHardenedFleshTalent = value;
        AbilityInfoHero.FinalDescription = value ? AbilityInfoHero.Description + $" {text}" : AbilityInfoHero.Description;
    }
    #endregion

    private void OnEnable()
    {
        Hero.DamageTracker.OnDamageTracked += OnDamageTaken;
        Hero.Health.DamageTaken += HandleDamageTaken;
    }

    private void OnDisable()
    {
        Hero.DamageTracker.OnDamageTracked -= OnDamageTaken;
        Hero.Health.DamageTaken -= HandleDamageTaken;
    }

    private void OnDamageTaken(Damage damage, GameObject attacker)
    {
        if (_isIceRuneTalent && damage.Value > 0  && Hero.TryGetResource(ResourceType.Energy) is Energy energy)
        {
            float energyToRestore = damage.Value * 0.2f;
            energy.Add(energyToRestore);
        }
    }

    private void HandleDamageTaken(Damage damage, Skill skill)
    {
        if (_isHardenedFleshTalent && damage.Type == DamageType.Physical && damage.Value > 0)
        {
            for (int i = 0; i < damage.Value; i++)
            {
                float roll = UnityEngine.Random.Range(0f, 1f);
                if (roll <= 0.01f)
                {
                    Hero.CharacterState.CmdAddState(States.HardenedFlesh, 9f, 0, Hero.gameObject, this.Name);
                    break;
                }
            }
        }
    }
}
