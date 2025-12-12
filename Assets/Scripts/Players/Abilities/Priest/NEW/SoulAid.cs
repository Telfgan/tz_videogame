using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class SoulAid : Skill
{
    [SerializeField] private float _speed = 0.0025f;
    [SerializeField] private float _cooldownReduceValue = 5f;
    [SerializeField] private float _defaultRadius = 4f;
    [SerializeField] private float _largeRadius = 8f;
    [SerializeField] private PriestShield _priestShield;
    [SerializeField] private Restoration _restoration;
    
    //private Character _target;
    private GameObject _tempTarget;
    private MoveComponent _tempTargetMove;
    
    private bool _talentTiredSoulDispelActive = false;
    private bool _talentCooldownReduce = false;
    private bool _talentDoubleRange = false;

    private void OnEnable()
    {
        CastEnded += DispelTiredSoul;
        _priestShield.CastEnded += ReduceCooldown;
    }

    private void OnDisable()
    {
        CastEnded -= DispelTiredSoul;
        _priestShield.CastEnded -= ReduceCooldown;
    }

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => 0;

    protected override bool IsCanCast
    {
        get
        {
            var isTargetInRadius = IsTargetInRadius(_defaultRadius, GetTargetCharacter().transform) || IsTargetHaveRestoration() && IsTargetInRadius(_largeRadius, GetTargetCharacter().transform);
            return isTargetInRadius && IsTargetHaveTiredSoul();
        }
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        SetTarget((Character)targetInfo.GetTargets()[0]);
    }

    protected override IEnumerator CastJob()
    {
        if (GetTargetCharacter() == null || GetTargetCharacter() == Hero || !IsCanCast) yield break;
        
        while (Vector2.Distance(transform.position, GetTargetCharacter().transform.position) > 2.1f)
        {
            Vector2 direction = (transform.position - GetTargetCharacter().transform.position).normalized;
            Vector2 pullForce = direction * (_speed * Time.fixedTime);

            CmdPull(GetTargetCharacter().gameObject, pullForce);
            yield return new WaitForFixedUpdate();
        }
    }

    protected override void ClearData()
    {
        ClearTarget();
        //_target = null;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        while (GetTargetCharacter() == null)
        {
            Radius = _talentDoubleRange ? _largeRadius : _defaultRadius;
            
            if (GetMouseButton)
            {
                FindTargetCharacter();
               // _target = GetRaycastTarget(_talentTiredSoulDispelActive);
            }
            yield return null;
        }
        TargetInfo targetInfo = new TargetInfo();
        targetInfo.AddTarget(GetTargetCharacter());
        callbackDataSaved(targetInfo);
    }

    public void EnableTiredSoulDispel(bool isActive)
    {
        _talentTiredSoulDispelActive = isActive;
    }
    
    public void EnableCooldownReduce(bool isActive)
    {
        _talentCooldownReduce = isActive;
    }

    public void EnableDoubleRange(bool isActive)
    {
        _talentDoubleRange = isActive;
    }

    private bool IsTargetHaveTiredSoul()
    {
        return GetTargetCharacter() != null && GetTargetCharacter().CharacterState.CheckForState(States.TiredSoul);
    }

    private bool IsTargetHaveRestoration()
    {
        if (!_talentDoubleRange || GetTargetCharacter() == null || _restoration.Target == null) return false;
        
        return _restoration.Target == GetTargetCharacter();
    }

    private void DispelTiredSoul()
    {
        if(!IsTargetHaveTiredSoul()) return;
        
        CmdRemoveBuff(States.TiredSoul, GetTargetCharacter().gameObject); 
    }

    private void ReduceCooldown()
    {
        if(!_talentCooldownReduce) 
            return;
        
        DecreaseSetCooldown(_cooldownReduceValue);
    }

    [Command]
    private void CmdPull(GameObject gameObject, Vector2 force)
    {
        if (_tempTarget != gameObject)
        {
            _tempTarget = gameObject;
            _tempTargetMove = gameObject.GetComponent<MoveComponent>();
        }
        _tempTargetMove.TargetRpcAddTransformPosition(force);
    }
    
    [Command]
    private void CmdRemoveBuff(States state, GameObject target)
    {
        var characterState = target.GetComponent<CharacterState>();
        characterState.RemoveState(state);
    }
}
