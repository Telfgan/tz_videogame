using Mirror;
using System.Collections;
using UnityEngine;

public class PoisonCloudDisplay : NetworkBehaviour
{
    [SerializeField] private Character _dad;
    [SerializeField] private ParticleSystem _poisonDamagingCloudPrefab;
    [SerializeField] private ParticleSystem _poisonHealingCloudPrefab;

    public PoisonCloudDisplay PoisonHealingCloud { get; set; }
    public PoisonCloudDisplay PoisonDamagingCloud { get; set; }

    private ParticleSystem _instancePoisonDamagingCloud;
    private ParticleSystem _instancePoisonHealingCloud;

    public int _currentStacks;
    public int _maxStacks;

    private float _duration;
    private float _baseDuration;

    private float _radiusCloud;

    private bool _isHealingCloud;

    private Coroutine _activatePoisonCloudCoroutine;
    private Coroutine _lifeTimeStacksCoroutine;

    public void InitializationPrefab(Character player, float duration, float radiusCloud, int maxStacks, bool isHealingCloud)
    {
        _dad = player;
        _duration = duration;
        _baseDuration = duration;
        _radiusCloud = radiusCloud;
        _maxStacks = maxStacks;
        _isHealingCloud = isHealingCloud;
    }

    public void AddStack()
    {
        if (_currentStacks < _maxStacks)
        {
            _currentStacks++;
            if (_activatePoisonCloudCoroutine == null)
            {
                _activatePoisonCloudCoroutine = StartCoroutine(ActivatePoisonCloud());
            }
            else
            {
                UpdateInstancePoisonCloud();
            }
        }

        if (_lifeTimeStacksCoroutine != null)
        {
            StopCoroutine(_lifeTimeStacksCoroutine);
        }

        _duration = _baseDuration;
        _lifeTimeStacksCoroutine = StartCoroutine(LifeTimeStacks());
    }

    private void InstantiateCloud()
    {
        if (_isHealingCloud)
        {
            if (_instancePoisonHealingCloud == null)
            {
                _instancePoisonHealingCloud = Instantiate(_poisonHealingCloudPrefab, transform.position, Quaternion.identity);
                _instancePoisonHealingCloud.Play();
            }
        }
        else
        {
            if (_instancePoisonDamagingCloud == null)
            {
                _instancePoisonDamagingCloud = Instantiate(_poisonDamagingCloudPrefab, transform.position, Quaternion.identity);
                _instancePoisonDamagingCloud.Play();
                Debug.Log("PoisonDamagingCloud = " + _instancePoisonDamagingCloud);
            }
        }

    }

    private void UpdateInstancePoisonCloud()
    {
        Debug.Log("UpdateInstancePoisonCloud / particleSystem");
        if (_instancePoisonDamagingCloud != null)
        {
            _instancePoisonDamagingCloud.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = _instancePoisonDamagingCloud.main;
            main.duration = _baseDuration;
            _instancePoisonDamagingCloud.Play();
        }

        if (_instancePoisonHealingCloud != null)
        {
            _instancePoisonHealingCloud.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = _instancePoisonHealingCloud.main;
            main.duration = _baseDuration;
            _instancePoisonHealingCloud.Play();
        }
    }

    private void Update()
    {
        if (_instancePoisonDamagingCloud != null)
        {
            _instancePoisonDamagingCloud.transform.position = _dad.transform.position;
        }

        if (_instancePoisonHealingCloud != null)
        {
            _instancePoisonHealingCloud.transform.position = _dad.transform.position;
        }
    }

    private IEnumerator ActivatePoisonCloud()
    {
        InstantiateCloud();
        yield return null;
    }

    private IEnumerator LifeTimeStacks()
    {
        Debug.Log("LifeTimeStacks");

        yield return new WaitForSecondsRealtime(_duration);
        Debug.Log("PoisonCloudDisplay / LifeTimeStacks");
        while (_currentStacks > 0)
        {
            _currentStacks = 0;
        }

        if (_instancePoisonDamagingCloud != null && PoisonDamagingCloud != null)
        {
            Debug.Log("Damag cloud not null");
            _instancePoisonDamagingCloud.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            NetworkServer.Destroy(PoisonDamagingCloud.gameObject);
            _instancePoisonDamagingCloud = null;
            NetworkServer.Destroy(_instancePoisonDamagingCloud.gameObject);
            PoisonDamagingCloud = null;
        }

        if (_instancePoisonHealingCloud != null && PoisonHealingCloud)
        {
            Debug.Log("Heal cloud not null");
            _instancePoisonHealingCloud.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            NetworkServer.Destroy(_instancePoisonHealingCloud.gameObject);
            _instancePoisonHealingCloud = null;
            NetworkServer.Destroy(PoisonHealingCloud.gameObject);
            PoisonHealingCloud = null;
        }

        StopAllCoroutines();
    }

}