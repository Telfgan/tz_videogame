using System.Collections;
using UnityEngine;

public class DesireToHide : Talent
{
    private float _cooldownTimeForApplyInvisible;
    private float _startCooldownTimeForApplyInvisible = 10.0f;

    private float _timeForApplicationInvisible;
    private float _startTimeForApplicationInvisible = 2.0f;

    private bool _isCanApplyInvisible = false;
    private bool _isCanStartApplicationCoroutine = false;
    private bool _isRecharged = false;

    private Coroutine _applicationInvisibleCoroutine;
    private Coroutine _rechargeApplicationInvisibleCoroutine;

    public bool IsCanApplyInvisible { get => _isCanApplyInvisible; }

    public override void Enter()
    {
        SetActive(true);
        _timeForApplicationInvisible = _startTimeForApplicationInvisible;
    }

    public override void Exit()
    {
        SetActive(false);
    }

    public void ApplyInvisible()
    {
        _isCanApplyInvisible = true;

        if (_applicationInvisibleCoroutine == null)
        {
            _applicationInvisibleCoroutine = StartCoroutine(ApplicationInvisible());
        }
    }

    private void StopCoroutine()
    {
        _isCanApplyInvisible = false;
        _timeForApplicationInvisible = _startTimeForApplicationInvisible;

        if (_rechargeApplicationInvisibleCoroutine != null)
        {
            StopCoroutine(_rechargeApplicationInvisibleCoroutine);
            _rechargeApplicationInvisibleCoroutine = null;
            _cooldownTimeForApplyInvisible = _startCooldownTimeForApplyInvisible;
        }

        if (_applicationInvisibleCoroutine != null)
        {
            StopCoroutine(_applicationInvisibleCoroutine);
            _applicationInvisibleCoroutine = null;
        }
    }

    private IEnumerator ApplicationInvisible()
    {
        if (_rechargeApplicationInvisibleCoroutine == null)
        {
            _isRecharged = false;
            yield return _rechargeApplicationInvisibleCoroutine = StartCoroutine(RechargeApplicationInvisible());
        }

        if (_isCanStartApplicationCoroutine)
        {
            while (_isCanApplyInvisible)
            {
                _timeForApplicationInvisible -= Time.deltaTime;
                Debug.Log("DesireToHide / IsCanApplyInvisible");
                if (_timeForApplicationInvisible <= 0)
                {
                    StopCoroutine();
                }

                yield return null;
            }
        }
    }

    private IEnumerator RechargeApplicationInvisible()
    {
        while (!_isRecharged)
        {
            _cooldownTimeForApplyInvisible -= Time.deltaTime;
            if (_cooldownTimeForApplyInvisible <= 0)
            {
                _isRecharged = true;
                _isCanStartApplicationCoroutine = true;
            }

            yield return null;
        }
    }
}

