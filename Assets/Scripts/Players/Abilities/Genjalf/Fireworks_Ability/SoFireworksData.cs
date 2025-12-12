using UnityEngine;

namespace Players.Abilities.Genjalf.Fireworks_Ability
{
    [CreateAssetMenu(menuName = "Create Fireworks Data", fileName = "Fireworks Data")]
    public class SoFireworksData : ScriptableObject
    {
        [SerializeField] private float _scaleX = 10f; //Scale в 10ед, это длина каста в 5 квадратов.
        [SerializeField] private float _scaleY = 2f; //Scale в 2ед, это ширина каста в 1 квадрат.
        [SerializeField] private float _timeToDie = 1.6f; //Время до окончания заклинания
        [SerializeField] private float _damageFireworksMin = 3f; //Минимальный урон
        [SerializeField] private float _damageFireworksMax = 6f; //Максимальынй урон
        [SerializeField] private float _manaCost = 3f; //Затраты маны

        [Header("Percentage of Damage")] 
        [SerializeField] private float _percentageTargetOne = 1f;       // 100% урона в первую цель
        [SerializeField] private float _percentageTargetTwo = 0.7f;    //70% урона во вторую цель
        [SerializeField] private float _percentageTargetThree = 0.3f; //30% урона в третью цель

        public float ScaleX
        {
            get => _scaleX;
            set => _scaleX = value;
        }

        public float ScaleY
        {
            get => _scaleY;
            set => _scaleY = value;
        }

        public float TimeToDie => _timeToDie;

        public float DamageFireworksMin => _damageFireworksMin;

        public float DamageFireworksMax => _damageFireworksMax;

        public float PercentageTargetOne => _percentageTargetOne;

        public float PercentageTargetTwo => _percentageTargetTwo;

        public float PercentageTargetThree => _percentageTargetThree;

        public float ManaCost => _manaCost;
    }
}