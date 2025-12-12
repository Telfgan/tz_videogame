using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class FillAmountOverTime : MonoBehaviour
{
    [SerializeField] private Image _image;

    private int _defaultFillOrigin;
    private float _currentTime;
    private float _duration;
    private Coroutine _fillJob;

    public event Action<FillAmountOverTime> Ended;

    private void Awake()
    {
        _defaultFillOrigin = _image.fillOrigin;
    }

    public void Stop()
    {
        if(_fillJob != null)
        {
            StopCoroutine(_fillJob);
            _fillJob = null;
            gameObject.SetActive(false);
        }
    }

    public void StartFill(float duration, float startValue = 0, float endValue = 1, bool addTime = true, float curretTime = 0, int type = -1)
    {
        gameObject.SetActive(true);

        if (type >= 0)
        {
            _image.fillOrigin = type;
        }
        else
        {
            _image.fillOrigin = _defaultFillOrigin;
        }

        if (_fillJob != null)
        {
            if (addTime)
            {
                _duration += -_currentTime + duration;
                StopCoroutine(_fillJob);
                _fillJob = StartCoroutine(ChangeFillAmountOverTimeCoroutine(_duration, curretTime, startValue, endValue));
            }
            else
            {
                _duration = duration;
                StopCoroutine(_fillJob);
                _fillJob = StartCoroutine(ChangeFillAmountOverTimeCoroutine(_duration, curretTime, startValue, endValue));
            }
        }
        else
        {
            _duration = duration;
            gameObject.SetActive(true);
            _fillJob = StartCoroutine(ChangeFillAmountOverTimeCoroutine(_duration, curretTime, startValue, endValue));
        }
    }

    IEnumerator ChangeFillAmountOverTimeCoroutine(float duration, float curretTime = 0, float startValue = 0, float endValue = 1)
    {
        while (curretTime < duration)
        {
            _image.fillAmount = Mathf.Lerp(startValue, endValue, curretTime / duration);
            curretTime += Time.deltaTime;
            _currentTime = curretTime;
            yield return null;
        }
        _fillJob = null;
        _image.fillAmount = endValue;
        Ended?.Invoke(this);
        gameObject.SetActive(false);
    }
}
