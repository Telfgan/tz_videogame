using System.Linq;
using UnityEngine;

public class AttributeModifier
{
    private readonly AttributeSaveManager _attributeSaveManager;

    public AttributeModifier(AttributeSaveManager attributeSaveManager)
    {
        _attributeSaveManager = attributeSaveManager;
    }

    public void ChangeAttribute(HeroComponent character, int index, int points, int saveGroup)
    {
        int currentPoints = _attributeSaveManager.LoadAttributePoints(character, saveGroup);
        
        if (currentPoints - points < 0) return;

        var attribute = character.Data.Attributes.AttributeData.FirstOrDefault(a => a.Id == index);
        if (attribute == null) return;
        
        attribute.Points += points;

        _attributeSaveManager.SaveAttribute(character, attribute.Id , saveGroup);
        
        var usedAttributes = _attributeSaveManager.LoadUsedAttributes(character , saveGroup);
        
        for (var i = 0; i < points; i++)
        {
            usedAttributes.Add(index);
        }
        _attributeSaveManager.SaveUsedAttributes(character, usedAttributes, saveGroup);
        _attributeSaveManager.SaveAttributePoints(character, saveGroup,currentPoints - points);
    }

    public int ReduceFreePoints(HeroComponent character, int pointsToDeduct, int saveGroup)
    {
        int freePoints = _attributeSaveManager.LoadAttributePoints(character, saveGroup);
        int deductAmount = Mathf.Min(freePoints, pointsToDeduct);
        _attributeSaveManager.SaveAttributePoints(character, saveGroup, freePoints - deductAmount);
        return pointsToDeduct - deductAmount;
    }

    public void ReduceAttributePoints(HeroComponent character, int pointsToDeduct, int saveGroup)
    {
        var usedAttributes = _attributeSaveManager.LoadUsedAttributes(character, saveGroup);
        usedAttributes.Reverse();

        foreach (var attributeIndex in usedAttributes)
        {
            if (pointsToDeduct <= 0) break;

            var attribute = character.Data.Attributes.AttributeData.FirstOrDefault(a => a.Id == attributeIndex);
            if (attribute == null || attribute.Points <= 0) continue;

            int deductPoints = Mathf.Min(pointsToDeduct, 1);
            attribute.Points -= deductPoints;
            pointsToDeduct -= deductPoints;

            _attributeSaveManager.SaveAttribute(character, attribute.Id , saveGroup);
            _attributeSaveManager.LoadAttribute(character, attribute.Id, saveGroup);
        }

        usedAttributes.RemoveAll(attributeIndex => character.Data.Attributes.AttributeData.FirstOrDefault(a => a.Id == attributeIndex)?.Points <= 0);
        _attributeSaveManager.SaveUsedAttributes(character, usedAttributes, saveGroup);
    }
}
