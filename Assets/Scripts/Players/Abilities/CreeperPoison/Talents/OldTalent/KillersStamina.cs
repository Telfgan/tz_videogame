public class KillersStamina : Talent
{
    public override void Enter()
    {
        SetActive(true);

    }

    public override void Exit()
    {
        SetActive(false);
    }
}
