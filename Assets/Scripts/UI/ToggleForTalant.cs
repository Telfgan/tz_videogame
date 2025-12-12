using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleForTalant : MonoBehaviour
{
    [SerializeField] private PlayerIcon _playerIcon;
    [SerializeField] private Button _button;

    private TalantAndAttributs _talantAndAttributs;

    private void Start()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        if (_talantAndAttributs == null)
        {
            _talantAndAttributs = Object.FindObjectOfType<TalantAndAttributs>();
        }
        _talantAndAttributs.SetCharacterForInfo((HeroComponent)_playerIcon.Character);
    }
}
