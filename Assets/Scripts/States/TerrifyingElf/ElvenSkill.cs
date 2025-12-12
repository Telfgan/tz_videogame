using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ElvenSkill : AbstractCharacterState
{
    private float _duration;
    private MoveComponent _move;
    private GameObject _elvenSkillEffect;
    private TerrifyingElfAura _aura;
    private SkillManager _skillManager;

    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override States State => States.ElvenSkill;
    public override StateType Type => StateType.Physical;
    public override List<StatusEffect> Effects => _effects;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Ability };

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _duration = durationToExit;
        _characterState = character;
        _personWhoMadeBuff = personWhoMadeBuff;
        _move = character.GetComponent<MoveComponent>();
        _skillManager = _characterState.Character.Abilities;

        if (_characterState.TryGetComponent<Character>(out var ability))
        {
            foreach (var skill in _skillManager.Abilities)
            {
                if (skill.DamageType == DamageType.Physical)
                {
                    skill.CastStarted += OnPhysCastStarted;
                    skill.CastEnded += OnPhysCastFinished;
                    skill.Canceled += OnPhysCastFinished;
                }

                if (skill is ReconnaissanceFire reconnaissanceFire) reconnaissanceFire.TryStartElvenBoostWindow();
            }
        }

        _aura = character.GetComponent<TerrifyingElfAura>();
        if (_aura != null && _aura.ElvenSkillEffect != null)
        {
            _elvenSkillEffect = _aura.ElvenSkillEffect;
            _elvenSkillEffect.SetActive(true);
        }
    }

    public override void ExitState()
    {
        if (_move) _move.CanMoveState = false;

        if (_characterState.TryGetComponent<Character>(out var ability))
        {
            foreach (var skillPhysics in ability.Abilities.Abilities.Where(skillPhysics => skillPhysics.DamageType == DamageType.Physical))
            {
                skillPhysics.CastStarted -= OnPhysCastStarted;
                skillPhysics.CastEnded -= OnPhysCastFinished;
                skillPhysics.Canceled -= OnPhysCastFinished;
            }
        }

        if (_elvenSkillEffect != null) _elvenSkillEffect.SetActive(false);

        _characterState.StateIcons.RemoveItemByState(State);
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        return false;
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;

        if (_duration <= 0) ExitState();
    }

    private void OnPhysCastStarted()  
    {
        if (_move) _move.CanMoveState = true;
    }

    private void OnPhysCastFinished()
    {
        if (_move) _move.CanMoveState = false;
    }
}

