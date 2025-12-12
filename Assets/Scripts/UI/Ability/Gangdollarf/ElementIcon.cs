using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ElementIcon : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private int _index;
    [SerializeField] private ElementalSpawn _elementalSpawn;

    private IconFollowOnMouse _iconFollowOnMouse;
    private Quaternion _rotation;

    private void Awake()
    {
        _button.onClick.AddListener(OnClick);
        _elementalSpawn.CastStarted += OnCastStarted;
        _elementalSpawn.CastEnded += OnCanceled;
        _elementalSpawn.Canceled += OnCanceled;

        gameObject.SetActive(false);

        _rotation = transform.rotation;
    }

    private void OnEnable()
    {
        transform.Rotate(Vector3.up, 180);

        transform.DOLocalRotate(_rotation.eulerAngles, 1);
    }

    private void OnDestroy()
    {
        _elementalSpawn.CastStarted -= OnCastStarted;
        _elementalSpawn.CastEnded -= OnCanceled;
        _elementalSpawn.Canceled -= OnCanceled;
    }

    private void OnClick()
    {
        _elementalSpawn.IndexElemental = _index;
    }
    
    private void OnCastStarted()
    {
        gameObject.SetActive(true);
    }    

    private void OnCanceled()
    {
        gameObject.SetActive(false);
    }
}
