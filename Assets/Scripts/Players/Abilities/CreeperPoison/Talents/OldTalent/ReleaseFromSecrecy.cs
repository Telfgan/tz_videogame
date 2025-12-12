using System.Collections;
using UnityEngine;

public class ReleaseFromSecrecy : Talent
{
    [SerializeField] private CreeperInvisible _creeperInvisible;
    [SerializeField] private CreeperStrike _creeperStrike;

    [SerializeField] private float _attackSpeedIncrease = 0.3f;

    private int _maxCountBuff = 1;
    private int _currentCountBuff = 0;

    private float _startTimeDurationBuff = 7f;
    private float _timeDurationBuff;

    private float _currentAttackSpeed;

    private Coroutine _increasingAttackSpeedCoroutine;

    public override void Enter()
    {
        SetActive(true);
        _timeDurationBuff = _startTimeDurationBuff;
    }

    public override void Exit()
    {
        SetActive(false);
        _timeDurationBuff = 0;
    }


    public void ApplyBuff()
    {
        if (!_creeperInvisible.IsInvisible)
        {
            if (_currentCountBuff < _maxCountBuff)
            {
                _currentCountBuff++;

                _increasingAttackSpeedCoroutine = StartCoroutine(IncreasingAttackSpeedJob());
            }
        }
    }

    private IEnumerator IncreasingAttackSpeedJob()
    {
        IncreaseAttackSpeed();

        yield return new WaitForSeconds(_timeDurationBuff);

        ReturnOriginalAttackSpeed();

        StopCoroutine(_increasingAttackSpeedCoroutine);
        _increasingAttackSpeedCoroutine = null;

        yield return null;
    }

    private void IncreaseAttackSpeed()
    {
        Debug.Log($"ReleaseFromSecrecy / IncreaseAttackSpeed / _creeperStrike.Buff.AttackSpeed = {_creeperStrike.Buff.AttackSpeed.Multiplier}");
        _creeperStrike.Buff.AttackSpeed.IncreasePercentage(_attackSpeedIncrease);
        Debug.Log($"ReleaseFromSecrecy / IncreaseAttackSpeed / _currentAttackSpeed = {_currentAttackSpeed}");
        Debug.Log($"ReleaseFromSecrecy / IncreaseAttackSpeed / _creeperStrike.Buff.AttackSpeed.Increase = {_creeperStrike.Buff.AttackSpeed.Multiplier}");
    }

    private void ReturnOriginalAttackSpeed()
    {
        Debug.Log($"ReleaseFromSecrecy / ReturnOriginalAttackSpeed / _creeperStrike.Buff.AttackSpeed = {_creeperStrike.Buff.AttackSpeed.Multiplier}");
        _creeperStrike.Buff.AttackSpeed.ReductionPercentage(_attackSpeedIncrease);
        Debug.Log($"ReleaseFromSecrecy / IncreaseAttackSpeed / _currentAttackSpeed = {_currentAttackSpeed}");
        Debug.Log($"ReleaseFromSecrecy / ReturnOriginalAttackSpeed / _creeperStrike.Buff.AttackSpeed.Reduction = {_creeperStrike.Buff.AttackSpeed.Multiplier}");
        _timeDurationBuff = _startTimeDurationBuff;
        _currentCountBuff = 0;
    }
}
