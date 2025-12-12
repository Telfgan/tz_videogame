using System.Collections;
using UnityEngine;

public class FeelingOfContinuation : Talent
{
    [SerializeField] private float _reductionTimeManaRegenMultiplier = 2f;
    private float _remainingManaValue;

    private float _maxMana;
    private float _currentMana;
    private float _originalRegenerationMana;
    private float _baseTimeRegenMana;
    private float _reductionTimeRegenMana;

    private Coroutine _manaRegenerationCoroutine;

    public override void Enter()
    {
        SetActive(true);
    }

    public override void Exit()
    {
        SetActive(false);
    }

    public void IncreaseRegenerationMana(Character player, float playerCriticalDamage)
    {
        _baseTimeRegenMana = character.TryGetResource(ResourceType.Mana).RegenerationDelay;

        _originalRegenerationMana = player.TryGetResource(ResourceType.Mana).RegenerationValue;

        _remainingManaValue = playerCriticalDamage;

        if (_manaRegenerationCoroutine != null)
        {
            StopCoroutine(_manaRegenerationCoroutine);
            _manaRegenerationCoroutine = null;
            _reductionTimeRegenMana = _baseTimeRegenMana;
        }

        _reductionTimeRegenMana = _baseTimeRegenMana / _reductionTimeManaRegenMultiplier;
        player.TryGetResource(ResourceType.Mana).RegenerationDelay = _reductionTimeRegenMana;

        _manaRegenerationCoroutine = StartCoroutine(ManaRegenerationJob(player, _remainingManaValue));
    }

    private IEnumerator ManaRegenerationJob(Character player, float remainingManaValue)
    {
        while (remainingManaValue > 0)
        {
            yield return new WaitForSeconds(_reductionTimeRegenMana);

            remainingManaValue -= _originalRegenerationMana;

            _maxMana = player.TryGetResource(ResourceType.Mana).MaxValue;
            _currentMana = player.TryGetResource(ResourceType.Mana).CurrentValue;

            if (_currentMana >= _maxMana)
            {
                _currentMana = _maxMana;
                CancelCoroutine(player);
                yield break;
            }
        }
        CancelCoroutine(player);
    }

    private void CancelCoroutine(Character player)
    {
        StopCoroutine(_manaRegenerationCoroutine);
        _manaRegenerationCoroutine = null;

        player.TryGetResource(ResourceType.Mana).RegenerationDelay = _baseTimeRegenMana;
    }
}
