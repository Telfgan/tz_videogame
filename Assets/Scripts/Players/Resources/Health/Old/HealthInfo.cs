using UnityEngine;

[CreateAssetMenu(menuName = "HealthInfo", fileName = "HealthInfo")]
public class HealthInfo : ScriptableObject
{
    [SerializeField] private float _defPhysDamage;
    [SerializeField] private float _defMagDamage;

    [SerializeField] private float _evadeMeleeDamage;
    [SerializeField] private float _evadeRangeDamage;
    [SerializeField] private float _evadeMagDamage;

    public float DefaultPhysicsDamage => _defPhysDamage;
    public float DefaultMagicDamage => _defMagDamage;
    public float EvadeMeleeDamage => _evadeMeleeDamage;
    public float EvadeRangeDamage => _evadeRangeDamage;
    public float EvadeMagicDamage => _evadeMagDamage;
}
