using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AbilityNameBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _descriptionWithNumbers;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private TextMeshProUGUI _textState;

    public static readonly string ColorState= "<color=#FFFF00>"; // test: state color
    public static readonly string ColorOpen = "<color=#53935E>";
    public static readonly string ColorEnd = "</color>";

    public void Show(Skill skill)
    {
        _name.text = skill.Name;
        _text.text = $"{skill.Description}";
        if (!string.IsNullOrEmpty(skill.State)) _text.text += $"\n'{ColorState}{skill.State}{ColorEnd}' - {skill.DescriptionState}";
        _descriptionWithNumbers.text = "";

        if (!(skill is ICounterSkill))
        {
            if (skill.SkillEnergyCosts.Count > 0)
            {
                _descriptionWithNumbers.text = $"Затрата: {ColorOpen}{skill.SkillEnergyCosts[0].resourceCost} ед. маны{ColorEnd}";

                if (skill.AdditionalSkillEnergyCosts.Count > 0) _descriptionWithNumbers.text += $"{ColorOpen} + {skill.AdditionalSkillEnergyCosts[0].resourceCost} ед. маны{ColorEnd}";
            }
            else _descriptionWithNumbers.text = $"Затрата: {ColorOpen}0 ед. маны{ColorEnd}";

            if (skill.ManaCostPerTick.Count > 0)
                _descriptionWithNumbers.text += $" + {ColorOpen}{skill.ManaCostPerTick[0].resourceCost} ед. маны/{skill.ManaCostRate} сек{ColorEnd}";

            switch (skill.AbilityForm)
            {
                case AbilityForm.Spell:
                    _descriptionWithNumbers.text += $" заклинание {GetSchoolName(skill)}";
                    break;

                case AbilityForm.Magic:
                    _descriptionWithNumbers.text += $" магия {GetSchoolName(skill)}";
                    break;

                case AbilityForm.Physical:
                    break;

                default:
                    break;
            }

            if (skill.Damage > 0)
            {
                int damage = Mathf.RoundToInt(skill.Damage);
                _descriptionWithNumbers.text += $"\nУрон: {ColorOpen}{damage}{ColorEnd} ед. {GetShoolNameForDamage(skill)}";
            }

            //WriteTypeDamage(skill);
            //WriteTypeAbityForm(skill);

            if (skill.CastDeley > 0)
                _descriptionWithNumbers.text += $"\nПодготовка: {ColorOpen}{skill.CastDeley} сек{ColorEnd}";

            if (skill.CastStreamDuration > 0)
                _descriptionWithNumbers.text += $"\nВыполнение: {ColorOpen}{skill.CastStreamDuration} сек{ColorEnd}";

            if (skill.CooldownTime > 0)
                _descriptionWithNumbers.text += $"\nПерезарядка: {ColorOpen}{skill.CooldownTime} сек{ColorEnd}";

            if (skill.ChargeCooldown > 0)
                _descriptionWithNumbers.text += $"\nКол-во Зарядов: {ColorOpen}{skill.MaxChargers}/{skill.ChargeCooldown} сек{ColorEnd}";

            //if (skill.AdditionalDescription != string.Empty)
            //    _descriptionWithNumbers.text += $"\n{skill.AdditionalDescription}";

        }

        else _descriptionWithNumbers.text += $"\n{skill.CounterSkill}: {ColorOpen}{skill.CurrentCounter}/{skill.MaxCounter}{ColorEnd}";
    }

    private string GetShoolNameForDamage(Skill skill)
    {
        switch (skill.School)
        {
            case Schools.Light:
                return "светом";
                break;

            case Schools.Dark:
                return "тьмой";
                break;

            case Schools.Fire:
                return "огнем";
                break;

            case Schools.Water:
                return "водой";
                break;

            case Schools.Air:
                return "воздухом";
                break;

            case Schools.Earth:
                return "землей";
                break;

            case Schools.Physical:
                return "";
                break;

            case Schools.Discipline:
                return "";
                break;

            case Schools.None:
                return "";
                break;

            default:
                return "";
                break;
        }
    }

    private void WriteTypeAbityForm(Skill skill)
    {
        _descriptionWithNumbers.text += "\nФорма способности:";

        switch (skill.AbilityForm)
        {
            case AbilityForm.Magic:
                _descriptionWithNumbers.text += " магия";
                break;
            case AbilityForm.Physical:
                _descriptionWithNumbers.text += " физика";
                break;
            case AbilityForm.Spell:
                _descriptionWithNumbers.text += " заклинания";
                break;
            default:
                break;
        }
    }

    private void WriteTypeDamage(Skill skill)
    {
        switch (skill.DamageType)
        {
            case DamageType.Magical:
                _descriptionWithNumbers.text += " \nмагия";
                switch (skill.School)
                {
                    case Schools.Light:
                        _descriptionWithNumbers.text += " света";
                        break;
                    case Schools.Dark:
                        _descriptionWithNumbers.text += " тьмы";
                        break;
                    case Schools.Fire:
                        _descriptionWithNumbers.text += " огня";
                        break;
                    case Schools.Water:
                        _descriptionWithNumbers.text += " воды";
                        break;
                    case Schools.Air:
                        _descriptionWithNumbers.text += " воздуха";
                        break;
                    case Schools.Earth:
                        _descriptionWithNumbers.text += " земли";
                        break;
                    case Schools.Physical:
                        // ---
                        break;
                    case Schools.Discipline:
                        // ????
                        break;
                    case Schools.None:
                        break;
                    default:
                        break;
                }
                break;
            case DamageType.Physical:
                _descriptionWithNumbers.text += " физический";
                break;
            case DamageType.DOTPhys:
                break;
            case DamageType.DOTMag:
                break;
            case DamageType.Both:
                _descriptionWithNumbers.text += "смешанный";
                break;
            case DamageType.None:
                break;
            default:
                break;
        }
    }

    private string GetSchoolName(Skill skill)
    {
        switch (skill.School)
        {
            case Schools.Light:
                return "света";
                break;
            case Schools.Dark:
                return "тьмы";
                break;
            case Schools.Fire:
                return "огня";
                break;
            case Schools.Water:
                return "воды";
                break;
            case Schools.Air:
                return "воздуха";
                break;
            case Schools.Earth:
                return "земли";
                break;
            case Schools.Physical:
                return "";
                // ---
                break;
            case Schools.Discipline:
                return "";
                // ????
                break;
            case Schools.None:
                return "";
                break;
            default:
                return "";
                break;
        }
    }
}
