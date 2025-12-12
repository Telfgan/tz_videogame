
public class ShoolTalent : Talent
{
    private int _counter = 0;
    private int _maxSkills = 2;
    private float _multiple = 0.3f;
    private Skill _skill;
    private Skill _skill1;
    private Skill _skill2;

    public override void Enter()
    {
        foreach (var item in character.Abilities.Abilities)
        {
            item.CastStarted += OnCastStarted;
        }
    }

    public override void Exit()
    {
        foreach (var item in character.Abilities.Abilities)
        {
            item.CastStarted -= OnCastStarted;
        }
    }

    private void OnCastStarted()
    {
        _counter++;

        if (_counter == _maxSkills)
        {
            foreach (var item in character.Abilities.Abilities)
            {
                item.PreparingStarted += OnPreparingStarted;
            }
        }
    }

    private void OnPreparingStarted(Skill skill)
    {
        _skill = skill;

        skill.PreparingStarted -= OnPreparingStarted;

        foreach (var item in skill.SkillEnergyCosts)
        {
            item.ModifyResourceCost(_multiple);
        }

        skill.CastEnded += OnCastEnded;
    }

    private void OnCastEnded()
    {
        _skill.CastEnded -= OnCastEnded;

        foreach (var item in _skill.SkillEnergyCosts)
        {
            item.ModifyResourceCost1(_multiple);
        }
    }
}
