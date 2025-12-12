using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSkillCast
{
    private Skill _currentSkill;
    private TargetInfo _targetInfo;
    private Coroutine _tryCastCoroutine;
    private MonoBehaviour _parentForCoroutine;

    public bool IsBusy { get { return _currentSkill != null; } }

    public AutoSkillCast(MonoBehaviour parentForCoroutine)
    {
        _parentForCoroutine = parentForCoroutine;
    }

    public void SetSkill(Skill skill, TargetInfo targetInfo)
    {
        _currentSkill = skill;
        _targetInfo = new();
     //   _targetInfo._targets = new(targetInfo._targets);
        _targetInfo.AddTargets(targetInfo.GetTargets());
        _targetInfo.Points = new(targetInfo.Points);
        _tryCastCoroutine = _parentForCoroutine.StartCoroutine(TryCastJob());

        _currentSkill.SkillRender.StartDrawAutoAttackRadius(_currentSkill.Radius);
    }

    public void DeleteSkill()
    {
        if (_currentSkill == null) return;
        _currentSkill.TryCancel(true);

        _currentSkill.SkillRender.StopDrawAutoAttackRadius();

        if (_currentSkill.Hero != null && _currentSkill.Hero.Move != null)  _currentSkill.Hero.Move.StopLookAt();

        StopTryCastCoroutine();

        _currentSkill = null;
    }

    public void Pause()
    {
        if (_currentSkill == null) return;

        _currentSkill.TryCancel(true);

        if (_currentSkill.Hero != null && _currentSkill.Hero.Move != null)
            _currentSkill.Hero.Move.StopLookAt();

    }

    public void Continue()
    {
        if (_tryCastCoroutine == null && _currentSkill != null)
        {
            _tryCastCoroutine = _parentForCoroutine.StartCoroutine(TryCastJob());

            _currentSkill.SkillRender.StartDrawAutoAttackRadius(_currentSkill.Radius);
        }
    }

    private void StopTryCastCoroutine()
    {
        if (_tryCastCoroutine != null)
        {
            _parentForCoroutine.StopCoroutine(_tryCastCoroutine);
            _tryCastCoroutine = null;
        }

        if (_currentSkill != null && _currentSkill.SkillRender != null)
        {
            _currentSkill.SkillRender.StopDrawAutoAttackRadius();
        }

        if (_targetInfo?.GetTargets() != null)
        {
            foreach (var item in _targetInfo.GetTargets())
            {
                if (item is Character character && character?.SelectedCircle != null)
                {
                    character.SelectedCircle.SwitchSelectCircle(false);
                }
            }
        }
    }


    private IEnumerator TryCastJob()
    {
        foreach (var item in _targetInfo.GetTargets())
        {
            if (item is Character character)
            {
                character.SelectedCircle.SwitchSelectCircle(true);
            }
        }

        while (true)
        {
            if (_targetInfo.GetTargets().Count > 0 )
            {
                if (_targetInfo.GetTargets()[0] is Character character)
                {
                    _currentSkill.Hero.Move.LookAtTransform(character.transform);
                    _currentSkill.Hero.Move.IsLookAtCursor = false;
                }
            }
            else if (_targetInfo.Points.Count > 0)
            {
                _currentSkill.Hero.Move.LookAtPosition(_targetInfo.Points[0]);
                _currentSkill.Hero.Move.IsLookAtCursor = false;
            }

            _currentSkill.TryCast(_targetInfo);

            yield return new WaitForSeconds(_currentSkill.AutoAttackDelay);
        }
    }

}
