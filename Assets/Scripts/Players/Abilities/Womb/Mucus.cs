using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mucus : NetworkBehaviour
{
    private ObjectHealth _objectHealth;
    private MucusAutoGrowth _mucusAutoGrowth;

    public MucusAutoGrowth MucusAutoGrowth
    {
        get => _mucusAutoGrowth;
        set
        {
            if (_mucusAutoGrowth != null) _mucusAutoGrowth.OnAnyMucusAutoGrowthDestroyed -= OnAutoGrowthDestroyed;
            _mucusAutoGrowth = value;

            if (_mucusAutoGrowth != null) _mucusAutoGrowth.OnAnyMucusAutoGrowthDestroyed += OnAutoGrowthDestroyed;
        }
    }

    private void Start()
    {
        _objectHealth = GetComponent<ObjectHealth>();
    }

    private void OnDestroy()
    {
        if (_mucusAutoGrowth != null) _mucusAutoGrowth.OnAnyMucusAutoGrowthDestroyed -= OnAutoGrowthDestroyed;
    }

    private void OnAutoGrowthDestroyed()
    {
        if (_objectHealth != null)
        {
            _objectHealth.IsDestroyOnDeath = true;
            _objectHealth.ÑmdStopCustomRegeneration();
            _objectHealth.ÑmdStartCustomNegativeRegeneration();
        }
    }
}
