using UnityEngine;
using TMPro;

public class HeroInfoUI : MonoBehaviour
{
    [SerializeField] private PlayerIcon _playerIcon;
    [SerializeField] private TMP_Text _heroName;
    [SerializeField] private TMP_Text _UserName;
    [SerializeField] private TMP_Text _kills;
    [SerializeField] private TMP_Text _damageInfo;
    [SerializeField] private TMP_Text _death;
    [SerializeField] private TMP_Text _deathInfo;
    [SerializeField] private TMP_Text _assist;

    private Character _hero;

    public void SetHero(Character character)
    {
        _hero = character;
        UpdateInfo();
    }

    public void UpdateInfo()
    {
        _playerIcon.Init(_hero);
        _heroName.text = _hero.Data.Name;
        _kills.text = _hero.KillCounter.ToString();
        _death.text = _hero.DeadsCounter.ToString();
        _assist.text = _hero.AssystCounter.ToString();

        _damageInfo.text = _hero.DamageGetCounter.ToString();
        _deathInfo.text = _hero.DamageTakeCounter.ToString();
    }
}
