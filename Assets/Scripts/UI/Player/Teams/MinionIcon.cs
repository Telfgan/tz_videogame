using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MinionIcon : MonoBehaviour
{
    [SerializeField] private Image _playerIcon;
    [SerializeField] private Bar _playerHp;
    [SerializeField] private Bar _playerMana;

    public void Init(Character character)
    {
        _playerIcon.sprite = character.Data.Icon;
        _playerHp.Init(character.Health);
        _playerMana.Init(character.Resources.FirstOrDefault(o=>o.Type == ResourceType.Mana));
    }
}
