using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/Player", order = 1)]
[Serializable]
public class CharacterData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [SerializeField] private string type;
    [SerializeField] private Sprite _icon;
    [SerializeField] private float _iconSize;
    [SerializeField] private AttributeGroup _attributes;

    public string Name => _name;
    public string Type => type;
    public string Description => _description;
    public Sprite Icon => _icon;
    public float IconSize => _iconSize;

    public AttributeGroup Attributes => _attributes;

    public float GetAttributeValue(string attributeName)
    {
        var attribute = _attributes.AttributeData.FirstOrDefault(o => o.Name == attributeName);
        return attribute?.DefaultValue ?? 0f;
    }
}

public static class Positions
{
    public static List<Vector2> unitInGroupPositions = new()
    {
        new Vector2(0, 0),
        new Vector2(0, 3),
        new Vector2(3, 0),
        new Vector2(3, 3),
        new Vector2(0, -3),
        new Vector2(-3, 0),
        new Vector2(-3, -3),
        new Vector2(3, -3),
        new Vector2(-3, 3)
    };
}

[Serializable]
public class Attribute
{
    public int Id;
    public string Name;
    public int Points;

    public float DefaultValue;
    public Sprite Icon;

    public bool IsVisible = false;

    public Attribute(int id, string name, int points)
    {
        Id = id;
        Name = name;
        Points = points;
    }
}

public static class AttributeNames
{
    public const string Health = "Health";
    public const string Mana = "Mana";
    public const string Energy = "Energy";
    public const string Rune = "Rune";
    public const string Speed = "Speed";
    public const string HpRegen = "HPRegen";
    public const string ManaRegen = "ManaRegen";
    public const string EnergyRegen = "EnergyRegen";
    public const string RuneRegen = "RuneRegen";
    public const string HpRegenDelay = "HPRegenDelay";
    public const string ManaRegenDelay = "ManaRegenDelay";
    public const string EnergyRegenDelay = "EnergyRegenDelay";
    public const string RuneRegenDelay = "RuneRegenDelay";
    public const string VisionRadius = "VisionRadius";
    public const string PhysicResist = "PhysicResist";
    public const string MagicResist = "MagicResist";
    public const string MeleeEvade = "MeleeEvade";
    public const string RangeEvade = "RangeEvade";
    public const string MagicEvade = "MagicEvade";
    public const string PhysicAbsorb = "PhysicAbsorb";
    public const string MagicAbsorb = "MagicAbsorb";
}

[Serializable]
public class AttributeGroup
{
    [SerializeField]
    private List<Attribute> attributesGroup = new()
    {
        new Attribute(1001, AttributeNames.Health, 0),
        new Attribute(1002, AttributeNames.Mana, 0),
        new Attribute(1003, AttributeNames.Energy, 0),
        new Attribute(1004, AttributeNames.Rune, 0),
        new Attribute(1005, AttributeNames.Speed, 0),
        new Attribute(1006, AttributeNames.HpRegen, 0),
        new Attribute(1007, AttributeNames.ManaRegen, 0),
        new Attribute(1008, AttributeNames.EnergyRegen, 0),
        new Attribute(1009, AttributeNames.RuneRegen, 0),
        new Attribute(1010, AttributeNames.HpRegenDelay, 0),
        new Attribute(1011, AttributeNames.ManaRegenDelay, 0),
        new Attribute(1012, AttributeNames.EnergyRegenDelay, 0),
        new Attribute(1013, AttributeNames.RuneRegenDelay, 0),
        new Attribute(1014, AttributeNames.VisionRadius, 0),
        new Attribute(1015, AttributeNames.PhysicResist, 0),
        new Attribute(1016, AttributeNames.MagicResist, 0),
        new Attribute(1017, AttributeNames.MeleeEvade, 0),
        new Attribute(1018, AttributeNames.RangeEvade, 0),
        new Attribute(1019, AttributeNames.MagicEvade, 0),
        new Attribute(1020, AttributeNames.PhysicAbsorb, 0),
        new Attribute(1021, AttributeNames.MagicAbsorb, 0)
    };

    public List<Attribute> AttributeData => attributesGroup;
    public int FreeAttributePointsCount { get; set; }
    public int UsedAttributePointsCount => attributesGroup.Sum(o => o.Points);
}