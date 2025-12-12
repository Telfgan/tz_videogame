using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.TextCore.Text;

public abstract class Aura
{
    private Character _self;
    private Transform _auraCentre;
    private float _distance;
    private LayerMask _layerMask;
    private List<Character> _charactersInRadius;
    private List<Collider> _collidersInRadius;

    public List<Character> CharactersInRadius { get => _charactersInRadius; }

    public Aura(Character self, float distance, LayerMask layerMask)
    {
        _self = self;
        _auraCentre = self.transform;
        _distance = distance;
        _layerMask = layerMask;
        _charactersInRadius = new();
        _collidersInRadius = new();
    }

    public abstract void EffectOnEnter(Character character);
    public abstract void EffectOnExit(Character character);
    public abstract void EffectOnStay(List<Character> characters);

    public void Update()
    {
        var colliders = Physics.OverlapSphere(_auraCentre.position, _distance, _layerMask);

        foreach (Collider collider in colliders)
        {
            if (_collidersInRadius.Contains(collider) == false && collider.TryGetComponent(out Character character))
            {
                EffectOnExit(character);
            }
        }
        _collidersInRadius.Clear();
        _charactersInRadius.Clear();

        foreach (Collider collider in colliders)
        {
            _collidersInRadius.Add(collider);

            if (collider.TryGetComponent(out Character character) && _charactersInRadius.Contains(character) == false)
            {
                _charactersInRadius.Add(character);
                EffectOnEnter(character);
            }
        }
        EffectOnStay(_charactersInRadius);
    }
}
