using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LVLUPParticle : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private Level _lvl;

    private void Start()
    {
        _lvl.LVLUped += OnLVLUped;
    }

    private void OnLVLUped(int obj)
    {
        _particleSystem.Play();
    }

    private void OnDestroy()
    {
        _lvl.LVLUped -= OnLVLUped;
    }
}
