using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkActivePsionica : MonoBehaviour
{
    [SerializeField] private SpriteRenderer FirstIconActivePsionica;
    [SerializeField] private SpriteRenderer SecondIconActivePsionica;
    [SerializeField] private SpriteRenderer ThirdIconActivePsionica;

    private float _activePsionica;
    private Coroutine _firstCoroutine;
    private Coroutine _secondCoroutine;
    private Coroutine _thirdCoroutine;

    private void Start()
    {
        FirstIconActivePsionica.gameObject.SetActive(false);
        SecondIconActivePsionica.gameObject.SetActive(false);
        ThirdIconActivePsionica.gameObject.SetActive(false);
    }
    void Update()
    {
        _activePsionica = transform.parent.Find("Abilities").GetComponent<FiveConversion>().PsionicaActive;

        CheckActivePsionica();
    }

    private void CheckActivePsionica()
    {
        if (_activePsionica > 0)
        {
            if (_activePsionica >= 30)
            {
                ThirdIconActivePsionica.gameObject.SetActive(true);
                if (_thirdCoroutine == null)
                {
                    _thirdCoroutine = StartCoroutine(Blink(ThirdIconActivePsionica));
                }
            }
            
            if (_activePsionica >= 20)
            {
                SecondIconActivePsionica.gameObject.SetActive(true);
                if (_secondCoroutine == null)
                {
                    _secondCoroutine = StartCoroutine(Blink(SecondIconActivePsionica));
                }
            }

            if (_activePsionica >= 10)
            {
                FirstIconActivePsionica.gameObject.SetActive(true);
                if (_firstCoroutine == null)
                {
                    _firstCoroutine = StartCoroutine(Blink(FirstIconActivePsionica));
                }
            }
        }
        else if(FirstIconActivePsionica.gameObject.activeSelf || SecondIconActivePsionica.gameObject.activeSelf || ThirdIconActivePsionica.gameObject.activeSelf)
        {
            if (_thirdCoroutine != null)
            {
                StopCoroutine(_thirdCoroutine);
                _thirdCoroutine = null;
            }
            ThirdIconActivePsionica.gameObject.SetActive(false);

            if (_secondCoroutine != null)
            {
                StopCoroutine(_secondCoroutine);
                _secondCoroutine = null;
            }
            SecondIconActivePsionica.gameObject.SetActive(false);

            if (_firstCoroutine != null)
            {
                StopCoroutine(_firstCoroutine);
                _firstCoroutine = null;
            }
            FirstIconActivePsionica.gameObject.SetActive(false);
        }

    }
    private IEnumerator Blink(SpriteRenderer IconAbility)
    {
        while (true)
        {
            // Затухание
            for (float t = 0f; t < 1; t += Time.deltaTime)
            {
                float normalizedTime = t / 1;
                float alpha = Mathf.Lerp(1f, 0f, normalizedTime);
                Color newColor = IconAbility.color;
                newColor.a = alpha;
                IconAbility.color = newColor;
                yield return null;
            }

            // Появление
            for (float t = 0f; t < 1; t += Time.deltaTime)
            {
                float normalizedTime = t / 1;
                float alpha = Mathf.Lerp(0f, 1f, normalizedTime);
                Color newColor = IconAbility.color;
                newColor.a = alpha;
                IconAbility.color = newColor;
                yield return null;
            }
        }
    }
}
