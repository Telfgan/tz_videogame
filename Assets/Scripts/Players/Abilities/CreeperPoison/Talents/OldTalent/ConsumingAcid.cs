public class ConsumingAcid : Talent
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
