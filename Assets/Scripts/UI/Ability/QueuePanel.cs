using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QueuePanel : MonoBehaviour
{
    [SerializeField] private AbilityIcon _iconPref;

    private SkillQueue _skillQueue;
    private Queue<AbilityIcon> _skillsIcon = new Queue<AbilityIcon>();

    public void Init(SkillQueue queue)
    {
        if(_skillQueue != null)
        {
            _skillQueue.SkillAdded -= OnSkillAdded;
            _skillQueue.SkillDeleted -= OnSkillDeleted;
        }
        _skillQueue = queue;

        _skillQueue.SkillAdded += OnSkillAdded;
        _skillQueue.SkillDeleted += OnSkillDeleted;
    }

    private void OnSkillDeleted(Skill obj)
    {
        var temp = _skillsIcon.Dequeue();
        Destroy(temp.gameObject);
    }

    private void OnSkillAdded(Skill skill)
    {
        var temp = Instantiate(_iconPref, transform);
        temp.Init(skill);

        _skillsIcon.Enqueue(temp);
    }
}
