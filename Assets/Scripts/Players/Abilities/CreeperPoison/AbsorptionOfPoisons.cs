using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbsorptionOfPoisons : Skill
{
    [SerializeField] private Character _player;
    [SerializeField] private LightningMovement _lightningMovement;

    private Dictionary<int, float> _stacks = new();

    private float _timeUntilEndEffect;
    private float _startTimeUntilEndEffect = 6f;
    private float _durationState = 6f;

    private float _cooldownForLightningMovement = 18f;

    private bool _isWorking = false;

    private PoisonBoneState _poisonBone;
    private EmpathicPoisonsState _empathicPoison;
    private WitheringPoisonState _witheringPoison;
    private BindingPoisonState _bindingPoison;

    private Coroutine _remainingTimeCoroutine;

    public bool IsWorking { get => _isWorking; }
    protected override int AnimTriggerCast => 0;
    protected override int AnimTriggerCastDelay => 0;
    protected override bool IsCanCast => true;

    protected override void ClearData()
    {
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        yield return null;
    }

    protected override IEnumerator CastJob()
    {
        _timeUntilEndEffect = _startTimeUntilEndEffect;
        _isWorking = true;

        if (_remainingTimeCoroutine == null)
        {
            _remainingTimeCoroutine = StartCoroutine(RemainingTimeJob());
        }

        _lightningMovement.IncreaseSetCooldown(_cooldownForLightningMovement);

        yield return null;
    }

    private IEnumerator RemainingTimeJob()
    {
        while (_timeUntilEndEffect > 0)
        {
            _timeUntilEndEffect -= Time.deltaTime;
            if (_timeUntilEndEffect <= 0)
            {
                if (_player.CharacterState.CheckForState(States.AbsorptionOfPoison))
                {
                    _player.CharacterState.CmdRemoveState(States.AbsorptionOfPoison);
                }
                _isWorking = false;
            }

            yield return null;
        }

        StopCoroutine(_remainingTimeCoroutine);
        _remainingTimeCoroutine = null;
    }

    public void CheckTargetWithDebuffs(GameObject target)
    {
        CmdCheckTargetWithDebuffs(target);
    }

    [Command]
    private void CmdCheckTargetWithDebuffs(GameObject target)
    {
        if (target != null)
        {
            Character targetWithDebuffs = target.GetComponent<Character>();

            if (targetWithDebuffs.CharacterState.Check(StatusEffect.Poison))
            {
                AdvertisementStates(targetWithDebuffs.CharacterState);

                Dictionary<AbstractCharacterState, float> poisonDurations = new();

                if (_poisonBone != null && _poisonBone.CurrentStacks > 0)
                {
                    poisonDurations[_poisonBone] = _poisonBone.StacksDuration;
                }
                if (_empathicPoison != null && _empathicPoison.CurrentStacks > 0)
                {
                    poisonDurations[_empathicPoison] = _empathicPoison.StacksDuration;
                }
                if (_witheringPoison != null && _witheringPoison.CurrentStacks > 0)
                {
                    poisonDurations[_witheringPoison] = _witheringPoison.StacksDuration;
                }
                if (_bindingPoison != null && _bindingPoison.CurrentStacks > 0)
                {
                    poisonDurations[_bindingPoison] = _bindingPoison.StacksDuration;
                }

                if (poisonDurations.Count > 0)
                {
                    var stateWithMinDuration = GetStateWithMinDuration(poisonDurations);

                    if (stateWithMinDuration is PoisonBoneState poisonBoneState)
                    {
                        poisonBoneState.CurrentStacks--;
                    }
                    else if (stateWithMinDuration is EmpathicPoisonsState empathicPoisonsState)
                    {
                        empathicPoisonsState.CurrentStacks--;
                    }
                    else if (stateWithMinDuration is WitheringPoisonState witheringPoisonState)
                    {
                        witheringPoisonState.CurrentStacks--;
                    }
                    else if (stateWithMinDuration is BindingPoisonState bindingPoisonState)
                    {
                        bindingPoisonState.CurrentStacks--;
                    }
                }

                _player.CharacterState.AddState(States.AbsorptionOfPoison, _durationState, 0, _player.gameObject, Name);
            }
         }
    }
    private AbstractCharacterState GetStateWithMinDuration(Dictionary<AbstractCharacterState, float> poisonDurations)
    {
        AbstractCharacterState stateWithMinDuration = null;
        float minDuration = float.MaxValue;

        foreach(var minValue in poisonDurations) 
        {
            if (minValue.Value < minDuration)
            {
                minDuration = minValue.Value;
                stateWithMinDuration = minValue.Key;
            }
        }

        return stateWithMinDuration;
    }


    private void AdvertisementStates(CharacterState targetWithDebuff)
    {
        _poisonBone = (PoisonBoneState)targetWithDebuff.GetState(States.PoisonBone);
        _empathicPoison = (EmpathicPoisonsState)targetWithDebuff.GetState(States.EmpathicPoisons);
        _witheringPoison = (WitheringPoisonState)targetWithDebuff.GetState(States.WitheringPoison);
        _bindingPoison = (BindingPoisonState)targetWithDebuff.GetState(States.BindingPoison);
    }
}
