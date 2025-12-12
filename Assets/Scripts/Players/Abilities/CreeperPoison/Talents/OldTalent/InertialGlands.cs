public class InertialGlands : Talent
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
