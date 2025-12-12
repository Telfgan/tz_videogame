using UnityEngine;

public class SuperFastScales : Talent
{
    private float _chanceOfDispelMagStates = 0.9f;
    private float _increaseResistanceToMagicDamage = 90f;
    private float _baseDefMagDamage;

    public override void Enter()
    {
        SetActive(true);
        _baseDefMagDamage = character.Health.DefMagDamage;
    }

    public override void Exit()
    {
        SetActive(false);
    }

    public void IncreasingResistance(Character target)
    {
        if (Random.Range(0.0f, 1.0f) <= _chanceOfDispelMagStates)
        {
            Debug.Log("SuperFastScales / DispelMageStates");
            if (target != null)
                character.CharacterState.DispelStates(StateType.Magic, target.NetworkSettings.TeamIndex, character.NetworkSettings.TeamIndex);
        }

        _baseDefMagDamage = character.Health.DefMagDamage;
        Debug.Log("BaseMagDamage = " + _baseDefMagDamage);

        if (character.Health.ResistMagDamage < 100f)
        {
            character.Health.ResistMagDamage = _increaseResistanceToMagicDamage;
            Debug.Log($"Increased ResistMagDamage == {character.Health.ResistMagDamage}");
        }
    }

    public void ResetResistance()
    {
        Debug.Log("Reset baseMagDamage = " + _baseDefMagDamage);
        character.Health.ResistMagDamage = _baseDefMagDamage;
        Debug.Log($"Reset ResistMagDamage == {character.Health.ResistMagDamage}");
    }
}
