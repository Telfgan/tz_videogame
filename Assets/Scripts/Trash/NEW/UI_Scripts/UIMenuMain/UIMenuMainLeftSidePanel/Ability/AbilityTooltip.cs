using UnityEngine;

public class AbilityTooltip : MonoBehaviour
{
    [SerializeField] private TMProLocalizer _title;
    [SerializeField] private TMProLocalizer _description;
    [SerializeField] private TMProLocalizer _manaCost;
    [SerializeField] private TMProLocalizer _energyCost;
    [SerializeField] private TMProLocalizer _runeCost;
    [SerializeField] private TMProLocalizer _cooldownValue;
    [SerializeField] private TMProLocalizer _castTime;
    [SerializeField] private TMProLocalizer _mainValue;
    [SerializeField] private TMProLocalizer _bonusValue;

    public void Fill(string title, string description,
        float cooldownValue, float castTime)
    {
        _title.Localize(title);
        _description.Localize(description);
        _cooldownValue.Localize(cooldownValue);
        _castTime.Localize(castTime);
       // _mainValue.Localize(mainValue);
        //_bonusValue.Localize(bonusValue);
        
       /* _energyCost.gameObject.SetActive(staminaType == StaminaType.Runes);
        _manaCost.gameObject.SetActive(staminaType == StaminaType.Runes);
        _runeCost.gameObject.SetActive(staminaType == StaminaType.Rune);
        
            switch (staminaType)
            {
                case StaminaType.Runes:
                    _manaCost.Localize(staminaType, manaCost);
                    break;
                case StaminaType.Runes:
                    _energyCost.Localize(staminaType, manaCost);
                    break;
                case StaminaType.Rune:
                    _runeCost.Localize(staminaType, manaCost);
                    break;
            }
            */
    }

    public void ChangePosition(Vector2 position)
    {
        transform.position = position;
    }
}
