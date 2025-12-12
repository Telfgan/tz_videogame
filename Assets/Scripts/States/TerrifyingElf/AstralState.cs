using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstralState : AbstractCharacterState
{
    private float _baseDuration;
    private float _duration;
    private int _currentStacks = 1;
    private const int _maxStacks = 1;

    private float _defMagDamageMod = 50f;
    private float _originalRegenerationValue;
    private float _originalDefPhysDamage;

    private StateEffects _stateEffects;
    private SkinnedMeshRenderer _characterRenderer;
    private GameObject _weapon;
    private Renderer _weaponRenderer;
    private Material _originalWeaponMaterial;
    private Material[] _originalMaterials;

    private Coroutine _dotJob;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Ability, StatusEffect.Move };
    private readonly Dictionary<Skill, float> _modifiedSkills = new();

    public override States State => States.Astral;
    public override StateType Type => StateType.Magic;
    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        Debug.Log("Entering Astral State");

        _characterState = character;
        _personWhoMadeBuff = personWhoMadeBuff;
        _baseDuration = durationToExit;
        _duration = _baseDuration;

        _stateEffects = _characterState.GetComponent<StateEffects>();
        if (_stateEffects == null)
        {
            Debug.LogWarning("StateEffects component is missing on character.");
            return;
        }

        _characterRenderer = _characterState.GetComponentInChildren<SkinnedMeshRenderer>();
        _weapon = _stateEffects.Weapon;

        if (_characterRenderer != null)
        {
            _originalMaterials = _characterRenderer.materials;
            Material[] ghostMaterials = new Material[_originalMaterials.Length];
            for (int i = 0; i < ghostMaterials.Length; i++)
            {
                ghostMaterials[i] = _stateEffects.MaterialGhost;
            }
            _characterRenderer.materials = ghostMaterials;
        }

        if (_weapon != null && (_weaponRenderer = _weapon.GetComponent<Renderer>()) != null)
        {
            _originalWeaponMaterial = _weaponRenderer.material;
            _weaponRenderer.material = _stateEffects.MaterialGhost;
        }

        var characterHealth = _characterState.Character.Health;

        _originalDefPhysDamage = characterHealth.DefPhysDamage;
        _originalRegenerationValue = characterHealth.RegenerationValue;
        characterHealth.DefMagDamage -= _defMagDamageMod;
        characterHealth.DefPhysDamage = 100;

        _characterState.Character.Health.RegenerationValue = 0;
        _characterState.Character.Move.ChangeMoveSpeed(0.5f);

        BlockPhysicalAbilities();

        foreach (var skill in _characterState.Character.Abilities.Abilities)
        {
            if (skill.AbilityForm == AbilityForm.Magic || skill.AbilityForm == AbilityForm.Spell)
            {
                _modifiedSkills[skill] = skill.Damage;
                skill.Damage *= 1.5f;
            }
        }

        if (_characterState.isServer) _dotJob = _characterState.StartCoroutine(DotJob());
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;
        if (_duration <= 0)
        {
            ExitState();
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Astral State");

        _characterState.RemoveState(this);

        if (_characterRenderer != null) _characterRenderer.materials = _originalMaterials;
        if (_weapon != null) _weaponRenderer.material = _originalWeaponMaterial;

        var characterHealth = _characterState.Character.Health;

        characterHealth.DefMagDamage += _defMagDamageMod;
        characterHealth.DefPhysDamage = _originalDefPhysDamage;
        _characterState.Character.Move.ChangeMoveSpeed(2);

        if (_dotJob != null) _characterState.StopCoroutine(_dotJob);
        characterHealth.RegenerationValue = _originalRegenerationValue;
        UnblockPhysicalAbilities();

        foreach (var (skill, baseDamage) in _modifiedSkills) skill.Damage = baseDamage;
        _modifiedSkills.Clear();

        _characterState.RemoveState(this);
    }

    private void BlockPhysicalAbilities()
    {
        foreach (var skill in _characterState.Character.Abilities.Abilities)
            if (skill.AbilityForm == AbilityForm.Physical) skill.Disactive = true;
    }

    private void UnblockPhysicalAbilities()
    {
        foreach (var skill in _characterState.Character.Abilities.Abilities)
            if (skill.AbilityForm == AbilityForm.Physical) skill.Disactive = false;
    }

    public override bool Stack(float time)
    {
        if (_currentStacks < _maxStacks) _currentStacks++;

        _duration = _baseDuration;
        return true;
    }

    private IEnumerator DotJob()
    {
        float period = _characterState.Character.Health.RegenerationDelay;
        if (period <= 0) period = 1f;

        while (true)
        {
            yield return new WaitForSeconds(period);

            float damage = _originalRegenerationValue;
            if (damage > 0) _characterState.Character.Health.TryUse(damage);
        }
    }
}
