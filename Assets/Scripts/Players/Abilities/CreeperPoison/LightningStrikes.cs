using System;
using System.Collections;
using UnityEngine;

public class LightningStrikes : AutoAttackSkill
{
    [Header("Talents")]
    [SerializeField] private HeatedGlands _heatedGlands;
    [SerializeField] private KillersStamina _killersStamina; 
    
    [Header("Abillity Components")]
    [SerializeField] private ColdBlood _coldBlood;
    [SerializeField] private LightningMovement _lightningMovement;
    [SerializeField] private CreeperStrike _creeperStrike;
    [SerializeField] private Character _player;
    
    //private Character _currentTarget;

    private float _animTime;
    private float _cooldownMultiplier = 2f;
    private float _heatedGlandsDuration = 4f;

    private bool _isUsedLightningStrikes = false;
    private bool _isIncreaseCooldownTime = false;
    private bool _isCanDamageDeal = false;

    public float BaseCooldownTime { get => _baseCooldownTime; }
    public bool IsUsedLightningStrikes { get => _isUsedLightningStrikes; set => _isUsedLightningStrikes = value; }
    public bool IsCanDamageDeal { get => _isCanDamageDeal; set => _isCanDamageDeal = value; }

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerAutoAttack => Animator.StringToHash("LightningStrikesAttacking");

    public event System.Action OnLightningStrikesEnd;

    protected override void Awake()
    {
        base.Awake();

        _baseCooldownTime = CooldownTime;
    }

    public void AnimLightningStrikesCast()
    {
        AnimCastAction();
    }

    public void AnimLightningStrikesEnd()
    {
        OnLightningStrikesEnd?.Invoke();
        AnimCastEnded();
    }
   /* public void SetTarget(Character target)
    {
        _target = target;
    }*/

    public void ClearDataLightningStrikes()
    {
        TryCancel();
        StopAutoDraw();
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        throw new NotImplementedException();
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> targetDataSavedCallback)
    {
        if (_lightningMovement.IsInMovement)
        {
            _animTime = GetClipLength();
            IncreaseAnimSpeed();

            Debug.Log("LightningStrikes / PrepareJob");
        }
        return base.PrepareJob(targetDataSavedCallback);
    }

    protected override void CastAction()
    {
        if (_lightningMovement.IsInMovement)
        {
            _animTime = GetClipLength();
            IncreaseAnimSpeed();
        }

        Debug.Log("LightningStrikes / CastAction");

        if (_coldBlood.IsCanCritLightningStrikes && _isIncreaseCooldownTime == false)
        {
            float newCooldownTime = _baseCooldownTime * _cooldownMultiplier;
            CooldownTime = newCooldownTime;

            _isIncreaseCooldownTime = true;
        }
        else
        {
            CooldownTime = _baseCooldownTime;
        }

        /*if (_currentTarget == null)
            _currentTarget = _target;*/

        DamageDeal();
    }

    private float GetClipLength()
    {
        RuntimeAnimatorController animController = _player.Animator.runtimeAnimatorController;
        foreach (var clip in animController.animationClips)
        {
            if (clip.name == "LightningStrikesAttack")
            {
                return clip.length;
            }
        }
        return -1f;
    }

    private void IncreaseAnimSpeed()
    {
        if (_animTime > 0)
        {
            float multiplier = _lightningMovement.DurationLeap - 4.9f; // �������� �������� (���������� - 0.1)
            float animTimeMultiplier = _animTime / multiplier;
            Debug.Log("LightningStrikes / multiplier = " + animTimeMultiplier);
            _player.Animator.SetFloat("LightningStrikesMultiplierSpeedAnimation", animTimeMultiplier);
        }
    }

    private void DamageDeal()
    {
        Debug.Log("LightningStrikes / DamageDeal");
        _creeperStrike.DamageDeal(GetTargetCharacter(), true);

       _isCanDamageDeal = false;

        //if (_heatedGlands.Data.IsOpen)
        //    _player.CharacterState.CmdAddState(States.HeatedGlands, _heatedGlandsDuration, 0, _player.gameObject, null);

        if (_coldBlood.IsCanCritLightningStrikes && _isIncreaseCooldownTime == true) _isIncreaseCooldownTime = false;
    }
}