using UnityEngine;

public class PreparingForFight : Talent
{
    [SerializeField] private float _manaRecoveryMultiplier = 0.01f;
    private float _maxManaPlayer;

    public override void Enter()
    {
        SetActive(true);
    }

    public override void Exit()
    {
        SetActive(false);
    }

    public void IncreaseManaRegeneration(Character player)
    {
        Resource playerMana = player.TryGetResource(ResourceType.Mana);
        _maxManaPlayer = playerMana.MaxValue;

        float updatedManaRecoveryValue = _maxManaPlayer * _manaRecoveryMultiplier;
        Debug.Log("updatedManaRecoveryValue = " + updatedManaRecoveryValue);
        Debug.Log("PlayerManaValue before AddMana = " + playerMana.CurrentValue);

        playerMana.Add(updatedManaRecoveryValue);
        Debug.Log("PlayerManaValue after AddMana = " + playerMana.CurrentValue);
    }

}
