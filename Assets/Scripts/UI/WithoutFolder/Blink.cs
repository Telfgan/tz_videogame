using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Blink : MonoBehaviour
{
    [SerializeField] private Image _image;

    private Coroutine _bilnkJob;

    public void StartBlink(float time)
    {
        StopBlink();
        _bilnkJob = StartCoroutine(BlinkCoroutine(time));
    }
    public void StopBlink()
    {
        if(_bilnkJob != null)
            StopCoroutine(_bilnkJob);
    }

    private IEnumerator BlinkCoroutine(float time)
    {
        float alpha;
        float curretTime;
        while (true)
        {
            curretTime = 0;

            while (curretTime < time)
            {
                alpha = Mathf.Lerp(0, 1, curretTime / time);
                _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, alpha);
                curretTime += Time.deltaTime;
                yield return null;
            }
            curretTime = 0;

            while (curretTime < time)
            {
                alpha = Mathf.Lerp(1, 0, curretTime / time);
                _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, alpha);
                curretTime += Time.deltaTime;
                yield return null;
            }
            yield return null;
        }
    }
}
