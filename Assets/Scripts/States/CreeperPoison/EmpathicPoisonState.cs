using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmpathicPoisonsState : AbstractCharacterState, IDamageable
{
    private PoisonCloudState _poisonCloud;
    private Character _player;
    private DamageType _damageType;
    private Resource _playerResource;
    private AttackRangeType _attackRangeType;

    private int _maxStacks = 8;

    private float _baseEvasionValue = 0.03f;
    private float _increasedEvasionValue;
    private float _evadeMeleePhysicalDamage;
    private float _evadeRangePhysicalDamage;
    private float _originalEvadeMeleeDamage;
    private float _originalEvadeRangeDamage;

    private float _radiusCloud;

    private float _timeBeforeReductionDebuff;
    private float _startTimeBeforeReductionDebuff = 1.0f;

    private float _duration;
    private float _baseDuration;
    private float _damageToExit;

    private Vector3 _playerPosition;
    private Vector3 _characterPosition;

    private bool _isInPoisonCloud;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Poison };

    public int CurrentStacks { get => CurrentStacksCount; set => CurrentStacksCount = value; }
    public float StacksDuration { get => _duration; }

    public event Action<Damage, Skill> DamageTaken;
    public override States State => States.EmpathicPoisons;
    public override StateType Type => StateType.Physical; 
    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;

    public override List<StatusEffect> Effects => _effects;

    public Transform transform => throw new NotImplementedException();
    public GameObject gameObject => throw new NotImplementedException();

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _player = personWhoMadeBuff;

        MaxStacksCount = _maxStacks;

        _timeBeforeReductionDebuff = _startTimeBeforeReductionDebuff;

        _originalEvadeMeleeDamage = _player.Health.EvadeMeleeDamage;
        _evadeMeleePhysicalDamage = _player.Health.EvadeMeleeDamage;

        _originalEvadeRangeDamage = _player.Health.EvadeRangeDamage;
        _evadeRangePhysicalDamage = _player.Health.EvadeRangeDamage;

        _player.Health.Shields.Add(this);
        _poisonCloud = (PoisonCloudState)_player.CharacterState.GetState(States.PoisonCloud);
        _radiusCloud = _poisonCloud.RadiusCloud;

        _duration = durationToExit;
        _baseDuration = durationToExit;

        if (CurrentStacksCount < MaxStacksCount)
        {
            CurrentStacksCount++;
        }
    }

    public void ShowPhantomValue(Damage value)
    {

    }

    public bool TryTakeDamage(ref Damage damage, Skill skill)
    {
        if (CurrentStacksCount > 0)
        {
          //  Debug.Log("EmpathicPoison / if (currentStacks > 0) currentStacks == " + _currentStacks);
            switch (_damageType)
            {
                case DamageType.Physical:
                //    Debug.Log("EmpathicPoison / TryTakeDamage / Case DamageType.Physical");
                    switch (_attackRangeType)
                    {
                        case AttackRangeType.MeleeAttack:
                       //     Debug.Log("EmpathicPoison / TryTakeDamage / Case DamageType.Physical / case AttackRangeType.Melee");
                            if (UnityEngine.Random.Range(0.0f, 100.0f) <= _evadeMeleePhysicalDamage)
                            {
                           //     Debug.Log("EmpathicPoison / TryTakeDamage / case AttackRangeType.Melee / if evadeMeleeDamage");
                                damage.Value = 0;
                                return true;
                            }
                            else
                            {
                             //   Debug.Log("EmpathicPoison / TryTakeDamage / case AttackRangeType.Melee / else evadeMeleeDamage");
                                return false;
                            }
                            break;

                        case AttackRangeType.RangeAttack:
                          //  Debug.Log("EmpathicPoison / TryTakeDamage / Case DamageType.Physical / case AttackRangeType.Range");
                            if (UnityEngine.Random.Range(0.0f, 100.0f) <= _evadeRangePhysicalDamage)
                            {
                               // Debug.Log("EmpathicPoison / TryTakeDamage / case AttackRangeType.Range / if evadeRangeDamage");
                                damage.Value = 0;
                                return true;
                            }
                            else
                            {
                              //  Debug.Log("EmpathicPoison / TryTakeDamage / case AttackRangeType.Range / else evadeRangeDamage");
                                return false;
                            }
                            break;

                        default:
                            break;
                    }
                    break;

                default:
                    break;
            }
        }
        return true;
    }

    public override void UpdateState()
    {
        _playerPosition = _player.transform.position;
        _characterPosition = _characterState.transform.position;

        _duration -= Time.deltaTime;
        if (_duration < 0)
        {
            ExitState();
        }

        if (CurrentStacksCount <= 0)
        {
            ExitState();
        }

        _timeBeforeReductionDebuff -= Time.deltaTime;
        if (_timeBeforeReductionDebuff <= 0)
        {
            CheckIfInPoisonCloud(_playerPosition, _characterPosition);
            if (_isInPoisonCloud)
            {
                ReducingChanceOfHittingAtEnemy();
                _timeBeforeReductionDebuff = _startTimeBeforeReductionDebuff;
            }
            else
            {
                DecreaseEvasionForCurrentTarget();
                _timeBeforeReductionDebuff = _startTimeBeforeReductionDebuff;
            }
        }
    }

    public override void ExitState()
    {
        ResetValues();
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        if (CurrentStacksCount < MaxStacksCount)
        {
            CurrentStacksCount++;
            _duration = _baseDuration;
            return true;
        }
        else
        {
            _duration = _baseDuration;
            return true;
        }
    }

    private void ReducingChanceOfHittingAtEnemy()
    {
        if (CurrentStacksCount < MaxStacksCount)
        {
            _increasedEvasionValue = _baseEvasionValue * CurrentStacksCount;
            _evadeMeleePhysicalDamage += _increasedEvasionValue;
            _evadeRangePhysicalDamage += _increasedEvasionValue;
        }
    }

    private void DecreaseEvasionForCurrentTarget()
    {
        //float reductionPerSecond = _baseEvasionValue * 0.33f;
        //_endEvasionValue = Mathf.Max(_originalEvasionValue, _characterState.Character.Health.EvadeMeleeDamage + reductionPerSecond);
        //_characterState.Character.Health.EvadeMeleeDamage = _endEvasionValue;
    }

    private void CheckIfInPoisonCloud(Vector3 playerPos, Vector3 characterPos)
    {
        float distance = Vector3.Distance(playerPos, characterPos);
        _isInPoisonCloud = distance <= _radiusCloud;
    }

    private void ResetValues()
    {
        CurrentStacksCount = 0;
        _baseDuration = 0;
        _duration = 0;

        _baseEvasionValue = 0.03f;
        _increasedEvasionValue = 0;
        _evadeMeleePhysicalDamage = _originalEvadeMeleeDamage;
        _evadeRangePhysicalDamage = _originalEvadeRangeDamage;
    }

    public void SetRadiusCloud(float value)
    {
        _radiusCloud = value;
    }
}
