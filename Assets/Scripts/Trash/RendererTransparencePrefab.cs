using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RendererTransparencePrefab : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    public float FadeDuration = 1f;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        Color startColor = _spriteRenderer.color;
        float alpha = 0.8f;

        float elapsedTime = 0f;
        while (elapsedTime < FadeDuration)
        {
            alpha = Mathf.Lerp(1f, 0f, elapsedTime / FadeDuration);

            _spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f);

        Destroy(gameObject);
    }
}
