using UnityEngine;

public class VoluminousBall : Talent
{
    [SerializeField] private PoisonBall _poisonBall;

    public float IncreasedSizeX = 1.2f;
    public float IncreasedSizeY = 1.2f;

    public override void Enter()
    {
        SetActive(true);
    }

    public override void Exit()
    {
        SetActive(false);
    }
}
