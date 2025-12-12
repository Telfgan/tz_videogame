using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSkillQueue : MonoBehaviour
{
    private List<Skill> _skills = new();
    private Skill _currentSkill = null;

    public bool IsBusy { get => _currentSkill != null; }
    public bool IsEmpty { get => _skills.Count == 0; }
    public Skill CurrentSkill { get => _currentSkill; }

    public event Action<Skill> SkillAdded;
    public event Action<Skill> SkillDeleted;
    public event Action<Skill> SkillActivated;

    private void Update()
    {
        if (IsBusy || IsEmpty)
            return;

        TryUseSkill();
    }

    public void Add(Skill skill)
    {
        if (_skills.Contains(skill))
            return;

        _skills.Add(skill);
        SkillAdded?.Invoke(skill);
    }

    public bool TryCancel(bool foceCancel = false)
    {
        if (_currentSkill != null)
        {
            _currentSkill.TryCancel(foceCancel);

            return true;
        }
        else if (IsEmpty == false)
        {
            foreach (var item in _skills)
            {
                SkillDeleted?.Invoke(item);
            }
            _skills.Clear();

            return true;
        }
        return false;
    }

    private bool TryUseSkill()
    {
        foreach (var item in _skills)
        {
            if (item.TryCast())
            {
                _currentSkill = item;
                _currentSkill.CastEnded += OnCastEnded;

                foreach (var temp in _skills)
                {
                    SkillDeleted?.Invoke(temp);
                }
                _skills.Clear();

                SkillActivated?.Invoke(item);

                return true;
            }
        }
        return false;
    }

    private void OnCastEnded()
    {
        _currentSkill.CastEnded -= OnCastEnded;
        _currentSkill = null;
    }
}
