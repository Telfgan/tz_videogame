using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetInfo
{
    private List<ITargetable> _targets = new();
    public List<Vector3> Points = new();
    public List<Quaternion> Roation = new();

    public List<ITargetable> GetTargets(bool canTakeDead = false)
    {
        if (canTakeDead) return _targets;

        List<ITargetable> targets = new List<ITargetable>();
        foreach (ITargetable target in _targets)
        {
            if (target.IsTargetable)
            {
                targets.Add(target);
            }
        }
        return targets;
    }

    public void AddTarget(ITargetable target)
    {
        if (target == null)
            return;

        _targets.Add(target); 
    }

    public void AddTargets(List<ITargetable> targets)
    {
        _targets.AddRange(targets);
    }

}
