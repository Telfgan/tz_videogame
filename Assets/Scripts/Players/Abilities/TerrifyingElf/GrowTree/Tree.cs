using Mirror;
using UnityEngine;

public class Tree : NetworkBehaviour
{
    private float baseVision;
    private float VisionMultiplier = 3f;
    private float RadiusMultiplier = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Character>(out Character character))
        {
            VisionComponent visionComponent = character.GetComponent<VisionComponent>();
            baseVision = visionComponent.VisionRange;
            visionComponent.VisionRange += VisionMultiplier;

            foreach (var skill in character.Abilities.Abilities) if (skill.AbilityForm == AbilityForm.Physical) skill.Radius *= RadiusMultiplier;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Character>(out Character character))
        {
            VisionComponent visionComponent = character.GetComponent<VisionComponent>();
            visionComponent.VisionRange = baseVision;

            foreach (var skill in character.Abilities.Abilities) if (skill.AbilityForm == AbilityForm.Physical) skill.Radius /= RadiusMultiplier;
        }
    }
}
