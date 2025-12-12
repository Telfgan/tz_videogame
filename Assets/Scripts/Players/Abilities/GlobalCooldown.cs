using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalCooldown : MonoBehaviour
{
    public Image[] CooldownToggles;
    public bool IsGlobalCooldown;

    public void StartGlobalCooldown()
    {
        if (gameObject.activeSelf)
        {
            StartCoroutine(ActiveImage());
        }
    }

    IEnumerator ActiveImage()
    {
        IsGlobalCooldown = true;

        if (CooldownToggles != null)
        {
            foreach (Image image in CooldownToggles)
            {
                if (image != null)
                {
                    image.gameObject.SetActive(true);

                    TextMeshPro textMeshPro = image.GetComponentInChildren<TextMeshPro>();
                    if (textMeshPro != null)
                    {
                        textMeshPro.gameObject.SetActive(false);
                    }
                    if (gameObject.activeSelf)
                    {
                        StartCoroutine(ChangeFillAmountOverTime(image));
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.5f);

        if (CooldownToggles != null)
        {
            foreach (Image image in CooldownToggles)
            {
                if (image != null)
                {
                    image.gameObject.SetActive(false);
                }
            }
        }
       
        IsGlobalCooldown = false;
    }

    IEnumerator ChangeFillAmountOverTime(Image image)
    {
        float elapsedTime = 0f;

        while (elapsedTime < 0.5f)
        {
            if (image != null)
            {
                image.fillAmount = Mathf.Lerp(1, 0, elapsedTime / 0.5f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            else
            {
    
                yield break; // ѕрерываем корутину, если изображение стало null
            }
        }

        if (image != null)
        {
            image.fillAmount = 0f;
        }
    }
}