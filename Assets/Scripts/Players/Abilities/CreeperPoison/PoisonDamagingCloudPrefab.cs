using Mirror;
using System.Collections;
using UnityEngine;

public class PoisonDamagingCloudPrefab : NetworkBehaviour
{
    [SerializeField] private ParticleSystem _poisonDamagingCloudParticle;
    private ParticleSystem _instancePoisonDamagingCloud;

    [SerializeField] private int _maxStacks = 5;
    private int _currentStacks;

    [SerializeField] private float _radiusCloud;
    private float _baseDuration;
    private float _duration;

    private PoisonDamagingCloudPrefab _poisonDamageCloud;
    private Character _player;

    private Coroutine _lifetimeStacksCoroutine;
    private Coroutine _activateParticlePoisonCloudCoroutine;

    public PoisonDamagingCloudPrefab PoisonDamageCloud { get => _poisonDamageCloud; set => _poisonDamageCloud = value; }

    private void Update()
    {
        if (_instancePoisonDamagingCloud != null)
        {
            _instancePoisonDamagingCloud.transform.position = _player.transform.position;
        }
    }

    public void InitializationProjectile(Character player, float duration)
    {
        _player = player;
        
        _duration = duration;
        _baseDuration = duration;
    }

    public void AddStack()
    {
        //Debug.Log("PoisonDamagingCloud / AddStack");
        //Debug.Log("PoisonDamagingCloud / AddStack / currentStacks = " + _currentStacks);
        if (_currentStacks < _maxStacks)
        {
            _currentStacks++;

            if (_activateParticlePoisonCloudCoroutine == null && _poisonDamageCloud == null)
            {
                _activateParticlePoisonCloudCoroutine = StartCoroutine(ActivatePoisonCloud());
            }
            else
            {
                UpdateInstanceCloud();
            }

            if (_lifetimeStacksCoroutine != null)
            {
                StopCoroutine(_lifetimeStacksCoroutine);
            }

            _duration = _baseDuration;
            _lifetimeStacksCoroutine = StartCoroutine(LifeTimeStacks());
        }
        else
        {
            UpdateInstanceCloud();

            if (_lifetimeStacksCoroutine != null)
            {
                StopCoroutine(_lifetimeStacksCoroutine);
            }

            _duration = _baseDuration;
            _lifetimeStacksCoroutine = StartCoroutine(LifeTimeStacks());
        }
    }

    private void InstantiateCloud()
    {
        if (_instancePoisonDamagingCloud == null)
        {
            _instancePoisonDamagingCloud = Instantiate(_poisonDamagingCloudParticle, _player.transform);
            _instancePoisonDamagingCloud.Play();
        }
    }

    private void UpdateInstanceCloud()
    {
        if (_instancePoisonDamagingCloud != null)
        {
            _instancePoisonDamagingCloud.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = _instancePoisonDamagingCloud.main;
            main.duration = _baseDuration;
            _instancePoisonDamagingCloud.Play();
        }
    }

    private IEnumerator ActivatePoisonCloud()
    {
        InstantiateCloud();
        yield return null;
    }

    private IEnumerator LifeTimeStacks()
    {
        float time = _duration;

        while (time > 0)
        {
            time -= Time.deltaTime;
            yield return null;
        }

        if (_activateParticlePoisonCloudCoroutine != null)
        {
            StopCoroutine(_activateParticlePoisonCloudCoroutine);
            _activateParticlePoisonCloudCoroutine = null;
        }

        if (_lifetimeStacksCoroutine != null)
        {
            StopCoroutine(_lifetimeStacksCoroutine);
            _lifetimeStacksCoroutine = null;
        }

        _currentStacks = 0;

        _instancePoisonDamagingCloud.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        Destroy(_instancePoisonDamagingCloud.gameObject);

        Destroy(gameObject);
        PoisonDamageCloud = null;
    }

    
}
