using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalantAndAttributs : MonoBehaviour
{
    [SerializeField] private UIMenuMainAttributesPanel _attributesPanel;
    [SerializeField] private UIMenuMainTalentsPanel _talentsPanel;

    public void SetCharacterForInfo(HeroComponent character)
    {
        gameObject.SetActive(true);
        _attributesPanel.gameObject.SetActive(true);
        _talentsPanel.gameObject.SetActive(true);

        SaveManager.Instance.SetHero(character);
        _attributesPanel.Show(character.Data.Attributes);
        _talentsPanel.Show(character.TalentManager, true, false);
    }
}
