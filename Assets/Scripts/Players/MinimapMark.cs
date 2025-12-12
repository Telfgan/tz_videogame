using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapMark : MonoBehaviour
{
    [SerializeField] UserNetworkSettings _userNetworkSettings;
    [SerializeField] SpriteRenderer _markForMinimap;

    private void OnEnable()
    {
        _userNetworkSettings.LayerMaskChanged += OnLayerMaskChanged;
    }

    private void OnDisable()
    {
        _userNetworkSettings.LayerMaskChanged -= OnLayerMaskChanged;
    }

    private void OnLayerMaskChanged(int obj)
    {
        if (obj == LayerMask.NameToLayer("Enemy"))
        {
            _markForMinimap.color = Color.red;
        }
        else if (obj == LayerMask.NameToLayer("Allies"))
        {
            _markForMinimap.color = Color.green;
        }
    }
}
