using Mirror;
using System.Collections;
using UnityEngine;

public class PoisonHealingCloudPrefab : NetworkBehaviour
{
    [SerializeField] private ParticleSystem _poisonHealingCloudParticle;
    private ParticleSystem _instancePoisonHealingCloud;

    [SerializeField] private int _maxStacks = 5;
    private int _currentStacks;

    [SerializeField] private float _radiusCloud;
    private float _baseDuration;
    private float _duration;

    private PoisonHealingCloudPrefab _poisonHealCloud;
    private Character _player;

    private Coroutine _lifetimeStacksCoroutine;
    private Coroutine _activateParticlePoisonCloudCoroutine;
    public PoisonHealingCloudPrefab PoisonHealingCloud { get => _poisonHealCloud; set => _poisonHealCloud = value; }

    private void Update()
    {
        if (_instancePoisonHealingCloud != null)
        {
            _instancePoisonHealingCloud.transform.position = _player.transform.position;
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
        if (_currentStacks < _maxStacks)
        {
            _currentStacks++;
            if (_activateParticlePoisonCloudCoroutine == null && _poisonHealCloud == null)
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
        if (_instancePoisonHealingCloud == null)
        {
            _instancePoisonHealingCloud = Instantiate(_poisonHealingCloudParticle, _player.transform);
            _instancePoisonHealingCloud.Play();
        }

    }

    private void UpdateInstanceCloud()
    {        
        if (_instancePoisonHealingCloud != null)
        {
            _instancePoisonHealingCloud.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = _instancePoisonHealingCloud.main;
            main.duration = _baseDuration;
            _instancePoisonHealingCloud.Play();
        }
    }

    private IEnumerator ActivatePoisonCloud()
    {
        InstantiateCloud();
        yield return null;
    }

    private IEnumerator LifeTimeStacks()
    {
        yield return new WaitForSecondsRealtime(_duration);

        while (_currentStacks > 0)
        {
            _currentStacks = 0;
        }

        if (_instancePoisonHealingCloud != null)
        {
            _instancePoisonHealingCloud.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Destroy(_instancePoisonHealingCloud.gameObject);
            _instancePoisonHealingCloud = null;

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
        Destroy(gameObject);
        PoisonHealingCloud = null;
    }


}
