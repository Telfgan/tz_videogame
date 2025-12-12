using System.Collections.Generic;

public class ReversePolarityState : AbstractCharacterState
{
    public override States State => States.ReversePolarity;
    public override StateType Type => StateType.Immaterial;
    public override List<StatusEffect> Effects => new List<StatusEffect>();
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
    }

    public override bool Stack(float time)
    {
        return false;
    }
}
