using Mirror;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CreeperStrike : Skill
{
    #region Variables

    [Header("Talents")]
    [SerializeField] private RestorationOfGlands _restorationOfGlands;
    [SerializeField] private ReleaseFromSecrecy _releaseFromSecrecy;
    [SerializeField] private StrokesOfAspiration _strokesOfAspiration;
    [SerializeField] private AssasinPoison _assasinPoison;
    [SerializeField] private DesireToHide _desireToHide;
    [SerializeField] private FirstStrike _firstStrike;
    [SerializeField] private FeelingOfContinuation _feelingOfContinuation;
    [SerializeField] private PreparingForFight _preparingForFight;
    [SerializeField] private bool isGeneticsTalentOne; 

    [Header("Abilities")]
    [SerializeField] private LightningStrikes _lightningStrikes;
    [SerializeField] private LightningMovement _lightningMovement;
    [SerializeField] private PoisonBall _poisonBall;
    [SerializeField] private CreeperInvisible _creeperInvisible;
    [SerializeField] private ColdBlood _coldBlood;
    [SerializeField] private SneakySpit sneakySpit;
    [SerializeField] private BlockPassiveSkill blockPassiveSkill;
    //[SerializeField] private AbsorptionOfPoisons _absorptionOfPoisons;

    [Header("Ability properties")]
    [SerializeField] private Character _player;
    [SerializeField] private float _multiplyCritDamage = 1.5f;
    [SerializeField ]private float _chanceOfCriticalStrike = 0.05f;

    //private Character _target;
    private Character _lastTarget;

    private int _currentCountHit = 0;
    private int _currentHitForStrokesOfAspiration = 0;
    private int _countHitForDesireToHideTalent = 0;
    private int _countCurrentHitForPreparingForFight = 0;
    private int _poisonBoneStack = 0;

    private float _animTime;
    private float _currentDamage;
    private float _lifeTimePoisonBoneStacks = 6.0f;

    private bool _isTwoHit = false;
    private bool _isHit = false;



    private Character _lastTargetFirst = null;
    private Character _lastTargetSecond = null;

    private Coroutine _timerForTwoHitVariableCoroutine;

    public int CurrentCountHit { get => _currentCountHit; set => _currentCountHit = value; }
    public int CountHitForReleaseFromSecrecyTalent { get => _countHitForDesireToHideTalent; set => _countHitForDesireToHideTalent = value; }
    public int PoisonBoneStack { get => _poisonBoneStack; set => _poisonBoneStack = value; }
    public bool IsTwoHit { get => _isTwoHit; set => _isTwoHit = value; }
    public bool IsHit { get => _isHit; set => _isHit = value; }

    protected override int AnimTriggerCast => Animator.StringToHash("CreeperStrikeAttacking");
    protected override int AnimTriggerCastDelay => 0;

    public event System.Action OnCreeperStrikeEnd;

    #endregion

    #region CastAbility

    public void AnimCreeperStrikeCast()
    {
        AnimStartCastCoroutine();
    }

    public void AnimCreeperStrikeEnded()
    {
        OnCreeperStrikeEnd?.Invoke();
        AnimCastEnded();
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        TargetInfo info = new TargetInfo();


        while (GetTargetCharacter() == null)
        {
            if (GetMouseButton)
            {
                FindTargetCharacter();
              /*  if (_target != null)
                {
                    _target.SelectedCircle.IsActive = true;
                    _hero.Move.LookAtTransform(_target.transform);
                    break;
                }*/
            }
            yield return null;
        }


        info.AddTarget(GetTargetCharacter());
        info.Points.Add(GetTargetCharacter().transform.position);
        callbackDataSaved?.Invoke(info);
    }

    protected override IEnumerator CastJob()
    {
        if (GetTargetCharacter() != null && Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius)
        {
            _hero.Move.StopLookAt();
            DamageDeal(GetTargetCharacter());
        }
        ClearTarget();
        yield return null;
    }

    /*public void SetTarget(Character target)
    {
       // _target = target;
    }*/

    public void ClearDataCreeperStrike()
    {
        TryCancel();
        StopAutoDraw();
    }

    private void IncreaseAnimSpeed()
    {
        if (_animTime > 0)
        {
            float multiplier = _lightningMovement.DurationLeap - 4.9f; // òåñòîâàÿ ñêîðîñòü (èçíà÷àëüíî - 0.1)
            float animTimeMultiplier = _animTime / multiplier;
            _player.Animator.SetFloat("CreeperStrikeMultiplierSpeedAnimation", animTimeMultiplier);
        }
    }

    private float GetClipLength()
    {
        RuntimeAnimatorController animController = _player.Animator.runtimeAnimatorController;
        foreach (var clip in animController.animationClips)
        {
            if (clip.name == "CreeperStrikeAttack")
            {
                return clip.length;
            }
        }
        return -1f;
    }

    public void DamageDeal(Character target, bool isUsingLightningStrikes = false)
    {
        var lastÑast = _player.Abilities.LastCastedSkill;
        var previewCast = _player.Abilities.PreviewCastedSkill;

        if (target != null)
        {
            _currentDamage = UnityEngine.Random.Range(7.0f, 11.0f);
            float _currentChanceOfCriticalStrike = UnityEngine.Random.Range(0.0f, 1.0f);

            _isHit = true;
            _currentCountHit++;


            //if (_absorptionOfPoisons != null && _absorptionOfPoisons.IsWorking)
            //{
            //    _absorptionOfPoisons.CheckTargetWithDebuffs(target.gameObject);
            //}

            if (_strokesOfAspiration.Data.IsOpen)
            {
                _currentHitForStrokesOfAspiration++;

                if (_currentHitForStrokesOfAspiration == 2)
                {
                    if (_lastTarget == target)
                    {
                        _strokesOfAspiration.UseTalentStrokesOfAspiration();
                    }

                    _currentHitForStrokesOfAspiration = 0;
                }

                _lastTarget = target;
            }

            if (_restorationOfGlands.Data.IsOpen && _poisonBoneStack > 0 && target.CharacterState.CheckForState(States.PoisonBone))
            {
                Debug.Log("CreeperStrike / if == true");
                float baseChanceOfRestorationOfGlands = 0.9f;
                float chanceOfRestorationOfGlands = baseChanceOfRestorationOfGlands * _poisonBoneStack;

                if (UnityEngine.Random.Range(0f, 1f) <= chanceOfRestorationOfGlands)
                {
                    Debug.Log("CreeperStrike / restorationOfGlands");
                    _restorationOfGlands.ReductionCooldown();
                }
            }

            if (_desireToHide.Data.IsOpen)
            {
                _countHitForDesireToHideTalent++;

                if (_countHitForDesireToHideTalent == 5)
                {
                    _desireToHide.ApplyInvisible();
                    _countHitForDesireToHideTalent = 0;
                }
            }

            if (_releaseFromSecrecy.Data.IsOpen && _creeperInvisible.IsInvisible)
            {
                _creeperInvisible.ExitingInvisible();
            }

            if (_assasinPoison.Data.IsOpen)
            {
                _assasinPoison.SpendCharge(target, _lifeTimePoisonBoneStacks);
            }

            if (_preparingForFight.Data.IsOpen && _creeperInvisible.IsReadyToThreeHitForPreparingForFightTalent)
            {
                _countCurrentHitForPreparingForFight++;

                CmdPreparingForFight(_player.gameObject);

                if (_countCurrentHitForPreparingForFight == 3)
                {
                    _countCurrentHitForPreparingForFight = 0;
                    _creeperInvisible.IsReadyToThreeHitForPreparingForFightTalent = false;
                }
            }

            if (_coldBlood.IsCanCritCreeperStrike || _coldBlood.IsCanCritLightningStrikes)
            {
                if (_player.IsInvisible)
                    _creeperInvisible.ExitingInvisible();

                DealCriticalDamage(target, _currentDamage, true);
            }
            else if (_currentChanceOfCriticalStrike <= _chanceOfCriticalStrike)
            {
                DealCriticalDamage(target, _currentDamage);
            }
            else
            {

                Damage damage = new Damage
                {
                    Value = Buff.Damage.GetBuffedValue(_currentDamage),
                    Type = DamageType.Physical,
                    PhysicAttackType = AttackRangeType.MeleeAttack,
                };

                CmdDamageDeal(damage, target.gameObject);
            }

            if (_firstStrike.Data.IsOpen)
            {
                _firstStrike.ReturnBoolFalse();
            }

            if (_currentCountHit == 2 || isUsingLightningStrikes && _currentCountHit == 2)
            {
                float time = 10f;

                if (_timerForTwoHitVariableCoroutine != null)
                {
                    StopCoroutine(_timerForTwoHitVariableCoroutine);
                }

                _timerForTwoHitVariableCoroutine = StartCoroutine(TimerForTwoHit(time, isUsingLightningStrikes));
                
                _currentCountHit = 0;

                if (_coldBlood.IsCanCritLightningStrikes)
                {
                    _coldBlood.IsCanCritLightningStrikes = false;
                }
            }

            _isHit = false;
        }

        TryTriggerSneakySpitWindow(target);
    }

    private void TryTriggerSneakySpitWindow(Character target)
    {
        _lastTargetSecond = _lastTargetFirst;
        _lastTargetFirst = target;

        var lastCast = _player.Abilities.LastCastedSkill;
        var previewCast = _player.Abilities.PreviewCastedSkill;

        if (_lastTargetFirst == target && _lastTargetSecond == target && lastCast is CreeperStrike && previewCast is CreeperStrike) ÑmdTriggerSneakySpitFreeWindow(target);
        if (_lastTargetFirst == target && lastCast is CreeperStrike) ÑmdBlockPassiveSkillFreeWindow(target);
    }

    private IEnumerator TimerForTwoHit(float duration, bool isUsingLightningStrikes)
    {
        float time = duration;

        _isTwoHit = true;

        _lightningStrikes.IsUsedLightningStrikes = isUsingLightningStrikes;

        while (time > 0)
        {
            time -= Time.deltaTime;
            
            if (time <= 0)
            {
                _isTwoHit = false;
                _lightningStrikes.IsUsedLightningStrikes = false;
            }

            yield return null;
        }

        StopCoroutine(_timerForTwoHitVariableCoroutine);
        _timerForTwoHitVariableCoroutine = null;
    }
    #endregion

    #region CalculateCriticalDamage

    private float CalculateCriticalDamage(Character target, float baseDamage)
    {
        float criticalDamage = baseDamage;
        float multiplyDamage = _multiplyCritDamage;
        float firstStrikeTalentMultiplyDamage = 5.0f;
        float coldBloodMultiplyDamage = 2.5f;

        if (_poisonBoneStack > 0)
        {
            for (int i = 0; i < _poisonBoneStack; i++)
            {
                multiplyDamage += 0.5f;
            }
        }

        if (_firstStrike.Data.IsOpen && _firstStrike.IsCanIncreaseCrit && _firstStrike.FirstHit)
        {
            criticalDamage *= (multiplyDamage * firstStrikeTalentMultiplyDamage);
            _firstStrike.ReturnBoolFalse();
        }
        else if (_coldBlood.IsCanCritCreeperStrike || _coldBlood.IsCanCritLightningStrikes)
        {
            float endCriticalDamage = coldBloodMultiplyDamage + multiplyDamage;

            if (_lightningStrikes.IsUsedLightningStrikes)
            {
                _coldBlood.IsCanCritCreeperStrike = false;
            }
            else
            {
                _coldBlood.IsCanCritCreeperStrike = false;
                _coldBlood.IsCanCritLightningStrikes = false;
            }

            criticalDamage *= endCriticalDamage;
        }
        else
        {
            criticalDamage *= multiplyDamage;
        }

        return criticalDamage;
    }

    private void DealCriticalDamage(Character currentTarget, float criticalDamage, bool isTalentCritDamage = false)
    {
        if (isTalentCritDamage)
        {
            criticalDamage = CalculateCriticalDamage(currentTarget, criticalDamage);
        }
        else if (isGeneticsTalentOne && currentTarget.CharacterState.CheckForState(States.PoisonBone))
        {
            criticalDamage = CalculateCriticalDamage(currentTarget, criticalDamage);
        }
        
        Damage critDamage = new Damage
        {
            Value = Buff.Damage.GetBuffedValue(criticalDamage),
            Type = DamageType.Physical,
            PhysicAttackType = AttackRangeType.MeleeAttack,
        };

        CmdApplyDamage(critDamage, currentTarget.gameObject);

        if (_feelingOfContinuation.Data.IsOpen) CmdFeelingOfContinuation(_player.gameObject, critDamage.Value);
    }

    #endregion

    #region CommandMethods

    [Command]
    private void CmdFeelingOfContinuation(GameObject player, float criticalDamage)
    {
        Character playerCharacter = player.GetComponent<Character>();
        _feelingOfContinuation.IncreaseRegenerationMana(playerCharacter, criticalDamage);
    }

    [Command]
    private void CmdPreparingForFight(GameObject player)
    {
        Character playerCharacter = player.GetComponent<Character>();
        _preparingForFight.IncreaseManaRegeneration(playerCharacter);
    }

    [Command] private void CmdDamageDeal(Damage damage, GameObject target) => ApplyDamage(damage, target);

    [Command] private void ÑmdTriggerSneakySpitFreeWindow(Character target) => RpcTriggerSneakySpitWindow(target);

    [Command] private void ÑmdBlockPassiveSkillFreeWindow(Character target) => RpcBlockPassiveSkillFreeWindow(target);

    [ClientRpc]
    private void RpcTriggerSneakySpitWindow(Character target)
    {
        if (sneakySpit != null) sneakySpit.TryStartSneakySpitBoostWindow(target);
    }

    [ClientRpc]
    private void RpcBlockPassiveSkillFreeWindow(Character target)
    {
        if (blockPassiveSkill != null) blockPassiveSkill.TryStartBlockPassiveSkillBoostWindow(target);
    }

    #endregion

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo?.GetTargets()?.Count > 0) SetTarget(targetInfo.GetTargets()[0] as Character);
    }

    #region Talents

    public void GeneticsTalentOne(bool value)
    {
        isGeneticsTalentOne = value;
    }

    protected override void ClearData()
    {
        ClearTarget();
        _hero.Move.StopLookAt();
    }

    #endregion
}
