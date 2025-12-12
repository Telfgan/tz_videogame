using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SkillQueue))]
[RequireComponent(typeof(AutoSkillQueue))]
[RequireComponent(typeof(AutoAttackQueue))]
public class SkillManager : MonoBehaviour
{
    [SerializeField] private List<Skill> _skills;
    [SerializeField] private Character _hero;
    [SerializeField] private TalentSystem _talentSystem;
    [SerializeField] private SkillRenderer _skillRenderer;

    private Skill[] _selectedSkills = new Skill[16];
    private List<AutoAttackSkill> _autoAttackSkills = new List<AutoAttackSkill>();
    //private List<AutoSkill> _autoSkills = new List<AutoSkill>();
    private List<Skill> _simpleSkills = new List<Skill>();
    private float _globalCooldownTime = .5f;
    private SkillQueue _skillQueue;
    //private AutoSkillQueue _autoSkillQueue;
    private AutoSkillCast _autoSkillCast;
    //private AutoAttackQueue _autoAttackQueue;
    private Skill _selectedSkill;
    private Coroutine _lastCastResetCoroutine;
    private int _castWindowId = 0;
    private int _countBonusCharges = 0;

    private Dictionary<Skill, Action> _castEndedHandlers = new();

    public TalentSystem TalesntSystem => _talentSystem;
    public Skill LastCastedSkill { get; private set; }
    public Skill PreviewCastedSkill { get; private set; }
    public SkillQueue SkillQueue { get => _skillQueue; }
    public Skill[] SelectedSkills { get => _selectedSkills; }
    public bool IsNextSkillFree { get; private set; }
    public IEnumerable<Skill> DefaultSkills => _skills.Where(o => o.IsTalentSpell == false);
    public IEnumerable<Skill> TalentsSkills => _skills.Where(o => o.IsTalentSpell);

    public List<Skill> Abilities => _skills;
    public event Action<int> SkillSelected;
    public event Action<int> SkillDeselected;
    public event Action<Skill> SkillAdded;
    public event Action<Skill> SkillRemoved;

    private void OnEnable()
    {
        foreach (var skill in _skills)
        {
            void Handler() => OnSkillCastEnded(skill);
            _castEndedHandlers[skill] = Handler;
            skill.CastEnded += Handler;
        }
    }

    private void OnDisable()
    {
        foreach (var skill in _skills)
        {
            if (_castEndedHandlers.TryGetValue(skill, out var handler))
            {
                skill.CastEnded -= handler;
            }
        }
        _castEndedHandlers.Clear();
    }

    private void Awake()
    {
        InputHandler.ScrollMouse += ScrollMouse;

        _skillQueue = GetComponent<SkillQueue>();
        _autoSkillCast = new(this);
        //_autoAttackQueue = GetComponent<AutoAttackQueue>();
        //_autoSkillQueue = GetComponent<AutoSkillQueue>();

        //_autoSkillQueue.SkillActivated += AutoSkillUsed;

        foreach (var item in _skills)
        {
            AddToSkillLists(item);
            SkillInit(item);
        }
    }

    #region Test
    private void OnSkillCastEnded(Skill skill)
    {
        if (!(skill is IPassiveSkill))
        {
            PreviewCastedSkill = LastCastedSkill;
            LastCastedSkill = skill;
            _castWindowId++;

            if (_lastCastResetCoroutine != null) StopCoroutine(_lastCastResetCoroutine);
            _lastCastResetCoroutine = StartCoroutine(CastWindowResetCoroutine(_castWindowId));

            Debug.Log($"PreviewCastedSkill: {PreviewCastedSkill}");
            Debug.Log($"LastCastedSkill: {LastCastedSkill}");
        }
    }

    private IEnumerator CastWindowResetCoroutine(int id)
    {
        yield return new WaitForSeconds(3f);

        if (_castWindowId != id)
            yield break;
        PreviewCastedSkill = null;
        LastCastedSkill = null;

    }
    #endregion

    public void CancleAllSkills()
    {
        while (_selectedSkill != null && _selectedSkill.IsPreparing)
        {
            CancelSkillCast();
        }
        while (SkillQueue.IsBusy)
        {
            CancelSkillCast();
        }
        while (_autoSkillCast.IsBusy)
        {
            CancelSkillCast();
        }
        while (SkillQueue.IsEmpty == false)
        {
            CancelSkillCast();
        }
    }

    private void ScrollMouse(float value)
    {
        if (_selectedSkill == null) return;

        var index = Array.IndexOf(_selectedSkills, _selectedSkill);

        if (value > 0)
        {
            if (index - 1 < 0)
            {
                index = _skills.Count;
            }
            SelectSkill(index - 1);
        }
        if (value < 0)
        {
            if (index >= _skills.Count)
            {
                index = 0;
            }
            SelectSkill(index + 1);
        }

    }

    private void AddToSkillLists(Skill skill)
    {
        _simpleSkills.Add(skill);
        skill.CastStarted += GlobalCooldown;
    }

    private void SkillInit(Skill skill)
    {
        skill.Init(_skillRenderer, _hero);

        foreach (var simpleSkill in _simpleSkills)
        {
            foreach (var autoAttackSkill in _autoAttackSkills)
            {
                simpleSkill.CastStarted += autoAttackSkill.Pause;
                simpleSkill.CastEnded += autoAttackSkill.Continue;
            }
        }
        ToggleSkillActivation(skill);
    }

    public void ActivateSkill(Skill skill)
    {
        for (int i = 0; i < _selectedSkills.Length; i++)
        {
            if (_selectedSkills[i] == null && !_selectedSkills.Contains(skill))
            {
                _selectedSkills[i] = skill;
                break;
            }
        }
        TalentSkillAdd(skill);

		skill.IsSkillActive = true;
        SkillAdded?.Invoke(skill);
    }

    public void SkillPanelUpdate()
    {
        SkillAdded?.Invoke(null);
    }

    public bool TryConsumeNextSkillFree()
    {
        if (!IsNextSkillFree)
            return false;

        IsNextSkillFree = false;
        return true;
    }

    public void SetNextSkillFree() => IsNextSkillFree = true;

    public void DeactivateSkill(Skill skill)
    {
        for (int i = 0; i < _selectedSkills.Length; i++)
        {
            if (_selectedSkills[i] == skill && _selectedSkills.Contains(skill))
            {
                _selectedSkills[i] = null;
                break;
            }
        }

        skill.IsSkillActive = false;
        SkillRemoved?.Invoke(skill);
    }

    private void ToggleSkillActivation(Skill skill)
    {
        if (skill.IsSkillActive)
        {
            ActivateSkill(skill);
        }
        else
        {
            DeactivateSkill(skill);
        }
    }

    public void SetAbilitiesCoolDown(float time)
    {
        foreach (var item in _skills)
        {
            item.IncreaseSetCooldown(time);
        }
    }

    public void OnSelect(bool value)
    {
        if (value)
        {
            InputHandler.OnShiftLeftMouse += PrepereSkill;
            InputHandler.OnAltClick += CancelSkillCast;

            InputHandler.OnCast += OnCastSelect;
        }
        else
        {
            InputHandler.OnShiftLeftMouse -= PrepereSkill;
            InputHandler.OnAltClick -= CancelSkillCast;

            InputHandler.OnCast -= OnCastSelect;

            if (_selectedSkill != null && _selectedSkill.IsPreparing)
            {
                _selectedSkill.TryCancel();

                DeselectSkill();
            }
        }
    }

    private void PrepereSkill()
    {
        if (_selectedSkill != null)
        {
            _selectedSkill.TryPreparing();
        }
    }

    private void CancelSkillCast()
    {
        if (_selectedSkill != null && _selectedSkill.IsPreparing)
        {
            _selectedSkill.TryCancel();

            if (_selectedSkill is AutoAttackSkill aa)
                DeselectSkill();
        }
        else if (SkillQueue.IsBusy)
        {
            SkillQueue.TryCancel();
        }
        else if (_autoSkillCast.IsBusy)
        {
            _autoSkillCast.DeleteSkill();
        }
        else if (SkillQueue.IsEmpty == false)
        {
            SkillQueue.TryCancel();
        }
        /*else if(_selectedSkill != null)
        {
            DeselectSkill();
        }*/ // not need now, but not deleted
    }

    private void OnCastSelect(int index)
    {
        if (SelectSkill(index))
        {
            PrepereSkill();
        }
    }

    private bool SelectSkill(int index)
    {
        if (_selectedSkills[index] == null) return false;
        if (_selectedSkills[index] is IPassiveSkill) return false;

        if (_selectedSkill != null && _selectedSkill.IsPreparing == true)
        {
            if (_selectedSkill != _selectedSkills[index])
            {
                _selectedSkill.TryCancel(true);

                DeselectSkill();
                SetSelectSkill(_selectedSkills[index]);
                PrepereSkill();
            }
            return false;
        }

        if (_selectedSkill == _selectedSkills[index])
        {
            SkillSelected?.Invoke(index);
        }
        else if (_selectedSkill == null)
        {
            SetSelectSkill(_selectedSkills[index]);

        }
        else if (_selectedSkill != _selectedSkills[index])
        {
            DeselectSkill();
            SetSelectSkill(_selectedSkills[index]);
        }
        return true;
    }

    private void SetSelectSkill(Skill skill)
    {
        _selectedSkill = skill;
        SubscribingSkillOnEvents(_selectedSkill);
        SkillSelected?.Invoke(Array.IndexOf(_selectedSkills, skill));
    }

    private void DeselectSkill()
    {
        int index = Array.IndexOf(_selectedSkills, _selectedSkill);

        if (index == -1)
            return;

        SkillDeselected?.Invoke(index);
        UnsubscribingSkillOnEvents(_selectedSkill);
        _selectedSkill = null;
    }

    private void GlobalCooldown()
    {
        foreach (var item in _skills)
        {
            if (item.IsSubjectToGlobalCooldownTime)
                item.IncreaseSetCooldown(_globalCooldownTime);
        }
    }

    private void SubscribingSkillOnEvents(Skill skill)
    {
        skill.PreparingSuccess += OnPreperingSuccess;
    }

    private void UnsubscribingSkillOnEvents(Skill skill)
    {
        if (skill == null)
            return;

        skill.PreparingSuccess -= OnPreperingSuccess;
    }

    private void OnPreperingSuccess(Skill skill)
    {
        if (skill is IPassiveSkill) return;

        if (_selectedSkill.IsAutoMode)
        {
            _autoSkillCast.SetSkill(skill, skill.TargetInfoQueue.Dequeue());

            foreach (var item in _simpleSkills)
            {
                if (_selectedSkill == item)
                    return;

                item.CastStarted += _autoSkillCast.Pause;
                item.CastEnded += _autoSkillCast.Continue;
            }
            DeselectSkill();
        }
        else
        {
            SkillQueue.Add(_selectedSkill);
        }
    }

    private void AutoSkillUsed(Skill skill)
    {
        foreach (var item in _skills)
        {
            if (item != skill)
            {
                item.TryCancel(true);
            }
        }
    }

    private void TalentSkillAdd(Skill skill)
    {
        if (_countBonusCharges > 0)
        {
            if (skill.IsUseCharges)
            {
                _countBonusCharges--;
                skill.AddMaxChargeCount();
            }
        }
    }

    public void TalentAddCharges(int countBonusCharges)
    {
        _countBonusCharges = countBonusCharges;
    }

    #region legacycode
    private void OnDestroy()
    {
        /*
        AbilitiesManager.Instance.RemovePanel(_abilityPanel);
        */
    }

    public void SetAbilitiesPanelSelect(bool isSelect)
    {
        /*
        AbilitiesManager.Instance.ChangeCurrentPanelSelectStatus(_abilityPanel, isSelect);
        if (isSelect) EnableAbilities();
        else DisableAbilities();
        */
    }
    public void SetAbilitiesPanelEnable()
    {
        /*
        AbilitiesManager.Instance.ActiveCurrentPanel(_abilityPanel);
        */
    }
    public void SetAbilitiesDisactive(bool value)
    {
        foreach (var ability in Abilities) ability.Disactive = value;
    }

    public void SetPhysicalAbilitiesDisactive(bool state)
    {
        foreach (Skill skill in Abilities) if (skill.AbilityForm == AbilityForm.Physical) skill.Disactive = state;
    }

    public void SwitchAvaliable(Schools school, bool value)
    {
        /*
        if (school == Schools.Physical)
            return;
        foreach (var item in _abilities)
        {
            if (item.School == school)
            {
                item.SwitchAvailible(value);
                //item.KnockDownTimerStart(coolDown);
            }
        }
        */
    }

    public void SwitchAvaliable(AbilityForm form, bool value)
    {
        /*
        foreach (var item in _abilities)
        {
            if (item.AbilityForm == form)
            {
                item.SwitchAvailible(value);
                //item.KnockDownTimerStart(coolDown);
            }
        }
        */
    }
    #endregion
}