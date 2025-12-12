using Mirror;
using System.Linq.Expressions;
using UnityEngine;

public class Punch_Scorpion : AutoAttackSkill
{
    [Header("Ability settings")]
    [SerializeField] private Character _playerLinks;
    [SerializeField] private PassiveCombo_Scorpion _comboCounter;
    //[SerializeField] private float _damageValue = 9f;

    private Character _lastTarget = null;

    protected override int AnimTriggerCastDelay => 0;

    protected override int AnimTriggerAutoAttack => throw new System.NotImplementedException();

    protected override void CastAction()
    {
        if (_lastTarget != null && _lastTarget != GetTargetCharacter()) //�����
        {
            _comboCounter.ResetCounter();
        }
        Debug.Log(transform.position);
        Debug.Log(GetTargetCharacter().transform.position);

        //Vector3 closestEnemyPoint = _target.gameObject.GetComponent<CircleCollider2D>().ClosestPoint(transform.position);
        //Vector3 closestMyPoint = transform.parent.parent.GetComponent<CircleCollider2D>().ClosestPoint(_target.transform.position);


        if (Vector2.Distance(LastTargetPosition, GetTargetCharacter().transform.position) <= 2f)
        {
            Debug.Log("����������� ����������");

            Damage damage = new Damage   
            {
                Value = Buff.Damage.GetBuffedValue(_damageValue),
                Type = DamageType,
            };

            CmdAttack(damage, GetTargetCharacter().gameObject);
        }
        else Debug.LogWarning("������� ������");
        _lastTarget = GetTargetCharacter();

    }
    private void AttackPassed(Character target)
    {
        Debug.LogWarning("Punch_Scorppion .AttackPassed - �����");

        _comboCounter.AddSkill(target, this);
    }
    private void AttackMissed()
    {
        Debug.LogWarning("Punch_Scorppion .AttackMissed -������");

        _comboCounter.ResetCounter();
    }

    [Command]
    private void CmdAttack(Damage damage, GameObject hp)
    {
        if (_tempTargetForDamage != hp.transform)
        {
            _tempTargetForDamage = hp.transform;
            _tempForDamage = hp.GetComponent<IDamageable>();
        }

        bool result = _tempForDamage.TryTakeDamage(ref damage, this);
       //RpcSelfNotifyHitResult(result, _tempTargetForDamage);

    }

    [TargetRpc]
    private void RpcSelfNotifyHitResult(bool state,Character target)
    {
        if (state)
        {
            AttackPassed(target);
        }
        else
        {
            AttackMissed();
        }
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        throw new System.NotImplementedException();
    }
}
