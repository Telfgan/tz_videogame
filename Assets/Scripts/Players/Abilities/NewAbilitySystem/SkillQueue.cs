using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillQueue : MonoBehaviour
{
    [SerializeField] private SkillRenderer _skillRenderer;

    private Queue<Skill> _skills = new Queue<Skill>();
    private Skill _currentSkill = null;
    private TargetInfo _targetInfo;

    public bool IsBusy { get => _currentSkill != null; }
    public bool IsEmpty { get => _skills.Count == 0; }
    public Skill CurrentSkill { get => _currentSkill; }
    public Queue<Skill> Skills => _skills;

    public event Action<Skill> SkillAdded;
    public event Action<Skill> SkillDeleted;

    private void Update()
    {
        if (IsBusy)
            return;
        
        if (_skills.TryPeek(out Skill skill))
        {
            if (skill.SkillType == SkillType.Zone)
                Draw(skill);

            if (!skill.Disactive)
            {
                if (skill.TargetInfoQueue.TryPeek(out TargetInfo targetInfo))
                {
                    _targetInfo = targetInfo;
                    foreach (var item in _targetInfo.GetTargets())
                    {
                        if (item is Character character)
                        {
                            character.SelectedCircle.SwitchSelectCircle(true);
                        }
                    }
                }
            }

            if (!skill.Disactive && skill.TryCast())
            {
                RemoveFromQueue();
                _currentSkill = skill;

                if (_currentSkill.TargetInfoQueue.TryPeek(out TargetInfo targetInfo))
                {
                    _targetInfo = targetInfo;
                    foreach (var item in _targetInfo.GetTargets())
                    {
                        if (item is Character character)
                        {
                            character.SelectedCircle.SwitchSelectCircle(true);
                        }
                    }
                }

                _currentSkill.CastEnded += OnCastEnded;
            }
        }
    }

    public void Add(Skill skill)
    {
        //if (_skills.Contains(skill))
        //return; 
        if (skill is IPassiveSkill) return;

        _skills.Enqueue(skill);
        SkillAdded?.Invoke(skill);
    }

    public bool TryCancel(bool isForceCancel = false)
    {
        if (_currentSkill != null)
        {
            _currentSkill.TryCancel(isForceCancel);
            if (_currentSkill.TargetInfoQueue.TryPeek(out TargetInfo target)) ToggleSelectCircles(target, false);


            if (_currentSkill.TargetInfoQueue.TryPeek(out TargetInfo targetInfo))
            {
                _targetInfo = targetInfo;
                foreach (var item in _targetInfo.GetTargets())
                {
                    if (item is Character character)
                    {
                        character.SelectedCircle.SwitchSelectCircle(false);
                    }
                }
            }
            return true;
        }

        var queuedSkill = RemoveFromQueue();

        if (queuedSkill != null)
        {
            if (queuedSkill.TargetInfoQueue.Count > 0)
            {
                var temp = queuedSkill.TargetInfoQueue.Dequeue();

                foreach (var item in temp.GetTargets())
                {
                    if (item is Character character)
                    {
                        character.SelectedCircle.SwitchSelectCircle(false);
                    }
                }

                queuedSkill.TryCancel(isForceCancel);
                return true;
            }
        }

        return false;
    }

    private void Draw(Skill skill)
    {
        if (_skillRenderer == null) return;
        if (skill.TargetInfoQueue == null || skill.TargetInfoQueue.Count == 0) return;

        var info = skill.TargetInfoQueue.Peek().Points;
        if (info == null || info.Count == 0) return;

        Vector3[] vector3s = new Vector3[info.Count];
        for (int i = 0; i < info.Count; i++) vector3s[i] = new Vector3(info[i].x, info[i].y + 0.1f, info[i].z);

        _skillRenderer.StartDrawAllLineForZone(vector3s);
        _skillRenderer.DrawRadius(skill.Radius);
    }

    private Skill RemoveFromQueue()
    {
        if (_skills.Count == 0) return null;

        var temp = _skills.Dequeue();
        SkillDeleted?.Invoke(temp);

        if (temp.SkillType == SkillType.Zone)
        {
            _skillRenderer.StopDrawRadius();
            _skillRenderer.StopDrawAllLineForZone();
        }

        return temp;
    }


    private void OnCastEnded()
    {
        _currentSkill.CastEnded -= OnCastEnded;


        if (_currentSkill.TargetInfoQueue.TryPeek(out TargetInfo targetInfo))
        {
            _targetInfo = targetInfo;
            foreach (var item in _targetInfo.GetTargets()) if (item is Character character) character.SelectedCircle.SwitchSelectCircle(false);
        }

        _currentSkill = null;
    }

    private static void ToggleSelectCircles(TargetInfo info, bool isOn)
    {
        if (info?.GetTargets() == null) return;
        foreach (var target in info.GetTargets()) if (target is Character character && character?.SelectedCircle != null) character.SelectedCircle.SwitchSelectCircle(isOn);
    }
}