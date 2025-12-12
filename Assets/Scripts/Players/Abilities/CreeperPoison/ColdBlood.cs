using System;
using System.Collections;
using UnityEngine;

public class ColdBlood : Skill
{
    [Header("Talent")]
    [SerializeField] private ColdBloodEnabledTalent _coldBloodEnabledTalent;
    [SerializeField] private Indomitable _indomitable;
    [SerializeField] private ColdBloodTalent _coldBloodTalent;
    [SerializeField] private KillersStamina _killersStamina;

    [Header("Ability Properties")]
    [SerializeField] private CreeperStrike _creeperStrike;
   // [SerializeField] private Character _player;
    [SerializeField] private float _reducingCooldownMultiplier = 2f;

    //private Character _target;
    private Vector3 _mousePosition = Vector3.positiveInfinity;

    private float _cooldownTimeWithTalent = 4f;

    private bool _isPlayer = false;
    private bool _isCanCritCreeperStrike;
    private bool _isCanCritLightningStrikes;

    private Coroutine _waitingHitFromCreeperStrike;

    public Indomitable IndomitableTalent { get => _indomitable; }
    public ColdBloodTalent ColdBloodTalent { get => _coldBloodTalent; }
    public KillersStamina KillersStaminaTalent { get => _killersStamina; }
    public bool IsCanCritCreeperStrike { get => _isCanCritCreeperStrike; set => _isCanCritCreeperStrike = value; }
    public bool IsCanCritLightningStrikes { get => _isCanCritLightningStrikes; set => _isCanCritLightningStrikes = value; }

    protected override int AnimTriggerCast => 0;
    protected override int AnimTriggerCastDelay => 0;
    protected override bool IsCanCast { get => _coldBloodEnabledTalent.Data.IsOpen; }

    protected override void Awake()
    {
        base.Awake();

        _baseCooldownTime = CooldownTime;    
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        Debug.LogError("DataError");
    }

    protected override void ClearData()
    {
        Debug.Log("ColdBlood / ClearData");
        _mousePosition = Vector3.positiveInfinity;
        //_target = null;
        _isPlayer = false;

        if (Hero.CharacterState.CheckForState(States.Immateriality))
        {
			Hero.CharacterState.CmdRemoveState(States.Immateriality);
        }

        if (_waitingHitFromCreeperStrike != null)
        {
            StopCoroutine(_waitingHitFromCreeperStrike);
            _waitingHitFromCreeperStrike = null;
        }
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        if (_indomitable.Data.IsOpen)
        {
            while (GetTargetCharacter() == null || float.IsPositiveInfinity(_mousePosition.x))
            {
                if (GetMouseButton)
                {
                    FindTargetCharacter(true);
					//_target = GetTarget(true).character;
                   // Debug.Log("ColdBlood / PrepareJob / Input.GetMouseButtonDown / target == " + _target);

                    if (GetTargetCharacter() != Hero)
                    {
                        _isPlayer = false;
                       // Debug.Log("Target != player / Target == " + _target);
                    }
                    if (GetTargetCharacter() == Hero)
                    { 
                        _isPlayer = true;
                       // Debug.Log("Target == player / Target == " + _target);
                    }

                    _mousePosition = GetMousePoint();
                    Debug.Log("ColdBlood / PrepareJob / Input.GetMouseButtonDown / _mousePosition == " + _mousePosition);

					Hero.CharacterState.CmdAddState(States.Immateriality, 0, 0, Hero.gameObject, Name);
                }
                yield return null;
            }
        }
        else
        {
            yield break;
        }

        Debug.LogError("DataError");
    }

    protected override IEnumerator CastJob()
    {
        if (_indomitable.Data.IsOpen)
        {
            UseAbilityWithTalent();
        }
        else
        {
            UseAbilityWithoutTalent();
        }

        if (CooldownTime != _baseCooldownTime)
            CooldownTime = _baseCooldownTime;

        yield return _waitingHitFromCreeperStrike = StartCoroutine(WaitingHitFromCreeperStrikeJob());
    }

    public void ReducingAbilityCooldown()
    { 
        if (RemainingCooldownTime > 0)
        {
            float reducingMultiplier = _reducingCooldownMultiplier;
            float newCooldownTime = RemainingCooldownTime / reducingMultiplier;
            ReductionSetCooldown(newCooldownTime);
        }
        else
        {
            float reducingMultiplier = _reducingCooldownMultiplier;
            CooldownTime /= reducingMultiplier;
        }
    }

    private IEnumerator WaitingHitFromCreeperStrikeJob()
    {
        while (!_creeperStrike.IsHit)
        {
            yield return null;    
        }
    }

    private void UseAbilityWithTalent()
    {
        if (_isPlayer)
        {
            ReductionSetCooldown(_cooldownTimeWithTalent);
            Debug.Log("ColdBlood / UseAbilityWithTalent / if _isPlayer == true");
			Hero.CharacterState.DispelStates(StateType.Physical, GetTargetCharacter().NetworkSettings.TeamIndex, Hero.NetworkSettings.TeamIndex, true);
        }
        else
        {
            Debug.Log("ColdBlood / UseAbilityWithTalent / else if _isPlayer == false");
            if (_killersStamina.Data.IsOpen)
            {
                _isCanCritLightningStrikes = true;
            }

            _isCanCritCreeperStrike = true;
        }
    }

    private void UseAbilityWithoutTalent()
    {
        Debug.Log("ColdBlood / UseAbilityWithoutTalent");
        if (_killersStamina.Data.IsOpen)
        {
            _isCanCritLightningStrikes = true;
        }

        _isCanCritCreeperStrike = true;
    }
}
