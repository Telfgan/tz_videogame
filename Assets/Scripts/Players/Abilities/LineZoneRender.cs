using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class LineZoneRender : MonoBehaviour 
{
    [SerializeField] private LineRenderer _lineRenderer;

    private Skill _skill;
    private Coroutine _lineDrawCoroutine;

    public void StartDraw(Skill skill)
    {
        _skill = skill;
        _skill.ClickPoint += SetPoint;

        _lineDrawCoroutine = _skill.StartCoroutine(DrawJob());
    }

    public void StartDraw(Vector3[] vector3s)
    {
        _lineRenderer.positionCount = vector3s.Length;
        _lineRenderer.SetPositions(vector3s);
    }

    public void StopDraw()
    {
        if (_skill != null)
        {
            _skill.StopCoroutine(_lineDrawCoroutine);

            _lineRenderer.positionCount = 0;
            _lineRenderer.SetPositions(new Vector3[0]);
            _skill.ClickPoint -= SetPoint;
            _skill = null;
        }
        else
        {
            _lineRenderer.positionCount = 0;
            _lineRenderer.SetPositions(new Vector3[0]);
        }
            
    }

    private void SetPoint(Vector3 point)
    {

        _lineRenderer.positionCount = _lineRenderer.positionCount + 1;
        _lineRenderer.SetPosition(_lineRenderer.positionCount - 2, point);
    }

    private IEnumerator DrawJob()
    {
        Vector3 mouse;
        Vector3 lastPoint = transform.position;

        _lineRenderer.positionCount = _lineRenderer.positionCount + 1;
        yield return null;

        while (true)
        {
            yield return null;

            mouse = _skill.GetMousePoint() + Vector3.up / 10;

            if (_lineRenderer.positionCount > 1)
                lastPoint = _lineRenderer.GetPosition(_lineRenderer.positionCount - 2);
            else
                lastPoint = mouse;

            if (_lineRenderer.positionCount > 1 && Vector3.Distance(lastPoint, mouse) > _skill.CastLength)
            {
                _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, lastPoint + (mouse - lastPoint).normalized * _skill.CastLength);
                continue;
            }

            if (Input.GetMouseButtonDown(0))
                SetPoint(mouse);

            _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, mouse);
        }
    }
}
