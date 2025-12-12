public class BladeMailPriestTalent : Talent
{
    private bool _isTalentActive;
    public bool IsTalentActive => _isTalentActive;
    
    public override void Enter()
    {
        _isTalentActive = true;
    }

    public override void Exit()
    {
        _isTalentActive = false;
    }
}
