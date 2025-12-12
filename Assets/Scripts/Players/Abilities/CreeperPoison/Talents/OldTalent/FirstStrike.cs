using UnityEngine;

public class FirstStrike : Talent
{
    [SerializeField] private CreeperInvisible _creeperInvisible;
    [SerializeField] private CreeperStrike _creeperStrike;

    private bool _isCanIncreaseCrit = false;

    public bool FirstHit = false;
    public bool IsCanIncreaseCrit { get => _isCanIncreaseCrit; set => _isCanIncreaseCrit = value; }

    public override void Enter()
    {
        SetActive(true);
    }

    public override void Exit()
    {
        SetActive(false);
    }

    public void SetBoolTrue()
    {
        _isCanIncreaseCrit = true;
        FirstHit = true;

        Debug.Log($"FirstStrike / SetBoolTrue / IsCanCrit = {_isCanIncreaseCrit} | FirstHit = {FirstHit}");
    }

    public void ReturnBoolFalse()
    {
        if (_isCanIncreaseCrit)
        {
            _isCanIncreaseCrit = false;
            FirstHit = false;
            Debug.Log($"FirstStrike / ReturnBoolFalse / IsCanCrit = {_isCanIncreaseCrit} | FirstHit = {FirstHit}");
        }
    }
}
