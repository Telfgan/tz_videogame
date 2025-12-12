using Mirror;
using UnityEngine;

public class TreeCharacterZone : NetworkBehaviour
{
    [SerializeField] private MoveComponent moveComponentElf;
    [SerializeField] private float minVelocityToTriggerDisable = 0.1f;
    private Character characterInZone;
    private Collider treeZoneCollider;

    private void Awake()
    {
        treeZoneCollider = GetComponent<Collider>();
        if (treeZoneCollider == null)
            Debug.LogError("TreeCharacterZone requires a Collider component.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Character>(out Character character))
        {
            characterInZone = character;
            moveComponentElf = character.Move;
            character.VisionComponent.VisionRange += 3;

            IncreasePhysicalSkillRange(character, 1.5f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (moveComponentElf == null || treeZoneCollider == null) return;

        Rigidbody rb = moveComponentElf.Rigidbody;
        if (rb == null) return;

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (horizontalVelocity.magnitude >= minVelocityToTriggerDisable)
        {
            treeZoneCollider.isTrigger = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Character>(out Character character) && character == characterInZone)
        {
            moveComponentElf = null;
            characterInZone = null;

            character.VisionComponent.VisionRange -= 3;
            RestorePhysicalSkillRange(character, 1.5f);

            if (treeZoneCollider != null) treeZoneCollider.isTrigger = true;
        }
    }

    private void IncreasePhysicalSkillRange(Character character, float multiplier)
    {
        if (character.Abilities == null) return;

        foreach (var skill in character.Abilities.Abilities)
        {
            if (skill.AbilityForm == AbilityForm.Physical)
                skill.Radius *= multiplier;
        }
    }

    private void RestorePhysicalSkillRange(Character character, float multiplier)
    {
        if (character.Abilities == null) return;

        foreach (var skill in character.Abilities.Abilities)
        {
            if (skill.AbilityForm == AbilityForm.Physical)
                skill.Radius /= multiplier;
        }
    }
}
