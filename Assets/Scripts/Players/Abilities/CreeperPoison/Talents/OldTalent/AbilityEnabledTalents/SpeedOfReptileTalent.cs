using UnityEngine;

public class SpeedOfReptileTalent : Talent
{
    [SerializeField] SpeedOfReptile _speedOfReptile;
    [SerializeField] private SkillManager _skillManager;

    public override void Enter()
    {
        SetActive(true);
        _skillManager.ActivateSkill(_speedOfReptile);

    }

    public override void Exit()
    {
        SetActive(false);
        _skillManager.DeactivateSkill(_speedOfReptile);

    }
}
