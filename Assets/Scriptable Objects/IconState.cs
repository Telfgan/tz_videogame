using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Ability/IconOnOff", fileName = "NewIconOnOff")]
public class IconState : ScriptableObject
{
    [SerializeField] private Sprite _on;
    [SerializeField] private Sprite _off;

    public Sprite On { get => _on; }
    public Sprite Off { get => _off; }
}
