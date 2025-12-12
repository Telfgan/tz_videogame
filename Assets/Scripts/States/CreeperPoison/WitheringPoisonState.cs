using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WitheringPoisonState : AbstractCharacterState
{
    private List<Skill> _skills = new();
    private List<Talent> _talents = new();
    private BindingPoison _bindingPoison;
    private Character _player;

    private int _maxStacks = 3;

    private float _timeBetweenTakeAwayMana;
    private float _startTimeBetweenTakeAwayMana = 1f;

    private float _duration;
    private float _baseDuration;

    private float _baseValueTakeAwayMana = 0.003f;
    private float _endValueTakeAwayMana;
    private float _baseChanceOfApplyBindingPoison = 0.03f;
    private float _chanceOfApplyBindingPoison = 0.9f;

    private bool _isActiveTalentBindingPoison = false;

    public int CurrentStacks { get => CurrentStacksCount; set => CurrentStacksCount = value; }
    public float StacksDuration { get => _duration; }

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Poison };
    public override States State => States.WitheringPoison;
    public override StateType Type => StateType.Physical;
    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        MaxStacksCount = _maxStacks;

        _characterState = character;

        _duration = durationToExit;
        _baseDuration = durationToExit;

        _player = personWhoMadeBuff;

        if (_player != null)
        {
            _talents = _player.CharacterState.Character.GetComponent<HeroComponent>().TalentManager.ActiveTalents;

            foreach (Talent talent in _talents)
            {
                if (talent is BindingPoison bindingPoison)
                {
                    if (_bindingPoison == null)
                    {
                        _bindingPoison = bindingPoison;
                        _isActiveTalentBindingPoison = _bindingPoison.Data.IsOpen;
                    }
                }
            }
        }

        if (CurrentStacksCount < MaxStacksCount)
        {
            AddStacks();
        }
    }

    public override void UpdateState()
    {
        _timeBetweenTakeAwayMana -= Time.deltaTime;
        if (_timeBetweenTakeAwayMana <= 0)
        {
            TakeAwayMana();
            _timeBetweenTakeAwayMana = _startTimeBetweenTakeAwayMana;
        }

        if (CurrentStacksCount <= 0)
        {
            ExitState();
        }

        _duration -= Time.deltaTime;
        if (_duration < 0)
        {
            ExitState();
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
            AddStacks();
            return true;
        }
        else
        {
            _duration = _baseDuration;
            return true;
        }
    }

    public void AddStacks()
    {
        if (CurrentStacksCount < MaxStacksCount)
        {
            CurrentStacksCount++;
            _duration = _baseDuration;
        }
        else
        {
            _duration = _baseDuration;
        }
    }

    [Server]
    private void TakeAwayMana()
    {
        float takeAwayMana = CurrentStacksCount * _baseValueTakeAwayMana;

        _endValueTakeAwayMana = _characterState.Character.Resources.FirstOrDefault(r => r.Type == ResourceType.Mana)!.CurrentValue * takeAwayMana;

        _chanceOfApplyBindingPoison *= _baseChanceOfApplyBindingPoison;

        if (_bindingPoison != null && _isActiveTalentBindingPoison)
        {
            if (UnityEngine.Random.Range(0.0f, 1.0f) <= _chanceOfApplyBindingPoison)
            {
                _characterState.AddState(States.BindingPoison, 10f, 0, _player.gameObject, null);
            }
        }

        _characterState.Character.Resources.FirstOrDefault(r => r.Type == ResourceType.Mana)!.Add(-_endValueTakeAwayMana);
    }

    private void ResetValues()
    {
        CurrentStacksCount = 0;
        _baseDuration = 0;
        _duration = 0;
        _endValueTakeAwayMana = 0;
        _baseValueTakeAwayMana = 1f;
        _chanceOfApplyBindingPoison = 0f;
        _timeBetweenTakeAwayMana = _startTimeBetweenTakeAwayMana;
    }
}
