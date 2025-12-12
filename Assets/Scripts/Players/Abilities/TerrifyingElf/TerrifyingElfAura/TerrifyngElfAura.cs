using Mirror;
using UnityEngine;
using System.Collections;
using System;

public class TerrifyingElfAura : NetworkBehaviour
{
    [Header("Main fields")]
    [SerializeField] private SkillManager skillManager;
    [SerializeField] private HeroComponent hero;

    [Header("Chances")]
    [SerializeField, Range(0f, 100f)] private float calmnessChance = 10f;
    [SerializeField, Range(0, 100)] private float elvenSkillFromPhysChance = 10f;
    [SerializeField, Range(0,100)] private float calmnessOnElvenSkillChance = 30f;
    [SerializeField, Range(0f, 100f)] private float huntressMarkApplyChance = 5f;

    [Header("Durations")]
    [SerializeField] private float durationCalmess;
    [SerializeField] private float durationHuntressMark;
    [SerializeField] private float durationElvenSkill;

    [Header("Effects")]
    [SerializeField] private GameObject elvenSkillEffect;

    [Header("Radius")]
    [SerializeField] private float radiusTreeCalmess = 12f;

    [Header("Skills")]
    [SerializeField] private ReconnaissanceFire reconnaissanceFire;

    public GameObject ElvenSkillEffect { get => elvenSkillEffect; set => elvenSkillEffect = value; }

    #region boolTalent

    private bool calmnessTalent;
    private bool fireWorshipperTalent;
    private bool treeRadiusCalmessTalent;
    private bool huntressMarkPhysicsTalent;
    private bool manaAbsorptionPhysicalTalent;
    private bool elvenSkillTalent;
    private bool elvenSkillPhysicsTalent;
    private bool calmnessOnElvenSkillTalent;
    private bool suppressionManaAbsorptionTalent;
    private bool _isReductionRecharge;
    private bool _isElvenSkillPhysDamageHealthChance;

    #endregion

    private Skill currentSkill;
    private Mana _heroMana;
    private float _baseAreaReconnaissanceFire;

    public bool IsReductionRecharge { get => _isReductionRecharge; }
    public bool IsElvenSkillPhysDamageHealthChance { get => _isElvenSkillPhysDamageHealthChance; }

    private void OnEnable()
    {
        CacheHeroMana();
        if (skillManager != null && skillManager.SkillQueue != null) skillManager.SkillQueue.SkillAdded += OnSkillAdded;
        if (hero != null && hero.DamageTracker != null) hero.DamageTracker.OnDamageTracked += OnDamageTracked;
        if (manaAbsorptionPhysicalTalent) hero.DamageTracker.OnDamageTracked += OnDamageDealt;
        if (_heroMana != null) _heroMana.ValueChanged += OnManaChanged;
        _baseAreaReconnaissanceFire = reconnaissanceFire.BaseArea;
    }

    private void OnDisable()
    {
        if (currentSkill != null)
        {
             currentSkill.CastSuccess -= ApplyCalmnessTalent;
             currentSkill.CastStarted -= ApplyFireWorshipperTalent;
        }

        if (_heroMana != null) _heroMana.ValueChanged -= OnManaChanged;
        hero.DamageTracker.OnDamageTracked -= OnDamageDealt;
        if (hero != null && hero.DamageTracker != null) hero.DamageTracker.OnDamageTracked -= OnDamageTracked;
    }

    private void OnSkillAdded(Skill skill)
    {
        currentSkill = skill;
        if (skill == null) return;

        if (calmnessTalent) skill.CastSuccess += ApplyCalmnessTalent;

        if (fireWorshipperTalent) skill.CastStarted += ApplyFireWorshipperTalent;
    }

    #region Work with mana

    private void CacheHeroMana()
    {
        if (hero == null) return;
        if (_heroMana != null) return;

        _heroMana = hero.TryGetResource(ResourceType.Mana) as Mana;
    }

    private void OnManaChanged(float oldValue, float newValue)
    {
        if (newValue > 0f) return;

        Debug.Log("Маны нет");
        ApplyElvenSkill();
        StartCoroutine(ReSubscribeAfterDelay());
    }

    private IEnumerator ReSubscribeAfterDelay()
    {
        yield return new WaitForSeconds(durationElvenSkill);

        if (_heroMana != null)
            _heroMana.ValueChanged += OnManaChanged;
    }

    #endregion

    #region Talent
    public void ReductionRecharge(bool value) => _isReductionRecharge = value;
    public void CalmnessTalentActive(bool value) => calmnessTalent = value;
    public void ElvenSkillPhysDamageHealthChance(bool value) => _isElvenSkillPhysDamageHealthChance = value;
    #endregion

    #region CalmnessTalent


    private void ApplyCalmnessTalent()
    {
        if (!calmnessTalent || currentSkill == null) return;

        if (currentSkill.AbilityForm == AbilityForm.Spell || currentSkill.AbilityForm == AbilityForm.Magic)
        {
            var character = currentSkill.Hero;
            if (character != null && character.CharacterState != null)
            {
                bool isCalmnessChance = UnityEngine.Random.Range(0f, 100f) <= calmnessChance;

                if (isCalmnessChance)
                {
                    character.CharacterState.CmdAddState(States.Calmness, durationCalmess, 0f, this.gameObject, currentSkill.Name);

                    if (treeRadiusCalmessTalent)
                    {
                        int treesCount = GetTreesCountInRadius(radiusTreeCalmess);
                        StartCoroutine(DelayAndUpdateCalmness(character.CharacterState, treesCount));
                    }
                }
            }
        }

        currentSkill = null;
    }

    #endregion

    #region FireWorshipperTalent

    public void FireWorshipperTalentActive(bool value)
    {
        fireWorshipperTalent = value;
        if (!fireWorshipperTalent) reconnaissanceFire.Area = _baseAreaReconnaissanceFire;
        else reconnaissanceFire.Area += 1;
    }

    private void ApplyFireWorshipperTalent()
    {
        if (!fireWorshipperTalent || currentSkill == null) return;

        if (currentSkill.DamageType != DamageType.Physical)
            return;

        var character = currentSkill.Hero;
        var targets = currentSkill.GetCloserTargetsCharacter(currentSkill.transform.position, currentSkill.Radius);
        if (targets == null || targets.Count == 0) return;

        foreach (var target in targets)
        {
            if (target != null && target.CharacterState.CheckForState(States.HuntressMark))
            {
                bool isCalmnessChance = UnityEngine.Random.Range(0f, 100f) <= calmnessChance;

                if (isCalmnessChance)
                {
                    character.CharacterState.CmdAddState(States.Calmness, durationCalmess, 0f, target.gameObject, currentSkill.Name);

                    if (treeRadiusCalmessTalent)
                    {
                        int treesCount = GetTreesCountInRadius(radiusTreeCalmess);
                        StartCoroutine(DelayAndUpdateCalmness(character.CharacterState, treesCount));
                    }
                }
            }
        }
    }

    #endregion

    #region treeRadiusCalmessTalent

    public void TreeRadiusCalmessTalentActive(bool value) => treeRadiusCalmessTalent = value;

    #endregion

    #region PhysicsTalent

    public void HuntressMarkPhysicsTalentActive(bool value) => huntressMarkPhysicsTalent = value;
    public void ElvenSkillPhysicsTalent(bool value) => elvenSkillPhysicsTalent = value;
    public void CalmnessOnElvenSkillTalent(bool value) => calmnessOnElvenSkillTalent = value;
    public void SuppressionManaAbsorption(bool value) => suppressionManaAbsorptionTalent = value;

    [Command]
    private void CmdOnDamageTracked(Damage damage, GameObject target)
    {
        OnDamageTracked(damage, target);
    }

    private void OnDamageTracked(Damage damage, GameObject target)
    {
        if (damage.Type == DamageType.Physical && hero != null && hero.CharacterState != null)
        {
            CharacterState selfState = hero.CharacterState;

            if (elvenSkillPhysicsTalent && UnityEngine.Random.Range(0f, 100f) <= elvenSkillFromPhysChance) 
                selfState.AddState(States.ElvenSkill, durationElvenSkill, 0f, gameObject, "TerrifyingElfAura");

            else if (calmnessOnElvenSkillTalent && selfState.CheckForState(States.ElvenSkill) && UnityEngine.Random.Range(0f, 100f) <= calmnessOnElvenSkillChance)
                selfState.AddState(States.Calmness, durationCalmess, 0f, gameObject, "TerrifyingElfAura");

            //if (huntressMarkPhysicsTalent && UnityEngine.Random.Range(0f, 100f) <= huntressMarkApplyChance && target != null && target.TryGetComponent<CharacterState>(out var victimState))
            //    victimState.AddState(States.HuntressMark, durationHuntressMark, 0f, gameObject, "HuntressMark");

            if (suppressionManaAbsorptionTalent && selfState.GetState(States.Suppression) is SuppressionState suppression && suppression.CurrentStacksCount > 0 && hero.TryGetResource(ResourceType.Mana) is Mana mana)
                mana.Add(damage.Value * 0.25f * suppression.CurrentStacksCount);

            if (manaAbsorptionPhysicalTalent) OnDamageDealt(damage, target);
        }
    }


    #endregion

    #region ManaAbsorptionPhysicalTalent

    public void ManaAbsorptionPhysicalTalentActive(bool value)
    {
        manaAbsorptionPhysicalTalent = value;
    }

    private void OnDamageDealt(Damage damage, GameObject target)
    {
        if (damage.Type == DamageType.Physical && hero != null)
        {
            float manaToRestore = damage.Value * 0.3f;
            RestoreMana(manaToRestore, target);
        }
    }

    private void RestoreMana(float amount, GameObject target)
    {
        if (target != null && target.TryGetComponent<Character>(out var targetCharacter))
        {
            if (targetCharacter.TryGetResource(ResourceType.Mana) is Mana targetManaResource)
            {
                float manaToReduce = Mathf.Min(amount, targetManaResource.CurrentValue);
                if (manaToReduce > 0) targetManaResource.TryUse(amount);

                else return;
            }
        }

        if (hero.TryGetResource(ResourceType.Mana) is Mana manaResource) manaResource.Add(amount);
    }

    #endregion

    #region The Elf has run out of mana

    private void ApplyElvenSkill()
    {
        if (elvenSkillTalent && hero == null && hero.CharacterState == null) return;

        hero.CharacterState.CmdAddState(States.ElvenSkill, durationElvenSkill, 0f, gameObject, "TerrifyingElfAura");
    }

    public void ElvenSkillTalent(bool value)
    {
        elvenSkillTalent = value;
    }

    #endregion

    #region Helpers
    private int GetTreesCountInRadius(float radius)
        {
            var trees = FindObjectsOfType<Tree>();
            int count = 0;
            foreach (var t in trees)
            {
                if (Vector3.Distance(t.transform.position, transform.position) <= radius)
                {
                    count++;
                }
            }
            return count;
        }

        private IEnumerator DelayAndUpdateCalmness(CharacterState targetState, int treesCount)
        {
            yield return null;

            //if (!isServer) yield break;

            var calmness = targetState.GetState(States.Calmness) as Calmness;
            if (calmness != null)
            {
                calmness.UpdateTreesCount(treesCount);
            }
        }
    #endregion
}
