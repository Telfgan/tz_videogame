using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AutoAttackQueue : MonoBehaviour
{
    private List<AutoAttackSkill> _autoAttackSkills = new List<AutoAttackSkill>();
    private List<Coroutine> _autoAttacksJob = new List<Coroutine>();

    public bool IsBusy { get => _autoAttackSkills.Count > 0; }

    public void Add(AutoAttackSkill skill)
    {
        _autoAttackSkills.Add(skill);
        _autoAttacksJob.Add(StartCoroutine(TryStartAutoAttack(skill)));
    }

    public bool TryCancel()
    {
        if (_autoAttacksJob.Count > 0)
        {
            foreach (var item in _autoAttacksJob)
            {
                if (item != null) StopCoroutine(item);
            }
            _autoAttacksJob.Clear();

            foreach (var item in _autoAttackSkills)
            {
                item.TryCancel(true);
            }
            _autoAttackSkills.Clear();

            return true;
        }
        else
        {
            return false;
        }
    }

    public void Pause()
    {
        foreach (var item in _autoAttackSkills)
        {
            item.Pause();
        }
    }

    public void Continue()
    {
        foreach (var item in _autoAttackSkills)
        {
            item.Continue();
        }
    }

    private IEnumerator TryStartAutoAttack(AutoAttackSkill skill)
    {
        while(skill.IsCasting == false)
        {
            skill.TryCast();
            yield return null;
        }
    }
}
