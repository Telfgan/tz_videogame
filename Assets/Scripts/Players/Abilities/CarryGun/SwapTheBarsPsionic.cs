using UnityEngine;

public class SwapTheBarsPsionic : MonoBehaviour
{
    [SerializeField] private BasePsionicEnergy basePsionicEnergy;
    [SerializeField] private AttackingPsionicEnergy attackingPsionicEnergy;
 
    [SerializeField] private RectTransform baseEnergySliderTransform;
    [SerializeField] private RectTransform attackingEnergySliderTransform;

    private void Start()
    {
        basePsionicEnergy.OnEnergyChanged += OnEnergyChanged;
        attackingPsionicEnergy.OnEnergyChanged += OnEnergyChanged;

        CompareAndSwap();
    }

    private void OnDestroy()
    {
        basePsionicEnergy.OnEnergyChanged -= OnEnergyChanged;
        attackingPsionicEnergy.OnEnergyChanged -= OnEnergyChanged;
    }

    private void OnEnergyChanged(float value)
    {
        CompareAndSwap();
    }

    private void CompareAndSwap()
    {
        float baseEnergy = basePsionicEnergy.CurrentValue;
        float attackingEnergy = attackingPsionicEnergy.CurrentValue;

        if (attackingEnergy > baseEnergy) baseEnergySliderTransform.SetSiblingIndex(attackingEnergySliderTransform.GetSiblingIndex() + 1);
        else attackingEnergySliderTransform.SetSiblingIndex(baseEnergySliderTransform.GetSiblingIndex() + 1);
    }
}
