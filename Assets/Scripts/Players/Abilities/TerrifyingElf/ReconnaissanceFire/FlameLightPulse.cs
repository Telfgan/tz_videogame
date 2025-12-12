using System.Collections;
using UnityEngine;

public class FlameLightPulse : MonoBehaviour
{
    [SerializeField] private Light flameLight;
    [SerializeField] private ReconnaissanceFireAura reconnaissanceFireAura;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 2f;

    private Coroutine _pulseCoroutine;

    public Light FlameLight { get => flameLight; set => flameLight = value; }

    private void OnEnable()
    {
        if (reconnaissanceFireAura != null)
        {
            reconnaissanceFireAura.OnStateDarkTalentChanged += UpdateLightColor;
        }

        StartCorrectCoroutine();
    }

    private void OnDisable()
    {
        if (reconnaissanceFireAura != null)
        {
            reconnaissanceFireAura.OnStateDarkTalentChanged -= UpdateLightColor;
        }

        StopPulseCoroutine();
    }

    private void StartCorrectCoroutine()
    {
        StopPulseCoroutine();

        if (flameLight != null)
        {
            _pulseCoroutine = StartCoroutine(PulseLight(flameLight));
        }
    }

    private void StopPulseCoroutine()
    {
        if (_pulseCoroutine != null)
        {
            StopCoroutine(_pulseCoroutine);
            _pulseCoroutine = null;
        }
    }

    private IEnumerator PulseLight(Light targetLight)
    {
        float time = 0f;

        while (true)
        {
            if (targetLight != null)
            {
                time += Time.deltaTime * pulseSpeed;
                targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PingPong(time, 1f));
            }

            yield return null;
        }
    }

    private void UpdateLightColor(bool isFireDarkTalent)
    {
        if (flameLight == null) return;

        flameLight.color = isFireDarkTalent
            ? new Color(139 / 255f, 0 / 255f, 255 / 255f)
            : new Color(255 / 255f, 170 / 255f, 0 / 255f);
    }
}
