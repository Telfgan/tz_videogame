using System.Collections;
using UnityEngine;

public class AssasinPoison : Talent
{
    [SerializeField] private CreeperInvisible _invisibleCreeper;
    [SerializeField] private PoisonBall _poisonBall;
    [SerializeField] private FlowOfPoisons _flowOfPoisons;

    private int _currentChargePoison;
    [SerializeField] private int _maxChargePoison = 3;

    private float _timeAccumulateCharge;
    [SerializeField] private float _startTimeAccumulateCharge = 3f;

    private float _timeForRemoveCharges;
    [SerializeField] private float _startTimeForRemoveCharges = 4f;

    private bool _isCanSpendCharge;

    private Coroutine _accumulateChargesCoroutine;
    private Coroutine _timeForRemoveAllChargesCoroutine;

    public int CurrentChargeAssasinPoison { get => _currentChargePoison; set => _currentChargePoison = value; }
    public bool IsCanSpendCharge { get => _isCanSpendCharge; }
    public override void Enter()
    {
        SetActive(true);
        if (_accumulateChargesCoroutine == null)
        {
            _accumulateChargesCoroutine = StartCoroutine(AccumulateChargeJob());
        }
    }

    public override void Exit()
    {
        SetActive(false);
        if (_accumulateChargesCoroutine != null)
        {
            StopCoroutine(AccumulateChargeJob());
            _accumulateChargesCoroutine = null;
        }
    }

    public void SpendCharge(Character target, float durationDebuff)
    {
        //if (character.CharacterState.CheckForState(States.CreeperInvisible))
        //{
        //}

        if (_currentChargePoison > 0)
        {
            _currentChargePoison--;
            target.CharacterState.CmdAddState(States.PoisonBone, durationDebuff, 0, character.gameObject, null);

            if (_flowOfPoisons.Data.IsOpen)
                FlowOfPoisonConvertCharge();

            Debug.Log("AssasinPoison / SpendCharge / currentChargePoison = " + _currentChargePoison);
        }
    }

    private void FlowOfPoisonConvertCharge()
    {
        if (Data.IsOpen && _flowOfPoisons.Data.IsOpen)
        {
            for (int i = 0; i < _poisonBall.RemainingCooldownTimeCharge.Count; i++)
            {
                if (_poisonBall.RemainingCooldownTimeCharge[i] > 0)
                {
                    Debug.Log("TestCharge / ConvertCharge");

                    float newCooldownTime = 0f;

                    //_poisonBall.ReductionCooldownTimeCharge(newCooldownTime);

                    break;
                }
            }
        }
    }

    public void RemoveAllCharges()
    {
        _timeForRemoveCharges = _startTimeForRemoveCharges;

        if (_timeForRemoveAllChargesCoroutine == null)
            _timeForRemoveAllChargesCoroutine = StartCoroutine(TimeForRemoveAllChargesJob());
    }
    
    private IEnumerator TimeForRemoveAllChargesJob()
    {
        while (_timeForRemoveCharges > 0)
        {
            yield return null;
        }

        if (_currentChargePoison > 0)
            _currentChargePoison = 0;

        Debug.Log("AssasinPoison / RemoveAllChargesCoroutine / currentCharges = " + _currentChargePoison);
        _timeAccumulateCharge = _startTimeAccumulateCharge;

        StopCoroutine(_timeForRemoveAllChargesCoroutine);
        _timeForRemoveAllChargesCoroutine = null;
    }

    private IEnumerator AccumulateChargeJob()
    {
        while (Data.IsOpen)
        {
            if (_currentChargePoison < 3 && character.CharacterState.CheckForState(States.CreeperInvisible))
            {
                _timeAccumulateCharge -= Time.deltaTime;
                if (_timeAccumulateCharge <= 0 && _currentChargePoison < _maxChargePoison)
                {
                    AccumulateChargePoison();
                    Debug.Log("AssasinPoison / AccumulateChargeJob / Accumulate Charge Done");
                }
            }
            yield return null;
        }
    }


    private void AccumulateChargePoison()
    {
        _currentChargePoison++;
        _timeAccumulateCharge = _startTimeAccumulateCharge;    
        Debug.Log("Accumulate AssasinPoison Charge = " + _currentChargePoison);
    }

}
