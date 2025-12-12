using UnityEngine;

namespace Players.Abilities.Genjalf.Shield_Ability
{
    [CreateAssetMenu(menuName = "Create Shield Data", fileName = "Shield Data")]
    public class SoShieldData : ScriptableObject
    {
        [SerializeField] private int _shieldCharges = 3;
        [SerializeField] private float _cooldownCharge = 17f;
        [SerializeField] private float _manaCost = 20f;
        [SerializeField] private float _absorptionAmount = 30f;
        [SerializeField] private float _durationShield = 2f;


        public float ManaCost => _manaCost;

        public float DurationShield => _durationShield;

        public int ShieldCharges
        {
            get => _shieldCharges;
            set => _shieldCharges = value;
        }

        public float CooldownCharge => _cooldownCharge;

        public float AbsorptionAmount
        {
            get => _absorptionAmount;
            set => _absorptionAmount = value;
        }
    }
}