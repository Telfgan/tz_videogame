using UnityEngine;

public class ContinuationAmbush : Talent
{
    private bool _isCanApplyInvisible;
    public bool IsCanApplyInvisible { get => _isCanApplyInvisible; set => _isCanApplyInvisible = value; }

    public override void Enter()
    {
        SetActive(true);
    }

    public override void Exit()
    {
        SetActive(false);
    }

    public void CanApplyInvisible(bool isCanApplyInvisible)
    {
        _isCanApplyInvisible = isCanApplyInvisible;
        Invoke("CanNotApplyInvisible", 4.0f);
    }

    private void CanNotApplyInvisible()
    {
        _isCanApplyInvisible = false;
    }
}
