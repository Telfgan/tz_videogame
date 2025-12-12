using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIcon : MonoBehaviour
{
    [SerializeField] private GameObject _icon;
    [SerializeField] private Image _playerIcon;
    [SerializeField] private ReviveVisualUI _reviveVisual;
    [SerializeField] private Bar _playerHp;
    [SerializeField] private Bar _playerMana;

    private Character _character;

    public Character Character { get => _character; }

    public void Init(Character character)
    {
        _character = character;
        UpdateInfo(character);
    }

    public void OnCharacterSelected(Character character)
    {
        gameObject.SetActive(true);

        UpdateInfo(character);
    }

    public void OnCharacterDeselected(Character character)
    {
        gameObject.SetActive(false);
    }

    public void StartReviveTimer(float time)
    {
        _reviveVisual.StartTimer(time);
    }

    protected virtual void UpdateInfo(Character character)
    {
        _playerIcon.sprite = character.Data.Icon;
        _playerHp.Init(character.Health);
        // _playerMana.Init(character.Resources.FirstOrDefault(o=>o.Type == ResourceType.Mana));
    }
}
