using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReviveVisualUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Image _image;

    private Coroutine _timerJob;

    public void StartTimer(float time)
    {
        if (_timerJob == null)
        {
            _timerJob = StartCoroutine(ReviveTimerCoroutine(time));
        }
        else
        {
            StopCoroutine(_timerJob);
            _timerJob = StartCoroutine(ReviveTimerCoroutine(time));
        }
    }

    private IEnumerator ReviveTimerCoroutine(float time)
    {
        _image.gameObject.SetActive(true);

        float currentTime = time;

        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            int intValue = currentTime > 0 ? Mathf.CeilToInt(currentTime) : Mathf.FloorToInt(currentTime);
            _text.text = intValue.ToString();
            yield return null;
        }
        _image.gameObject.SetActive(false);
    }
}
