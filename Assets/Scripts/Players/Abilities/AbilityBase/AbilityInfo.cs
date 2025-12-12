using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ability/AbilityInfo", fileName = "AbilityInfo")]
public class AbilityInfo : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [SerializeField] private string _state;
    [SerializeField] private string _descriptionState;
    [SerializeField] private string _counter;
    [SerializeField] private Sprite _icon;

    private string _finalDescription;

    public string FinalDescription { get => _finalDescription; set => _finalDescription = value; }
    public string Name => _name;
    public string Description => _description;
    public string State => _state;
    public string DescriptionState => _descriptionState;
    public string Counter => _counter;
    public Sprite Icon => _icon;

    private void OnEnable()
    {
        _finalDescription = _description;
    }
}
