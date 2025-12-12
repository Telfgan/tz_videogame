using System.Collections.Generic;
using System.Linq;

public class AttributeSaveManager
{
    private readonly ISaveData _saveData;

    public AttributeSaveManager(ISaveData saveData)
    {
        _saveData = saveData;
    }

    public void SaveAttributePoints(HeroComponent character, int saveGroup, int points)
    {
        _saveData.SaveInt($"{character.Data.Name}_Group{saveGroup}_FreeAttributesPoints", points);
    }

    public int LoadAttributePoints(HeroComponent character, int saveGroup)
    {
        return _saveData.LoadInt($"{character.Data.Name}_Group{saveGroup}_FreeAttributesPoints", 0);
    }

    public void SaveAllAttributes(HeroComponent character, int saveGroup)
    {
        foreach (var attribute in character.Data.Attributes.AttributeData)
        {
            SaveAttribute(character, attribute.Id, saveGroup);
        }
    }

    public void LoadAllAttributes(HeroComponent character, int saveGroup)
    {
        foreach (var attribute in character.Data.Attributes.AttributeData)
        {
            attribute.Points = _saveData.LoadInt($"{character.Data.Name}_Group{saveGroup}_{attribute.Name}_Points", 0);
        }
    }

    public void SaveAttribute(HeroComponent character, int index, int saveGroup)
    {
        var attribute = character.Data.Attributes.AttributeData.FirstOrDefault(a => a.Id == index);
        if (attribute == null) return;
        
        _saveData.SaveInt($"{character.Data.Name}_Group{saveGroup}_{attribute.Name}_Points", attribute.Points);
    }

    public List<int> LoadUsedAttributes(HeroComponent character, int saveGroup)
    {
        string savedUsedAttributes = _saveData.LoadString($"{character.Data.Name}_Group{saveGroup}_UsedAttributePoints", "");
        if (string.IsNullOrWhiteSpace(savedUsedAttributes)) return new List<int>();

        return savedUsedAttributes.Split(',')
            .Select(s => int.TryParse(s, out int index) ? (int?)index : null)
            .Where(i => i.HasValue)
            .Select(i => i.Value)
            .ToList();
    }

    public void SaveUsedAttributes(HeroComponent character, List<int> usedAttributes, int saveGroup)
    {
        string savedUsedAttributes = string.Join(",", usedAttributes);
        _saveData.SaveString($"{character.Data.Name}_Group{saveGroup}_UsedAttributePoints", savedUsedAttributes);
    }
    
    public void LoadAttribute(HeroComponent character, int attributeId, int saveGroup)
    {
        var attribute = character.Data.Attributes.AttributeData.FirstOrDefault(a => a.Id == attributeId);
        if (attribute == null) return;

        attribute.Points = _saveData.LoadInt($"{character.Data.Name}_Group{saveGroup}_{attribute.Name}_Points", 0);
    }
}
