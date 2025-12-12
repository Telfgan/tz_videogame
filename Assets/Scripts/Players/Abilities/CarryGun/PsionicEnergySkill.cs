using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PsionicEnergySkill : Skill, IPassiveSkill
{
    #region Skill
    protected override int AnimTriggerCastDelay => throw new NotImplementedException();
    protected override int AnimTriggerCast => throw new NotImplementedException();
    public override void LoadTargetData(TargetInfo targetInfo) => throw new NotImplementedException();

    protected override IEnumerator CastJob()
    {
        yield return null;
    }

    protected override void ClearData() => throw new NotImplementedException();
    protected override IEnumerator PrepareJob(Action<TargetInfo> targetDataSavedCallback) => throw new NotImplementedException();
    #endregion

    [SerializeField] private BasePsionicEnergy basePsionicEnergy;
    [SerializeField] private float modifier = 1f;

    #region Talent
    private bool _isPsiEnergyActive = false;
    private bool _isDischargingPsiTalent = false;

    public bool IsDischargingPsiTalent { get => _isDischargingPsiTalent; set => _isDischargingPsiTalent = value; }
    public bool IsPsiEnergyActive { get => _isPsiEnergyActive; set => _isPsiEnergyActive = value;}

    public void DischargingPsiTalen(bool value) => _isDischargingPsiTalent = value;
    public void PsiEnergyActive(bool value) => _isPsiEnergyActive = value;
    #endregion

    public void HandleIncomingDamage(ref Damage damage, Skill skill)
    {
        if (!_isPsiEnergyActive) return;
        if (damage.Value <= 0 || basePsionicEnergy.CurrentValue <= 0) return;

        float absorptionAmount = Mathf.Min(basePsionicEnergy.CurrentValue, damage.Value);
        basePsionicEnergy.UsePsiEnergy(absorptionAmount);

        float reduced = absorptionAmount * modifier;
        damage.Value -= reduced;
        damage.Value = Mathf.Max(damage.Value, 0f);

        if (_isDischargingPsiTalent && skill?.Hero != null)
        {
            Damage retaliationDamage = new Damage
            {
                Value = absorptionAmount,
                Type = DamageType.Magical,
                School = Schools.Air,
                Form = AbilityForm.Magic,
            };

            ApplyDamage(retaliationDamage, skill.Hero.gameObject);
        }
    }
}
