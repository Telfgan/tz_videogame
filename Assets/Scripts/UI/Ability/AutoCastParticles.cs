using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

public class AutoCastParticles : MonoBehaviour
{
    //[SerializeField] private ParticleSystem _particle1;
    //[SerializeField] private ParticleSystem _particle2;
    [SerializeField] private float _duration = 3;
    [SerializeField] private Transform[] _transformpath1;
    [SerializeField] private Transform[] _transformpath2;
    [SerializeField] private GameObject _particle1;
    [SerializeField] private GameObject _particle2;

    private Vector3[] _path1;
    private Vector3[] _path2;

    public void Play()
    {
        _particle1.gameObject.SetActive(true);
        _particle2.gameObject.SetActive(true);

        _path1 = new Vector3[(_transformpath1.Length)];
        _path2 = new Vector3[(_transformpath2.Length)];
        _particle1.transform.position = _transformpath1[0].position;
        _particle2.transform.position = _transformpath2[0].position;

        for (int i = 0; i < _path1.Length; i++)
            _path1[i] = transform.InverseTransformPoint(_transformpath1[i].position);

        for (int i = 0; i < _path2.Length; i++)
            _path2[i] = transform.InverseTransformPoint(_transformpath2[i].position);
        /*
        _particle1.gameObject.SetActive(false);
        _particle2.gameObject.SetActive(false);

        _particle1.gameObject.SetActive(true);
        _particle2.gameObject.SetActive(true);

        _particle2.GetComponent<SplineAnimate>().StartOffset = 0.51f;
        */
        _particle1.transform.localPosition = _path1[0];
        _particle1.transform.DOLocalPath(_path1, _duration).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
        _particle2.transform.localPosition = _path2[0];
        _particle2.transform.DOLocalPath(_path2, _duration).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
    }

    public void Stop()
    {
        _particle1.gameObject.SetActive(false);
        _particle2.gameObject.SetActive(false);

        _particle1.transform.DOKill();
        _particle2.transform.DOKill();

        _particle1.transform.position = _transformpath1[0].position;
        _particle2.transform.position = _transformpath2[0].position;
    }
}
