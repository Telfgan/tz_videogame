using UnityEngine;

public class StandartAutoAttack : AutoAttackSkill
{
    [SerializeField] private float _damage;
    [SerializeField] private float _animSpeed = 1;

    protected override int AnimTriggerCastDelay => 0;

    protected override int AnimTriggerAutoAttack => Animator.StringToHash("AtackStandartAutoAttack");

    public void AnimStandartAutoAttackCast()
    {
        AnimCastAction();
    }

    public void AnimStandartAutoAttackEnded()
    {
        AnimCastEnded();
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        throw new System.NotImplementedException();
    }

    protected override void CastAction()
    {
        if (GetTargetCharacter() == null)
            return;

        Buff.AttackSpeed.IncreasePercentage(_animSpeed);

        Damage damage = new Damage
        {
            Value = Buff.Damage.GetBuffedValue(_damage),
            Type = DamageType,
            School = School,
            Form = AbilityForm,
            PhysicAttackType = AttackRangeType,

        };
        CmdApplyDamage(damage, GetTargetCharacter().Transform.gameObject);
    }
}
