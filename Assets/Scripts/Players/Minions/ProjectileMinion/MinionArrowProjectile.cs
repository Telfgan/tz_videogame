using Mirror;
using UnityEngine;

public class MinionArrowProjectile : MinionProjectile
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _lifeTime = 5f;
    [SerializeField] private float physicDamage;
    [SerializeField] private float minDamage;
    [SerializeField] private float maxDamage;
    [SerializeField] private float duration;

    private void Start()
    {
        physicDamage = Random.Range(minDamage, maxDamage + 1);
    }

    public void StartFly(Vector3 direction)
    {
        if (_rb != null)
        {
            _rb.linearVelocity = direction * _speed;
        }

        Destroy(gameObject, _lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != _dad.gameObject)
        {
            if (((1 << other.gameObject.layer) & _skill.TargetsLayers.value) != 0)
            {
                ApplyEnemy(other);
                Destroy(gameObject);
            }
        }
    }

    //private void TargetApply(Collider other)
    //{
    //    //if (other.TryGetComponent<IDamageable>(out IDamageable target))
    //    //{
    //    //    //if (other.TryGetComponent<UserNetworkSettings>(out UserNetworkSettings userNetworkSettings))
    //    //    //{
    //    //    //    if (userNetworkSettings.TeamIndex != _dad.NetworkSettings.TeamIndex)
    //    //    //    {
    //    //    //        ApplyEnemy(other, target);
    //    //    //    }
    //    //    //}

    //    //    if (other.gameObject != _dad.gameObject && ((1 << other.gameObject.layer) & _skill.TargetsLayers.value) != 0) ApplyEnemy(other);
    //    //}
    //}

    #region ApplyEnemy
    private void ApplyEnemy(Collider collider)
    {
        ApplyDamage(physicDamage, _skill.DamageType, collider.gameObject);
    }
    #endregion

    private void ApplyDamage(float damage, DamageType damageType, GameObject target)
    {
        Damage _damage = new Damage
        {
            Value = damage,
            Type = damageType,
        };
        _skill.CmdApplyDamage(_damage, target);
    }
}
