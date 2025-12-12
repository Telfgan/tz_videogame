using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ability/AbilityLineRenderer", fileName = "NewLine")]
public class AbilityLineRenderer : ScriptableObject
{
    [SerializeField] private BoxArea _startSprite;
    [SerializeField] private BoxArea _endSprite;

    public BoxArea Start => _startSprite;
    public BoxArea End => _endSprite;
}
