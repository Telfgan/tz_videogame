using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileInfo : ScriptableObject
{
    [SerializeField] private float _speed;
    [SerializeField] private ParticleSystem _particle;

    public ParticleSystem Particle => _particle;

    private void OnValidate()
    {
        
    }
}
