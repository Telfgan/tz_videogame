using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class SkillEnergyCost
{
    public ResourceType resourceType;
    public float resourceCost;

    public void ModifyResourceCost(float multiplier)
    {
        resourceCost *= multiplier;
    }

    public void ModifyResourceCost1(float multiplier)
    {
        resourceCost /= multiplier;
    }

    public void ResetResourceCost(float baseCost)
    {
        resourceCost = baseCost;
    }
}
/*
public class TargetToShot
{
    public Vector3 Position;
    public ITargetable targetable;

    public Character character;
    public IDamageable damageable;
    public bool isCharater = false;
}*/

public enum Schools
{
    Light,
    Dark,
    Fire,
    Water,
    Air,
    Earth,
    Physical,
    Discipline,
    None
}

public enum AbilityForm
{
    Spell,
    Magic,
    Physical,
    Both,
    Passiv,
}

public enum SkillType
{
    Target,
    Projectile,
    Zone,
    NonTarget
}

public enum Moving
{
    Static,
    NonStatic
}

public enum AutoAttack
{
    autoAttack,
    nonAutoAttack
}

public enum DamageType
{
    Magical,
    Physical,
    DOTPhys,
    DOTMag,
    Both,
    None
}

public enum AttackRangeType
{
    MeleeAttack,
    RangeAttack,
}

public abstract class Skill : NetworkBehaviour
{
    [Header("Talent State")]
    [SerializeField] protected bool _isTalentSpell = false;
    [SerializeField] protected bool _isSkillActive = true;

    [Header("AbilitiesInfo")]
    [SerializeField] private AbilityInfo _abilityInfo;
    [Header("Main Settings")]
    [NonSerialized] public float ExtraAnimationSpeedMultiplier = 1f; // test
    [SerializeField] protected bool _isSubjectToGlobalCooldownTime = true;

    [SerializeField] protected List<SkillEnergyCost> _skillEnergyCosts;
    [SerializeField] protected List<SkillEnergyCost> _additionalSkillEnergyCosts;
    [SerializeField] protected float _cooldownTime;
    [SerializeField] protected float _castDeley;
    [SerializeField] protected float _damageValue;
    [SerializeField] private Schools _abilitySchool;
    [SerializeField] private AbilityForm _abilityForm;
    [SerializeField] private DamageType _damageType;
    [SerializeField] private AttackRangeType _attackRangeType;
    [SerializeField] private SkillType _skillType;
    [SerializeField] private Moving _moving;
    [SerializeField] private AutoAttack autoAttack;
    [SerializeField] protected LayerMask _targetsLayers;
    [SerializeField] protected LayerMask _obstacle;
    [Header("Streaming settings")]
    [SerializeField] protected float _castDuration;
    [SerializeField] protected float _manaCostRate;
    [SerializeField] protected List<SkillEnergyCost> _manaCostPerTick;
    [Header("Charge settings")]
    [SerializeField] private bool _isUseCharges;
    [SerializeField] protected bool _useChargesAsComboPart = false; // test
    [SerializeField] protected bool _chargesHaveSeparateCooldown;
    [SerializeField] protected int _maxCharges;
    [SerializeField] protected float _chargeCooldown;
    [Header("Area settings")]
    [SerializeField] protected float _radius;
    [SerializeField] protected float _area;
    [SerializeField] protected float _castLength;
    [SerializeField] protected float _castWidth;
    [Header("Area settings")]
    [SerializeField] protected float _autoAttackDelay;
    [Header("Render settings")]
    [SerializeField] protected bool _isAutoRadiusRender = true;
    [SerializeField] protected bool _isAutoAreaRender = true;
    [SerializeField] protected bool _isAutoLineRender = true;
    [SerializeField] protected bool _isDynamicRenderer = false;
    [Header("Availability")]
    [SerializeField] protected bool _disactive = false;
    [SerializeField] protected bool _earlyCooldown = false;
    [Header("Counter settings")]
    [SerializeField] protected float maxCounter;


    protected SkillRenderer _skillRender;
    protected Character _hero;
    protected bool _isCanCancle = true;
    protected Coroutine _prepareCoroutine;
    protected Coroutine _castCoroutine;
    protected Coroutine _cooldownJob;
    protected Coroutine _rechargeJob;
    protected Coroutine _castDeleyCoroutine;
    protected Coroutine _castStreamCoroutine;
    protected Coroutine _dynamicRendererJob;
    protected Transform _tempTargetForDamage;
    protected IDamageable _tempForDamage;
    protected IHealingable _tempForHealing;
    protected bool _isPlayCastAnim;
    protected int _currentChargers;
    protected float _baseCooldownTime;
    //test counter
    protected float _currentCounter;

    private Character _tempTargetbase;
    private float _remainingCooldownTime;
    private StatsBuff _statsBuff = new StatsBuff();
    private Coroutine _actionWrapperForPreparingCoroutine;
    private Coroutine _actionWrapperForCastCoroutine;
    private bool _isPreparing = false;
    private bool _isCasting = false;
    private TypeClick _click;
    private List<float> _remainingCooldownTimeChargers = new();
    private List<Coroutine> _currentChargeCooldownJob;
    private Queue<TargetInfo> _targetInfoQueue = new();
    private bool _isAutoMode;
    private ITargetable _target;
    private ITargetable _tempTarget;



    public bool IsAutoMode
    {
        get
        {
            return _isAutoMode;
        }
        set
        {
            if (_isAutoMode != value)
            {
                _isAutoMode = value;
                AutoModeChanged?.Invoke(_isAutoMode);
            }
        }
    }
    public bool IsTalentSpell => _isTalentSpell;
    public bool IsSkillActive
    {
        get => _isSkillActive;
        set => _isSkillActive = value;
    }
    public Transform TempTargetForDamage => _tempTargetForDamage;
    public bool GetMouseButton { get => _click != TypeClick.None; }
    public bool IsSubjectToGlobalCooldownTime { get => _isSubjectToGlobalCooldownTime; }
    public Character Hero { get => _hero; }
    public StatsBuff Buff => _statsBuff;
    public string Name => _abilityInfo.Name;
    public string Description { get => _abilityInfo.FinalDescription; set => _abilityInfo.FinalDescription = value; }
    public string State => _abilityInfo.State; // test: we output the name of the state
    public string DescriptionState => _abilityInfo.DescriptionState; // test: we output a description of the state
    public string CounterSkill => _abilityInfo.Counter; // test: the counter is in the ability
    public Sprite Icon => _abilityInfo.Icon;
    public AbilityInfo AbilityInfoHero { get => _abilityInfo; set => _abilityInfo = value; }
    public bool IsCooldowned { get => _remainingCooldownTime <= 0; }
    public virtual bool IsPayCostStartCooldown { get => true; }
    public int Chargers { get => _currentChargers; protected set { _currentChargers = value; CurrentChargeChanged?.Invoke(_currentChargers); } }
    public int MaxChargers { get => _maxCharges; set => _maxCharges = value; }
    public bool IsHaveCharge => (_currentChargers > 0);
    public float ChargeCooldown => _chargeCooldown;
    public List<float> RemainingCooldownTimeCharge { get => _remainingCooldownTimeChargers; }
    public bool IsPreparing => _isPreparing;
    public SkillRenderer SkillRender => _skillRender;
    public bool IsHaveResourceOnSkill { get => CheckResourcesOnSkill(); }
    public bool IsHaveResources { get => IsHaveResourceOnSkill && IsCooldowned && IsHaveCharge; }
    public float CooldownTime { get => Buff.Cooldown.GetBuffedValue(_cooldownTime); protected set => _cooldownTime = value; }
    public float RemainingCooldownTime { get => _remainingCooldownTime; set => _remainingCooldownTime = value; }
    public float CastDeley { get => Buff.CastSpeed.GetBuffedValue(_castDeley); set => _castDeley = value; }
    public bool IsCasting { get => _isCasting; protected set => _isCasting = value; }
    public float CastStreamDuration { get => _castDuration; set => _castDuration = value; }
    public float Radius { get => Buff.Radius.GetBuffedValue(_radius); set => _radius = value; }
    public float Area { get => Buff.Area.GetBuffedValue(_area); set => _area = value; }
    public float CastLength { get => Buff.Area.GetBuffedValue(_castLength); protected set => _castLength = value; }
    public float CastWidth { get => Buff.Area.GetBuffedValue(_castWidth); protected set => _castWidth = value; }
    public float MaxCounter { get => maxCounter; set => maxCounter = value; }
    public float CurrentCounter { get => _currentCounter; set => _currentCounter = value; }
    public virtual float Damage { get => _damageValue; set => _damageValue = value; }
    public bool IsUseCharges { get => _isUseCharges; }
    public LayerMask TargetsLayers { get => _targetsLayers; protected set => _targetsLayers = value; }
    public Schools School { get => _abilitySchool; protected set => _abilitySchool = value; }
    public AbilityForm AbilityForm => _abilityForm;
    public DamageType DamageType => _damageType;
    public AttackRangeType AttackRangeType => _attackRangeType;
    public SkillType SkillType => _skillType;
    public Moving Moving => _moving;
    public AutoAttack AutoAttack => autoAttack;
    public List<SkillEnergyCost> SkillEnergyCosts { get => _skillEnergyCosts; }
    public List<SkillEnergyCost> AdditionalSkillEnergyCosts { get => _additionalSkillEnergyCosts; }
    public List<SkillEnergyCost> ManaCostPerTick { get => _manaCostPerTick; }
    public float ManaCostRate { get => _manaCostRate; }
    public float AutoAttackDelay { get => _autoAttackDelay; }
    public Queue<TargetInfo> TargetInfoQueue { get => _targetInfoQueue; }
    public ChargeCDUI LinkedChargeCDUI { get; set; }

    public bool Disactive
    {
        get => _disactive;
        set
        {
            if (_disactive != value)
            {
                _disactive = value;
                OnSkillStateChanged?.Invoke(_disactive);
            }
        }
    }

    public event Action<bool> OnSkillStateChanged;
    public event Action<int> CurrentChargeChanged;
    public event Action<float> CooldownStarted;
    public event Action<float> ChargeStartCooldown;
    public event Action<int> ChargeCooldownEnded;
    public event Action CooldownEnded;
    public event Action<Skill> PreparingStarted;
    public event Action<Skill> PreparingSuccess;
    public event Action PreparingCanceled;
    public event Action<float> CastDeleyStarted;
    public event Action CastDeleyEnded;
    public event Action<float> CastStreamStarted;
    public event Action CastStreamEnded;
    public event Action CastStarted;
    public event Action CastEnded;
    public event Action Canceled;
    public event Action<float> MassageHaventMana;
    public event Action MassageHaventCharge;
    public event Action<float> MassageNotCooldowned;
    public event Action OnSkillCanceled;
    public event Action CastSuccess;
    public event Action<TargetInfo> TargetDataSaved;
    public event Action<bool> AutoModeChanged;
    public event Action<Vector3> ClickPoint;
    public event Action BoostEnabled;
    public event Action BoostDisabled;
    public event Action AfterCast;
    public int AnimTriggerCastPublic => AnimTriggerCast;

    /// <summary>
    /// There may be a description that will be shown in the AbillityNameBox.
    /// </summary>
    public virtual string AdditionalDescription { get; }
    protected abstract int AnimTriggerCastDelay { get; }
    protected abstract int AnimTriggerCast { get; }

    protected void RaiseCooldownStarted(float cooldownTime) => CooldownStarted?.Invoke(cooldownTime);
    protected void RaiseCooldownEnded() => CooldownEnded?.Invoke();
    protected void SkillAfterCastJob() => AfterCast?.Invoke();
    protected void CastEndedJob() => CastEnded?.Invoke();

    protected virtual bool IsCanCast
    {
        get
        {
            _targetInfoQueue.TryPeek(out TargetInfo temp);

            if (temp == null)
                return true;

            switch (SkillType)
            {
                case SkillType.Target:

                    if (temp.GetTargets().Count > 0)
                    {
                        foreach (var target in temp.GetTargets())
                            if (Vector3.Distance(target.Position, transform.position) > Radius)
                                return false;

                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case SkillType.Projectile:

                    if (temp.GetTargets().Count > 0)
                    {
                        foreach (var target in temp.GetTargets())
                            if (Vector3.Distance(target.Position, transform.position) > Radius)
                                return false;

                        return true;
                    }
                    else
                    {
                        return true;
                    }

                case SkillType.Zone:

                    if (temp.Points.Count > 0)
                    {
                        foreach (var point in temp.Points)
                            if (Vector3.Distance(point, transform.position) > Radius)
                                return false;

                        return true;
                    }
                    else if (temp.GetTargets().Count > 0)
                    {
                        foreach (var target in temp.GetTargets())
                            if (Vector3.Distance(target.Position, transform.position) > Radius)
                                return false;

                        return true;
                    }
                    else
                    {
                        return true;
                    }

                case SkillType.NonTarget:

                    return true;

                default:

                    return true;
            }
        }
    }

    public abstract void LoadTargetData(TargetInfo targetInfo);
    protected abstract IEnumerator PrepareJob(Action<TargetInfo> targetDataSavedCallback);
    protected abstract IEnumerator CastJob();
    protected abstract void ClearData();

    protected virtual void SkillEnableBoostLogic() { }

    protected virtual void SkillDisableBoostLogic() { }

    public void Init(SkillRenderer render, Character hero)
    {
        _hero = hero;
        _skillRender = render;
    }

    protected virtual void Awake()
    {
        if (_isUseCharges)
        {
            _currentChargers = _maxCharges;
            _remainingCooldownTimeChargers = new List<float>(new float[_maxCharges]);
            _currentChargeCooldownJob = new List<Coroutine>(new Coroutine[_maxCharges]);
        }
        else
            _currentChargers = 1;
    }

    public ITargetable GetTarget(bool canGetDead = false)
    {
        if (_target != null)
        {
            if (!_target.IsTargetable && !canGetDead) return null;

            return _target;
        }
        return null;
    }

    public Character GetTargetCharacter(bool canGetDead = false)
    {
        if (_target != null)
        {
            if (!_target.IsTargetable && !canGetDead) return null;

            return (Character)_target;
        }
        return null;
    }

    public Character GetTempTargetCharacter(bool canGetDead = false)
    {
        if (_tempTarget != null)
        {
            if (!_tempTarget.IsTargetable && !canGetDead) return null;

            return (Character)_tempTarget;
        }
        return null;
    }

    public void SetTargetCharacter(Character character)
    {
        _target = character;
    }

    public void SetTarget(ITargetable character)
    {
        if (character == null)
            return;

        _target = character;
    }

    public void ClearTempTarget()
    {
        _tempTarget = null;
    }

    protected void FindTarget(bool canTargetHimself = false, bool canTargetDead = false)
    {
        if (canTargetDead)
        {
            if (GetCloserTargets(GetMousePoint(), Radius, canTargetHimself) != null)
                _tempTarget = GetCloserTargets(GetMousePoint(), Radius, canTargetHimself)[0];
        }
        else
        {
            //Debug.Log()
            if (GetCloserTargets(GetMousePoint(), Radius, canTargetHimself) != null)
                _tempTarget = GetCloserTargets(GetMousePoint(), Radius, canTargetHimself).FirstOrDefault(target => target.IsTargetable);
        }
    }

    protected void FindTargetCharacter(bool canTargetHimself = false, bool canTargetDead = false)
    {
        if (canTargetDead)
        {
            if (GetCloserTargets(GetMousePoint(), Radius, canTargetHimself) != null)
                _tempTarget = GetCloserTargetsCharacter(GetMousePoint(), Radius, canTargetHimself)[0];
        }
        else
        {
            if (GetCloserTargets(GetMousePoint(), Radius, canTargetHimself) != null)
                _tempTarget = GetCloserTargetsCharacter(GetMousePoint(), Radius, canTargetHimself).FirstOrDefault(target => target.IsTargetable);
        }
    }

    public void ClearTarget()
    {
        _target = null;
    }

    public void EnableSkillBoost()
    {
        SkillEnableBoostLogic();
        BoostEnabled?.Invoke();
    }

    public void DisableSkillBoost()
    {
        SkillDisableBoostLogic();
        BoostDisabled?.Invoke();
    }

    public void InvokeCastStreamStarted(float duration)
    {
        CastStreamStarted?.Invoke(duration);
    }

    public bool TryPreparing()
    {
        if (_isPreparing == false)
        {
            foreach (var skillCost in _skillEnergyCosts)
            {
                //var currentResourceValue = _hero.Resources.Where(r => r.Type == skillCost.resourceType);
                var resource = _hero.Resources.First(r => r.Type == skillCost.resourceType);
                resource.PhantomValueShow(skillCost.resourceCost);
            }
            _actionWrapperForPreparingCoroutine = StartCoroutine(ActionWrapperForPreparingJob());
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool TryCast()
    {
        if (_isCasting || _isPreparing)
            return false;

        LoadTargetDataForCheckCast();
        if (IsHaveResources && IsCanCast && _isCasting == false && NoObstacles() && Hero.IsDead == false)
        {
            _isCasting = true;
            TryPayCost(IsPayCostStartCooldown);

            if (_targetInfoQueue.Count > 0)
            {
                var targetInfo = _targetInfoQueue.Dequeue();

                LoadTargetData(targetInfo);

                if (targetInfo.GetTargets().Count > 0)
                {
                    var target = (Character)targetInfo.GetTargets()[0];
                    _hero.Move.LookAtTransform(target.transform);
                }

                if (targetInfo.Points.Count > 0)
                {
                    var point = (Vector3)targetInfo.Points[0];
                    _hero.Move.LookAtPosition(point);
                }
            }


            _actionWrapperForCastCoroutine = StartCoroutine(ActionWrapperForCastingJob());

            return true;
        }

        else return false;
    }

    public bool TryCast(TargetInfo targetInfo)
    {
        if (_isCasting || _isPreparing)
            return false;

        LoadTargetDataForCheckCast();
        if (IsHaveResources && _isCasting == false && NoObstacles() && Hero.IsDead == false)
        {
            _isCasting = true;
            LoadTargetData(targetInfo);

            if (IsCanCast)
            {
                TryPayCost(IsPayCostStartCooldown);

                _actionWrapperForCastCoroutine = StartCoroutine(ActionWrapperForCastingJob());

                if (_targetInfoQueue.Count > 0)
                {
                    if (targetInfo.GetTargets().Count > 0)
                    {
                        var target = (Character)targetInfo.GetTargets()[0];
                        _hero.Move.LookAtTransform(target.transform);
                    }

                    if (targetInfo.Points.Count > 0)
                    {
                        var point = (Vector3)targetInfo.Points[0];
                        _hero.Move.LookAtPosition(point);
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public bool TryCancel(bool foceCancel = false)
    {
        foreach (var skillCost in _skillEnergyCosts)
        {
            //var currentResourceValue = _hero.Resources.Where(r => r.Type == skillCost.resourceType);
            var resource = _hero.Resources.First(r => r.Type == skillCost.resourceType);
            resource.PhantomValueShow(0);
            //resourse.
        }

        if (foceCancel || _isCanCancle)
        {
            Canceled?.Invoke();
            if (_isAutoMode) _hero.Move.CanMove = true;
            ClearData();
            _isPlayCastAnim = false;

            if (_dynamicRendererJob != null)
            {
                StopCoroutine(_dynamicRendererJob);
            }
            CancelCoroutine(_castCoroutine);

            if (_actionWrapperForCastCoroutine != null)
            {
                StopCoroutine(_actionWrapperForCastCoroutine);
                CancelCoroutine(_castCoroutine);
                _actionWrapperForCastCoroutine = null;
                _isCasting = false;
                ClearData();

                CastEnded?.Invoke();
            }

            CancelCoroutine(_castDeleyCoroutine);
            CancelCoroutine(_castStreamCoroutine);

            if (_actionWrapperForPreparingCoroutine != null)
            {
                StopCoroutine(_actionWrapperForPreparingCoroutine);
                CancelCoroutine(_prepareCoroutine);
                _actionWrapperForPreparingCoroutine = null;
                _isPreparing = false;
                StopAutoDraw();

                PreparingCanceled?.Invoke();

                UnSubscribeClickEvents();
                OnClickCanceled();
            }

            _tempTargetbase = null;

            _hero.Animator.SetTrigger(HashAnimPlayer.AnimCancled);
            _hero.NetworkAnimator.SetTrigger(HashAnimPlayer.AnimCancled);
            OnSkillCanceled?.Invoke();

            return true;
        }
        else
        {
            return false;
        }
    }

    public void IncreaseSetCooldown(float time)
    {
        if (time < _remainingCooldownTime)
            return;

        if (_cooldownJob != null)
            StopCoroutine(_cooldownJob);

        _cooldownJob = StartCoroutine(CooldownCoroutine(time));
    }

    public void IncreaseSetCooldownPassive(float time)
    {
        if (_cooldownJob != null) StopCoroutine(_cooldownJob);
        _cooldownJob = StartCoroutine(CooldownCoroutine(time));
    }

    public void DecreaseSetCooldown(float time)
    {
        var timeToSet = _remainingCooldownTime - time > 0 ? _remainingCooldownTime - time : 0;

        if (_cooldownJob != null)
            StopCoroutine(_cooldownJob);

        _cooldownJob = StartCoroutine(CooldownCoroutine(timeToSet));
    }

    public void ResetCooldown()
    {
        if (_cooldownJob != null)
        {
            StopCoroutine(_cooldownJob);
            _cooldownJob = null;
        }

        _remainingCooldownTime = 0;

        CooldownEnded?.Invoke();
    }

    public void ReductionSetCooldown(float time)
    {
        if (time > _remainingCooldownTime)
            return;

        if (_cooldownJob != null)
            StopCoroutine(_cooldownJob);

        _cooldownJob = StartCoroutine(CooldownCoroutine(time));
    }

    public void ResetCurrentChargeCooldown(int index)
    {
        if (!_isUseCharges || !_chargesHaveSeparateCooldown) return;
        if (_currentChargeCooldownJob[index] != null)
        {
            StopCoroutine(_currentChargeCooldownJob[index]);
            _currentChargeCooldownJob[index] = null;
        }

        _remainingCooldownTimeChargers[index] = 0f;

        LinkedChargeCDUI?.RemoveChargeCD(index);

        _currentChargers = Mathf.Min(_currentChargers + 1, _maxCharges);
        CurrentChargeChanged?.Invoke(_currentChargers);

        ChargeCooldownEnded?.Invoke(index);
    }

    public void CheckResources()
    {
        foreach (var skillCost in _skillEnergyCosts)
        {
            var currentResourceValue = _hero.Resources.Where(r => r.Type == skillCost.resourceType).Sum(r => r.CurrentValue);

            if (currentResourceValue < Buff.ManaCost.GetBuffedValue(skillCost.resourceCost))
            {
                float shortage = Buff.ManaCost.GetBuffedValue(skillCost.resourceCost) - currentResourceValue;

                switch (skillCost.resourceType)
                {
                    case ResourceType.Health:
                        MassageHaventMana?.Invoke(shortage);
                        break;
                    case ResourceType.Mana:
                        MassageHaventMana?.Invoke(shortage);
                        break;
                    case ResourceType.Energy:
                        MassageHaventMana?.Invoke(shortage);
                        break;
                    case ResourceType.Rune:
                        MassageHaventMana?.Invoke(shortage);
                        break;
                    default:
                        break;
                }
            }
        }

        if (IsCooldowned == false)
            MassageNotCooldowned?.Invoke(_remainingCooldownTime);

        if (IsHaveCharge == false)
            MassageHaventCharge?.Invoke();
    }

    private bool CheckResourcesOnSkill()
    {
        return _skillEnergyCosts.All(skillCost =>
            _hero.Resources.Where(r => r.Type == skillCost.resourceType).Sum(r => r.CurrentValue) >= Buff.ManaCost.GetBuffedValue(skillCost.resourceCost));
    }

    public void AddMaxChargeCount()
    {
        _maxCharges += 1;

        _remainingCooldownTimeChargers.Add(0);

        _currentChargers += 1;

        CurrentChargeChanged?.Invoke(_currentChargers);
    }

    public void ReductionCooldownForAllCharges(float reductionTime, float reductionPercentage = 0)
    {
        for (int i = 0; i < RemainingCooldownTimeCharge.Count; i++)
        {
            var time = _remainingCooldownTimeChargers[i] - reductionTime - (_remainingCooldownTimeChargers[i] * reductionPercentage);
            ReductionCooldownForCharge(i, time);
        }
    }

    public void ReductionCooldownCharges(float reductionTime)
    {
        float time;
        for (int i = 0; i < RemainingCooldownTimeCharge.Count; i++)
        {
            time = _remainingCooldownTimeChargers[i] - reductionTime;

            if (time <= 0)
            {
                ReductionCooldownForCharge(i, reductionTime);
                reductionTime = reductionTime - _remainingCooldownTimeChargers[i];
            }
            else
            {
                ReductionCooldownForCharge(i, reductionTime);
                break;
            }
        }
    }

    public void DeductMaxChargeCount()
    {
        if (_maxCharges - 1 > 0)
        {
            _maxCharges -= 1;

            _remainingCooldownTimeChargers.RemoveAt(_remainingCooldownTimeChargers.Count - 1);

            if (_currentChargers > _maxCharges)
            {
                _currentChargers -= 1;
                CurrentChargeChanged?.Invoke(_currentChargers);
            }
        }
    }

    public void DrawDamageZone(Vector3 position)
    {
        Damage damage = new Damage
        {
            Value = Damage,
            Type = DamageType,
        };
        _skillRender.CmdDrawDamageZone(position, Area, damage, _hero.gameObject);
    }

    public void StopDamageZone()
    {
        _skillRender.CmdRemoveNextDamageZone();
    }

    [ClientCallback]
    protected void AnimStartCastCoroutine()
    {
        _castCoroutine = StartCoroutine(CastJob());
        if (_castDuration > 0) _castStreamCoroutine = StartCoroutine(CastStreamJob());
    }

    protected virtual void AnimCastEnded()
    {
        _isPlayCastAnim = false;
    }

    protected virtual IEnumerator DynamicRendererJob(float time = 0.2f)
    {
        yield return null; //new WaitForSeconds(time);
    }

    protected virtual void StartAutoDraw()
    {
        Damage damage = new Damage
        {
            Value = Damage,
            Type = DamageType,
        };

        if (_isAutoRadiusRender)
            _skillRender.DrawRadius(Radius);

        if (_isAutoAreaRender)
        {
            _skillRender.DrawArea(Area, damage, TargetsLayers);
            _skillRender.StartDynamicRadiusColor(Radius);
        }

        _skillRender.StartPreview(Area, damage, TargetsLayers);

        if (_isAutoLineRender)
            _skillRender.DrawLine(CastLength, CastWidth, damage, TargetsLayers);

        if (_skillType == SkillType.Target)
        {
            /* Debug.Log("DRAAAAAAAAAAW");
             Character enemy = GetCloserTargets(transform.position, Radius)[0];
             Debug.Log(enemy.name);
             enemy.SelectedCircle.IsActive = true;*/
            _skillRender.DrawClosestTarget(Radius, TargetsLayers, _hero);
        }

        if (_skillType == SkillType.Zone)
        {
            _skillRender.StartDrawLineForZone(this);
        }
    }

    protected virtual void StopAutoDrawRadius() => _skillRender.StopDrawRadius();

    protected virtual void StopAutoDraw()
    {
        _skillRender.ResetCursor();
        _skillRender.StopDrawRadius();
        _skillRender.StopDrawArea();
        _skillRender.StopDrawLine();
        _skillRender.StopDrawClosestTarget();
        _skillRender.StopDynamicRadiusColor();

        _skillRender.StopPreview();

        if (_skillType == SkillType.Zone)
        {
            _skillRender.StopDrawLineForZone();
        }


        /*if (true)
		{
			Character enemy = GetCloserTargets(transform.position, Radius)[0];
			enemy.SelectedCircle.IsActive = false;
		}*/
    }

    protected virtual bool TryPayCost(List<SkillEnergyCost> skillEnergyCosts, bool startCooldown = true)
    {
        if (IsHaveResourceOnSkill)
        {
            foreach (var skillCost in skillEnergyCosts)
            {
                var resource = _hero.Resources.First(r => r.Type == skillCost.resourceType);
                resource.CmdUse(Buff.ManaCost.GetBuffedValue(skillCost.resourceCost));
            }

            if (startCooldown)
                IncreaseSetCooldown(CooldownTime);

            if (!_useChargesAsComboPart) TryUseCharge();
            return true;
        }
        else
        {
            return false;
        }
    }

    protected virtual bool TryPayCost(bool startCooldown = true)
    {
        if (_hero.Abilities.TryConsumeNextSkillFree()) return true;
        return TryPayCost(_skillEnergyCosts, startCooldown);
    }

    /* protected IDamageable GetRaycastTarget(bool isCanTargetHimself = false)
     {
         return _hero.TargetSeeker.GetRaycastTarget(this, isCanTargetHimself);
     }*/

    public List<ITargetable> GetCloserTargets(Vector3 position, float radius, bool isCanTargetHimself = false)
    {
        return _hero.TargetSeeker.GetCloserTargets(position, radius, isCanTargetHimself);
    }

    public List<Character> GetCloserTargetsCharacter(Vector3 position, float radius, bool isCanTargetHimself = false)
    {
        return _hero.TargetSeeker.GetCloserTargetsCharacter(position, radius, isCanTargetHimself);
    }

    protected bool IsTargetInRadius(float radius, Transform target)
    {
        if (target == null)
            return false;

        float distance = Vector3.Distance(target.position, transform.position);
        return distance <= radius;
    }
    protected bool IsPointInRadius(float radius, Vector3 point)
    {
        float distance = Vector3.Distance(point, transform.position);
        return distance <= radius;
    }

    protected bool NoObstacles(Vector3 target, Vector3 point, LayerMask obstacle)
    {
        if (target == Vector3.zero)
            return true;

        var vector = (target - point);
        var dir = vector.normalized;
        float distance = vector.magnitude;

        RaycastHit[] rayHit = Physics.RaycastAll(point, dir, distance, obstacle);

        if (rayHit.Length > 0)
            return false;
        else
            return true;
    }

    protected bool NoObstacles(Vector3 target, LayerMask obstacle)
    {
        return NoObstacles(target, transform.position, obstacle);
    }

    protected bool NoObstacles()
    {
        if (_tempTargetbase != null)
            return NoObstacles(_tempTargetbase.transform.position, transform.position, _obstacle);

        return true;
    }

    protected Coroutine StartCastDeleyCoroutine()
    {
        _castDeleyCoroutine = StartCoroutine(CastDeleyJob(CastDeley));
        return _castDeleyCoroutine;
    }

    protected Coroutine StartCastDeleyCoroutine(float time)
    {
        _castDeleyCoroutine = StartCoroutine(CastDeleyJob(time));
        return _castDeleyCoroutine;
    }

    protected void CancelCoroutine(Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }

    protected bool IsMouseInRadius(float radius)
    {
        float distance = Vector3.Distance(GetMousePoint(), transform.position);

        return distance <= radius;
    }

    public Vector3 GetMousePoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (autoAttack == AutoAttack.autoAttack)
            {
                if (UnityEngine.InputSystem.Keyboard.current.leftCtrlKey.isPressed)
                {
                    if (hit.collider.TryGetComponent<IDamageable>(out _))
                    {

                        IsAutoMode = true;
                        AutoModeChanged?.Invoke(true);
                    }
                }
            }

            return hit.point;
        }
        return Vector3.zero;
    }

    /*protected TargetToShot GetTarget(bool isCanTargetHimself = false)
	{
		return _hero.TargetSeeker.GetTarget(_click, ClickPoint, _skillType, Radius, this, isCanTargetHimself);
	}*/

    protected ITargetable ClosedTarget(bool isCanTargetHimself = false)
    {
        return _hero.TargetSeeker.ClosedTarget(isCanTargetHimself);
    }
    protected Character ClosedTargetCharacter(bool isCanTargetHimself = false)
    {
        return _hero.TargetSeeker.ClosedTargetCharacter(isCanTargetHimself);
    }

    public bool TryUseCharge()
    {
        if (_isUseCharges == false)
            return true;

        if (_currentChargers > 0)
        {
            _currentChargers--;
            CurrentChargeChanged?.Invoke(_currentChargers);

            if (_rechargeJob == null && _chargesHaveSeparateCooldown == false)
            {
                _rechargeJob = StartCoroutine(RechargeCoroutine());
            }
            else if (_rechargeJob == null && _chargesHaveSeparateCooldown)
            {
                for (int i = 0; i < _maxCharges; i++)
                {
                    if (_remainingCooldownTimeChargers[i] <= 0)
                    {
                        _currentChargeCooldownJob[i] = StartCoroutine(RechargeOneChargeCoroutine(i, ChargeCooldown));

                        ChargeStartCooldown?.Invoke(ChargeCooldown);
                        break;
                    }
                }
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    private IEnumerator RechargeOneChargeCoroutine(int chargeIndex, float time)
    {
        _remainingCooldownTimeChargers[chargeIndex] = time;

        while (_remainingCooldownTimeChargers[chargeIndex] > 0)
        {
            _remainingCooldownTimeChargers[chargeIndex] -= Time.deltaTime;

            yield return null;
        }

        if (_currentChargers < _maxCharges)
        {
            _currentChargers++;
            CurrentChargeChanged?.Invoke(_currentChargers);
        }

    }

    protected virtual IEnumerator RechargeCoroutine()
    {
        while (_currentChargers < _maxCharges)
        {
            ChargeStartCooldown?.Invoke(ChargeCooldown);
            float time = 0;
            while (time < ChargeCooldown)
            {
                time += Time.deltaTime;
                yield return null;
            }
            if (_currentChargers < _maxCharges)
            {
                _currentChargers++;
                CurrentChargeChanged?.Invoke(_currentChargers);
            }
        }
        _rechargeJob = null;
    }

    private bool IsValidTarget(IDamageable target)
    {
        if (target == null) return false;
        if (target is MonoBehaviour monoBehaviour) return monoBehaviour != null;

        return true;
    }

    private void OnClick()
    {
        _click = TypeClick.LMB;
    }

    private void OnClickCanceled()
    {
        _click = TypeClick.None;
    }

    private void OnShiftClick()
    {
        _click = TypeClick.ShiftLMB;
    }

    private void OnCtrlClick()
    {
        _click = TypeClick.CtrlLMB;
    }

    private void OnSpaceClick()
    {
        _click = TypeClick.SpaceLMB;
    }

    private void ReductionCooldownForCharge(int index, float reductionTime)
    {
        var tempTime = reductionTime;
        if (tempTime > _remainingCooldownTimeChargers[index])
            return;

        if (_currentChargeCooldownJob[index] != null)
            StopCoroutine(_currentChargeCooldownJob[index]);

        _currentChargeCooldownJob[index] = StartCoroutine(RechargeOneChargeCoroutine(index, tempTime));
    }

    private void StartDynamicRenderer()
    {
        _dynamicRendererJob = StartCoroutine(DynamicRendererJob());
    }
    private void AddAssist(Character character)
    {
        Hero.AssystCounter++;
    }

    private void AddAssist()
    {
        Hero.AssystCounter++;
    }

    private void AddKill(Character character)
    {
        Hero.KillCounter++;
    }

    private void SaveTargetData(TargetInfo targetInfo)
    {
        _targetInfoQueue.Enqueue(targetInfo);
    }

    private void LoadTargetDataForCheckCast()
    {
        if (_isCasting == false && _targetInfoQueue.TryPeek(out TargetInfo temp))
            LoadTargetData(temp);
    }

    private IEnumerator CooldownCoroutine(float cooldownTime)
    {
        CooldownStarted?.Invoke(cooldownTime);
        _remainingCooldownTime = cooldownTime;

        while (_remainingCooldownTime > 0)
        {
            _remainingCooldownTime -= Time.deltaTime;
            yield return null;
        }
        CooldownEnded?.Invoke();
        _cooldownJob = null;
    }

    private IEnumerator CastDeleyJob(float delayTime)
    {
        CastDeleyStarted?.Invoke(delayTime);

        Hero.Animator.SetFloat(HashAnimPlayer.CastSpeed, Buff.CastSpeed.Multiplier);
        _hero.Animator.SetTrigger(AnimTriggerCastDelay);
        _hero.NetworkAnimator.SetTrigger(AnimTriggerCastDelay);

        float time = 0;

        while (time < delayTime)
        {
            if (NoObstacles() == false)
            {
                TryCancel(true);
            }
            time += Time.deltaTime;
            yield return null;
        }
        _castDeleyCoroutine = null;
        CastDeleyEnded?.Invoke();
    }

    private IEnumerator CastStreamJob()
    {
        CastStreamStarted?.Invoke(CastStreamDuration);
        float time = 0;

        while (time < CastStreamDuration)
        {
            time += _manaCostRate;

            foreach (var skillCost in _manaCostPerTick)
            {
                var currentResourceValue = _hero.Resources.Where(r => r.Type == skillCost.resourceType).Sum(r => r.CurrentValue);

                if (currentResourceValue < Buff.ManaCost.GetBuffedValue(skillCost.resourceCost))
                {
                    TryCancel(true);
                }
                else
                {
                    var resource = _hero.Resources.First(r => r.Type == skillCost.resourceType);
                    resource.CmdUse(Buff.ManaCost.GetBuffedValue(skillCost.resourceCost));
                }
            }
            yield return new WaitForSeconds(_manaCostRate);
        }
        _castStreamCoroutine = null;
        CastStreamEnded?.Invoke();
    }

    private IEnumerator ActionWrapperForPreparingJob()
    {
        PreparingStarted?.Invoke(this);
        _isPreparing = true;
        //ClearData();
        StartAutoDraw();

        if (_isDynamicRenderer)
        {
            StartDynamicRenderer();
        }

        SubscribeClickEvents();
        _skillRender.SetPrepareCursor();

        yield return _prepareCoroutine = StartCoroutine(PrepareJob(SaveTargetData));

        UnSubscribeClickEvents();

        OnClickCanceled();

        //test
        if (_targetInfoQueue.TryPeek(out TargetInfo info))
        {
            if (info.GetTargets().Count > 0)
            {
                if (info.GetTargets()[0] is Character targetCharacter && targetCharacter != _hero)
                {
                    targetCharacter.UIComponent.CircleSelect1.IsActive = false;
                }
            }
        }

        PreparingSuccess?.Invoke(this);
        ClearTempTarget();
        _isPreparing = false;
        StopAutoDraw();

        _prepareCoroutine = null;
    }

    private IEnumerator ActionWrapperForCastingJob()
    {
        CastStarted?.Invoke();
        _isCasting = true;

        if (CastDeley > 0)
            yield return StartCastDeleyCoroutine();

        if (AnimTriggerCast != 0)
        {
            _isPlayCastAnim = true;
            //_isWaitingForCastCoroutine = true;

            float finalCastSpeed = Buff.CastSpeed.Multiplier * ExtraAnimationSpeedMultiplier;
            Hero.Animator.SetFloat(HashAnimPlayer.CastSpeed, finalCastSpeed);
            _hero.Animator.SetTrigger(AnimTriggerCast);
            _hero.NetworkAnimator.SetTrigger(AnimTriggerCast);

            while (_isPlayCastAnim)
            {
                //*
                if (_tempForDamage != null && !IsValidTarget(_tempForDamage))
                {
                    _isCanCancle = true;
                    _hero.Move.CanMove = true;

                    TryCancel(true);
                    yield break;
                }

                if (!IsCanCast)
                {
                    TryCancel(true);
                    yield break;
                }
                //*/
                yield return null;
            }

            //_isWaitingForCastCoroutine = false;
        }

        else
        {
            _hero.Animator.SetTrigger(HashAnimPlayer.AnimCancled);
            _hero.NetworkAnimator.SetTrigger(HashAnimPlayer.AnimCancled);

            _castCoroutine = StartCoroutine(CastJob());
            if (_castDuration > 0) _castStreamCoroutine = StartCoroutine(CastStreamJob());
            yield return _castCoroutine;
        }

        _hero.Animator.SetTrigger(HashAnimPlayer.AnimCancled);
        _hero.NetworkAnimator.SetTrigger(HashAnimPlayer.AnimCancled);

        CastSuccess?.Invoke();
        CastEnded?.Invoke();
        _isCasting = false;

        ClearData();

        /// test
        if (_tempTargetForDamage != null && _tempTargetForDamage.TryGetComponent(out Character character))
        {
            character.SelectedCircle.IsActive = false;
            character.SelectedCircle.SwitchSelectCircle(false);
        }

        _hero.Move.StopLookAt();
        if (!_isAutoMode) _hero.Move.CanMove = true;

        _castCoroutine = null;
    }

    private IEnumerator CancelCoroutine()
    {
        yield return new WaitForNextFrameUnit();
    }

    [ClientRpc]
    public void RpcResetSkillState()
    {
        ResetSkillState();
    }

    [ClientRpc]
    public void RpcCancelActiveSkill()
    {
        if (_isPreparing || _isCasting)
        {
            TryCancel(true);
        }
    }

    public void ResetSkillState()
    {
        _remainingCooldownTime = 0;

        if (_cooldownJob != null)
        {
            StopCoroutine(_cooldownJob);
            _cooldownJob = null;
        }
        CooldownEnded?.Invoke();

        if (_castDeleyCoroutine != null)
        {
            StopCoroutine(_castDeleyCoroutine);
            _castDeleyCoroutine = null;
        }
        CastDeleyEnded?.Invoke();

        _isPreparing = false;
        _isCasting = false;

        if (_isUseCharges)
        {
            _currentChargers = _maxCharges;
            CurrentChargeChanged?.Invoke(_currentChargers);
        }

        if (_castStreamCoroutine != null)
        {
            StopCoroutine(_castStreamCoroutine);
            _castStreamCoroutine = null;
        }
        CastStreamEnded?.Invoke();

        CancelCoroutine(_castCoroutine);
        CancelCoroutine(_actionWrapperForPreparingCoroutine);
        CancelCoroutine(_actionWrapperForCastCoroutine);
        ClearData();
    }

    public void ApplyDamage(Damage damage, GameObject target)
    {
        var damageable = target != null ? target.GetComponent<IDamageable>() : null;
        if (damageable != null)
        {
            damageable.TryTakeDamage(ref damage, this);
            _hero.DamageTracker.AddDamage(damage, target, isServerRequest: isServer);
            _hero.DamageGet(damage, target);
        }

        else
        {
            Debug.LogWarning($"[Skill] Target {target?.name} is not damageable or null");
        }

        _hero.DamageTracker.AddDamage(damage, target, isServerRequest: isServer);
        _hero.DamageGet(damage, target);

    }

    public void CmdApplyDamage(Damage damage, GameObject target)
    {
        _hero.DamageGet(damage, target);
        CmdApplyDamageLogic(damage, target);
    }

    [Command]
    private void CmdApplyDamageLogic(Damage damage, GameObject target)
    {
        if (_tempTargetForDamage != target.transform)
        {
            _tempTargetForDamage = target.transform;
            _tempForDamage = target.GetComponent<IDamageable>();
        }

        if (target == null)
        {
            Debug.LogError("[CmdApplyDamageLogic] Target is null, skipping");
            return;
        }

        ApplyDamage(damage, target);
    }

    public void ApplyHeal(Heal heal, GameObject hp, Skill skill, string sourceName)
    {
        hp.GetComponent<IHealingable>().Heal(ref heal, sourceName, skill);
        Hero.DamageTracker.AddHeal(heal, isServerRequest: isServer);
    }

    [Command]
    public void CmdApplyHeal(Heal heal, GameObject hp, Skill skill, string sourceName)
    {
        if (_tempTargetForDamage != hp.transform)
        {
            _tempTargetForDamage = hp.transform;
            _tempForHealing = hp.GetComponent<IHealingable>();
        }

        ApplyHeal(heal, hp, skill, sourceName);
    }

    public void AfterCastJob()
    {
        CmdSkillAfterCastJob();
        SkillAfterCastJob();
    }

    [Command] private void CmdSkillAfterCastJob() => SkillAfterCastJob();

    private void SubscribeClickEvents()
    {
        InputHandler.OnClick += OnClick;
        InputHandler.OnShiftLeftMouse += OnShiftClick;
        InputHandler.OnSwitchAutoMode += OnCtrlClick;
        InputHandler.OnSpacetLeftMouse += OnSpaceClick;

        //cancelled

        InputHandler.OnClickCanceled += OnClickCanceled;
        InputHandler.OnShiftLeftMouseCanceled += OnClickCanceled;
        InputHandler.OnSwitchAutoModeCanceled += OnClickCanceled;
        InputHandler.OnSpacetLeftMouseCanceled += OnClickCanceled;

    }

    private void UnSubscribeClickEvents()
    {
        InputHandler.OnClick -= OnClick;
        InputHandler.OnShiftLeftMouse -= OnShiftClick;
        InputHandler.OnSwitchAutoMode -= OnCtrlClick;
        InputHandler.OnSpacetLeftMouse -= OnSpaceClick;

        //cancelled

        InputHandler.OnClickCanceled -= OnClickCanceled;
        InputHandler.OnShiftLeftMouseCanceled -= OnClickCanceled;
        InputHandler.OnSwitchAutoModeCanceled -= OnClickCanceled;
        InputHandler.OnSpacetLeftMouseCanceled -= OnClickCanceled;

    }
}