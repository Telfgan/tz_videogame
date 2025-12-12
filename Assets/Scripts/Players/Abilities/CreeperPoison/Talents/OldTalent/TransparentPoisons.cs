using UnityEngine;

public class TransparentPoisons : Talent
{
    [SerializeField] private PoisonBall _poisonBall;
    [SerializeField] private SpitPoison _spitPoison;

    private float _increaseManaCostValue = 1.3f;

    public override void Enter()
    {
        Debug.Log("TransparentPoisons / Enter");
        SetActive(true);
    }

    public override void Exit()
    {
        SetActive(false);
    }

    public void IncreaseManaCost()
    {
        _poisonBall.Buff.ManaCost.IncreasePercentage(_increaseManaCostValue);
        _spitPoison.Buff.ManaCost.IncreasePercentage(_increaseManaCostValue);
    }
}
