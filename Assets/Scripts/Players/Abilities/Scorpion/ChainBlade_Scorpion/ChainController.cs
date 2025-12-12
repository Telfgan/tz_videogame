using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainController : NetworkBehaviour
{
    private LineRenderer lineRenderer;
    
    public Transform _startTarget = null;
    public Transform target = null;

    [SyncVar]
    public float num = 0;

    [SyncVar(hook = nameof(OnTargetChanged))]
    public uint targetID;

    [SyncVar(hook = nameof(OnTargetChanged2))]
    public uint parentID;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    public void Clear(Transform pos)
    {
        lineRenderer.SetPosition(0, pos.position);
        lineRenderer.SetPosition(1, pos.position);
    }

    public void AssignTarget(Transform startTarget, Transform newTarget)
    {
        lineRenderer.positionCount = 2;
        _startTarget = startTarget;
        target = newTarget;
        lineRenderer.SetPosition(0, target.position);
        lineRenderer.SetPosition(1, _startTarget.position);
    }

    private void Update()
    {
        if (target != null && _startTarget != null)
        {
            lineRenderer.SetPosition(0, target.position);
            lineRenderer.SetPosition(1, _startTarget.position);
        }

        //CmdUpdatePos();
    }

    private void UpdatePos()
    {
        if (target != null && _startTarget != null)
        {
            lineRenderer.SetPosition(0, target.position);
            lineRenderer.SetPosition(1, _startTarget.position);
        }
    }

    private void OnTargetChanged(uint _, uint newValue)
    {
        target = null;

        if (NetworkClient.spawned.TryGetValue(targetID, out NetworkIdentity identity))
        {
            if (identity.TryGetComponent<BladeProjectile>(out BladeProjectile blade))
            {
                target = blade.ChainLinkPoint;
            }
            else
            {
                target = identity.transform;
            }
            Debug.Log(identity.gameObject.name);
        }
        else
            StartCoroutine(SetTarget());
    }

    private IEnumerator SetTarget()
    {
        while (target == null)
        {
            yield return null;
            if (NetworkClient.spawned.TryGetValue(targetID, out NetworkIdentity identity))
            {
                if (identity.TryGetComponent<BladeProjectile>(out BladeProjectile blade))
                {
                    target = blade.ChainLinkPoint;
                }
                else
                {
                    target = identity.transform;
                }
            }
            Debug.Log(identity.gameObject.name);
        }
    }

    private void OnTargetChanged2(uint _, uint newValue)
    {
        _startTarget = null;

        if (NetworkClient.spawned.TryGetValue(parentID, out NetworkIdentity character))
            _startTarget = character.transform;
        else
            StartCoroutine(SetTarget2());
    }

    private IEnumerator SetTarget2()
    {
        while (_startTarget == null)
        {
            yield return null;
            if (NetworkClient.spawned.TryGetValue(parentID, out NetworkIdentity character))
                _startTarget = character.transform;

        }
    }
}
