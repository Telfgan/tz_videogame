using UnityEngine;

public class MetabolismReptileTalent : Talent
{
    [SerializeField] MetabolismReptile _metabolismReptile;
    [SerializeField] private SkillManager _skillManager;

    public override void Enter()
    {
        SetActive(true);
        _skillManager.ActivateSkill(_metabolismReptile);

    }

    public override void Exit()
    {
        SetActive(false);
        _skillManager.DeactivateSkill(_metabolismReptile);

    }
}
