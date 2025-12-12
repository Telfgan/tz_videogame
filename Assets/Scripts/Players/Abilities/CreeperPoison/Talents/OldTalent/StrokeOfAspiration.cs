using UnityEngine;

public class StrokesOfAspiration : Talent
{
    [SerializeField] private CreeperStrike _creeperStrike;

    [SerializeField] private PoisonBall _poisonBall;
    [SerializeField] private SpitPoison _spitPoison;

    private const float _timeBetweenAttack = 0.1f;
    private const float _decreaseCooldownTime = 3.3f;
    public override void Enter()
    {
        SetActive(true);
        if (_creeperStrike.Buff.AttackSpeed.Multiplier > _timeBetweenAttack)
        {
            _creeperStrike.Buff.AttackSpeed.IncreasePercentage(_timeBetweenAttack);
        }
    }

    public override void Exit()
    {
        SetActive(false);
        if (_creeperStrike.Buff.AttackSpeed.Multiplier < 1.0f)
        {
            _creeperStrike.Buff.AttackSpeed.ReductionPercentage(_timeBetweenAttack);
            //Debug.Log("StrokeOfAspiration / Reduction AttackSpeed = " + _creeperStrike.Buff.AttackSpeed.Multiplier);
        }
    }

    public void UseTalentStrokesOfAspiration()
    {
        //Debug.Log($"StrokesOfAspiration / UseTalentStrokesOfAspiration / after updateRemainingCooldownTimeForSpitPoison = {_spitPoison.RemainingCooldownTime}");
        float updateRemainingCooldownTimeForSpitPoison = _spitPoison.RemainingCooldownTime - _decreaseCooldownTime;
        _spitPoison.ReductionSetCooldown(updateRemainingCooldownTimeForSpitPoison);
        //Debug.Log($"StrokesOfAspiration / UseTalentStrokesOfAspiration / before updateRemainingCooldownTimeForSpitPoison = {_spitPoison.RemainingCooldownTime}");

        for (int i = 0; i < _poisonBall.RemainingCooldownTimeCharge.Count; i++)
        {
            if (_poisonBall.RemainingCooldownTimeCharge[i] > 0)
            {
                //float updateRemainingCooldownTimeForPoisonBall = _poisonBall.RemainingCooldownTimeCharge[i] - _decreaseCooldownTime;
                float updateRemainingCooldownTimeForPoisonBall = 5f;
                //_poisonBall.ReductionCooldownTimeCharge(updateRemainingCooldownTimeForPoisonBall);

                Debug.Log($"StrokesOfAspiration / UseTalentStrokesOfAspiration / before updateRemainingCooldownTimeForSpitPoison = {_poisonBall.RemainingCooldownTimeCharge[i]}");
                break;
            }
        }
    }
}
