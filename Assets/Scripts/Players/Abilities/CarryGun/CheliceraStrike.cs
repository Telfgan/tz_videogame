using Mirror;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class CheliceraStrike : Skill
{
    [SerializeField] private Character _player;
    [SerializeField] private BasePsionicEnergy _basePsionicEnergy;
    [SerializeField] private AttackingPsionicEnergy _attackingPsionicEnergy;
    [SerializeField] private JumpWithChelicera _jumpWithChelicera;
    [SerializeField] private ClawStrike clawStrike;
    [SerializeField] private CooldownEnergy cooldownEnergy;
    [SerializeField] private float animSpeed = 1.4f;
    [SerializeField] private float chanceCritDamageEvolutionTwo = 0.05f;
    [SerializeField] private float chanceCritDamageEvolutionFour = 0.15f;
    [SerializeField] private float chanceApplyBleeding = 0.15f;
    [SerializeField] private float durationBleeding = 3.0f;
    [SerializeField] private float chanceApplyBleedingIncrease = 0.4f;
    [SerializeField] private float chanceCritDamageIncrease = 0.3f;
    [SerializeField] private float cooldownEnergyCost = 2;

    private Damage _dealDamage;
    private Animator _animator;
    //private IDamageable _target;
    //private Character _runtimeTarget;
    private float _totalChanceApplyBleeding;
    private float _totalchanceCritDamage;
    private float _criticalDamage;
    private float _baseDamage;
    private float _additionalDamageFromSkill;
    private float _spentAttackingPsiEnergy;
    private bool _isClawStrike_Right = true;
    //private Coroutine _castDelayResetCoroutine;
    //private string _baseDescription;

    private static readonly int RightClawStrikeTrigger = Animator.StringToHash("CheliceraStrikeTrigger_Right");
    private static readonly int LeftClawStrikeTrigger = Animator.StringToHash("CheliceraStrikeTrigger_Left");

    protected override int AnimTriggerCast => _isClawStrike_Right ? RightClawStrikeTrigger : LeftClawStrikeTrigger;
    protected override int AnimTriggerCastDelay => 0;

    protected override bool IsCanCast => CheckIsCanCast() && cooldownEnergy.CurrentValue >= cooldownEnergyCost;

    public float ChanceCritDamageEvolutionFour { get => chanceCritDamageEvolutionFour; set => chanceCritDamageEvolutionFour = value; }

    public event System.Action OnCheliceraStrikeEnd;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnDisable()
    {
        OnSkillCanceled -= HandleSkillCanceled;
    }
    private void OnEnable()
    {
        _baseDamage = UnityEngine.Random.Range(11f, 13f);
        Damage = _baseDamage;
        OnSkillCanceled += HandleSkillCanceled;
    }

    #region Talens
    private bool isCheliceraStrikeChanceDamageCrit = false;
    private bool isEvolutionTalentTwo = false;
    private bool isPsionicsTalentTwo = false;
    private bool _isChanceApplyBleedingIncrease = false;
    private bool _isChanceCritDamageIncrease = false;

    public void CheliceraStrikeChanceDamageCrit(bool value) => isCheliceraStrikeChanceDamageCrit = value;
    public void EvolutionTalentTwo(bool value) => isEvolutionTalentTwo = value;

    public void PsionicsTalentTwo(bool value, string text)
    {
        isPsionicsTalentTwo = value;
        AbilityInfoHero.FinalDescription = value ? AbilityInfoHero.Description + $" {text}" : AbilityInfoHero.Description;
    }

    public void ChanceApplyBleedingIncrease(bool value) => _isChanceApplyBleedingIncrease = value;
    public void ChanceCritDamageIncrease(bool value) => _isChanceCritDamageIncrease = value;
    #endregion

    private bool CheckIsCanCast()
    {
        return GetTargetCharacter() != null &&
            Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius &&
            NoObstacles(GetTargetCharacter().transform.position, transform.position, _obstacle);
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        //_runtimeTarget = null;

        while (GetTargetCharacter() == null)
        {
            if (GetMouseButton)
            {
                FindTargetCharacter();
                //_target = GetRaycastTarget();

                if (GetTargetCharacter() != null && GetTargetCharacter() is Character characterTarget)
                {
                    //_runtimeTarget = characterTarget;
                    characterTarget.SelectedCircle.IsActive = true;
                }
            }
            yield return null;
        }

        TargetInfo targetInfo = new TargetInfo();
        if (GetTargetCharacter() != null) targetInfo.AddTarget(GetTargetCharacter());
        callbackDataSaved?.Invoke(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        if (GetTargetCharacter() == null) yield break;

        _baseDamage = UnityEngine.Random.Range(11f, 13f);
        Damage = _baseDamage;

        if (_jumpWithChelicera.IsJumpDone)
        {
            cooldownEnergy.CastCooldownEnergySkill(_jumpWithChelicera.CooldownJump, _jumpWithChelicera);

            SetTarget((Character)_jumpWithChelicera.GetTargetCharacter());
            _jumpWithChelicera.IsJumpDone = false;
        }
        else cooldownEnergy.CastCooldownEnergySkill(cooldownEnergyCost, this);

        DamageDealChelicera(GetTargetCharacter());
        _isClawStrike_Right = !_isClawStrike_Right;

        yield return null;
    }

    private void HandleSkillCanceled()
    {
        ClearTarget();
        //_target = null;
    }

   /* public void SetTarget(IDamageable target)
    {
        _target = target;
    }*/

    public void DamageDealChelicera(IDamageable target)
    {
        if (target == null) return;
        Character targetCharacter = target as Character;

        if (_jumpWithChelicera.IsJumpDone)
        {
            float bonusDamage = _baseDamage * _additionalDamageFromSkill;
            Damage = _baseDamage + bonusDamage;
        }
        else Damage = _baseDamage;

        if (isEvolutionTalentTwo)
        {
            float chanceBleedingValue = UnityEngine.Random.Range(0f, 1f);
            float chanceCritValue = UnityEngine.Random.Range(0f, 1f);

            _totalChanceApplyBleeding = chanceApplyBleeding;
            _totalchanceCritDamage = chanceCritDamageEvolutionTwo;

            if (_isChanceApplyBleedingIncrease && CheckStateForBleeding()) _totalChanceApplyBleeding += chanceApplyBleedingIncrease;
            if (_isChanceCritDamageIncrease && CheckStateForBleeding()) _totalchanceCritDamage += chanceCritDamageIncrease;

            if (chanceCritValue <= _totalchanceCritDamage) _criticalDamage = CriticalDamageDeal(Damage, 1.6f);

            if (chanceBleedingValue <= _totalChanceApplyBleeding && targetCharacter != null) CmdAddState(targetCharacter);
        }

        if (isCheliceraStrikeChanceDamageCrit)
        {
            float chanceCritValue = UnityEngine.Random.Range(0f, 1f);
            float chanceCritDamageValue = UnityEngine.Random.Range(1.8f, 2.7f);

            _totalchanceCritDamage = chanceCritDamageEvolutionFour;

            if (_isChanceCritDamageIncrease && CheckStateForBleeding()) _totalchanceCritDamage += chanceCritDamageIncrease;

            if (chanceCritValue <= chanceCritDamageEvolutionFour) _criticalDamage = CriticalDamageDeal(Damage, chanceCritDamageValue);
        }

        _dealDamage = new Damage()
        {
            Value = Damage + _criticalDamage,
            Type = DamageType.Physical,
            PhysicAttackType = AttackRangeType.MeleeAttack,
        };

        if (_attackingPsionicEnergy.IsAttackingPsiEnergy && targetCharacter != null) DamageDealWithAttackingPsionicEnergy(targetCharacter);

        CmdApplyDamage(_dealDamage, target.gameObject);

        _criticalDamage = 0f;
        _dealDamage.Value = 0f;
        Damage = _baseDamage;
    }

    private float CriticalDamageDeal(float criticalDamage, float multiplierCrit)
    {
        return criticalDamage * multiplierCrit;
    }

    private bool CheckStateForBleeding()
    {
        States[] blockingStates = { States.Stun, States.Stupefaction, States.TentacleGrip };
        return GetTargetCharacter() != null && blockingStates.Any(state => GetTargetCharacter().CharacterState.CheckForState(state));
    }

    private void DamageDealWithAttackingPsionicEnergy(Character targetCharacter)
    {
        float attackingPsi = _spentAttackingPsiEnergy;

        float magicDamagePerPsiMainTarget = 0.3f;
        float magicDamagePerPsiNearby = 0.5f;

        if (!isPsionicsTalentTwo && attackingPsi <= 0) return;

        float radius = attackingPsi >= 30 ? 2.5f : attackingPsi >= 20 ? 2f : 1.5f;

        if (attackingPsi >= 10)
        {
            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, radius, _targetsLayers);
            foreach (var enemyCollider in nearbyEnemies)
                if (enemyCollider.TryGetComponent<Character>(out var enemy) && enemy != targetCharacter)
                {
                    CmdDispel(enemy);
                    ApplyDamage(attackingPsi, magicDamagePerPsiNearby, enemy);
                }

            TotalMagicDamageEnemy(targetCharacter, attackingPsi, magicDamagePerPsiMainTarget);
            CmdDispel(targetCharacter);
        }
    }

    private void ApplyDamage(float attackingPsi, float magicDamagePerPsiNearby, Character enemy)
    {
        if (enemy != _player)
        {
            TotalMagicDamageEnemy(enemy, attackingPsi, magicDamagePerPsiNearby);
        }
    }

    private void TotalMagicDamageEnemy(Character enemy, float attackingPsi, float magicDamage)
    {
        float totalMagicDamageEnemy = attackingPsi * magicDamage;

        var magicDamageNearby = new Damage
        {
            Value = totalMagicDamageEnemy,
            Type = DamageType.Magical,
            PhysicAttackType = AttackRangeType.MeleeAttack,
        };

        CmdApplyDamage(magicDamageNearby, enemy.gameObject);
    }

    public void CheliceraStrikePreparingForAnim()
    {
        _player.Move.CanMove = false;
        if (_attackingPsionicEnergy.IsAttackingPsiEnergy && _attackingPsionicEnergy.CurrentValue > 0f) TrySpendAttackingPsi();
        else _spentAttackingPsiEnergy = 0;
    }

    public void SetAdditionalDamage(float value)
    {
        _additionalDamageFromSkill = value;
    }

    public void CheliceraStrikeCast()
    {
        AnimStartCastCoroutine();
    }

    public void CheliceraStrikeEnded()
    {
        OnCheliceraStrikeEnd?.Invoke();
        _player.Move.CanMove = true;
        AnimCastEnded();
    }

    public void ClearDataCheliceraStrike()
    {
        ClearData();
        StopAutoDraw();
    }

    public void TrySpendAttackingPsi()
    {
        _spentAttackingPsiEnergy = _attackingPsionicEnergy.CurrentValue;
        CmdUseAttackingEnergy(_attackingPsionicEnergy.CurrentValue);
    }

    [Command]
    private void CmdUseAttackingEnergy(float value)
    {
        _attackingPsionicEnergy.CurrentValue -= value;
    }

    [Command]
    private void CmdAddState(Character character)
    {
        character.CharacterState.AddState(States.Bleeding, durationBleeding, 0, _player.gameObject, null);
    }

    [Command]
    private void CmdDispel(Character targetCharacter)
    {
        targetCharacter.CharacterState.DispelStates(StateType.Magic, targetCharacter.NetworkSettings.TeamIndex, _player.NetworkSettings.TeamIndex, true);
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo.GetTargets().Count > 0) SetTarget((Character)targetInfo.GetTargets()[0]);
    }

    protected override void ClearData()
    {
        ClearTarget();
        //_target = null;
    }
}
