using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(LineRenderer))]
public class DrawCircle : MonoBehaviour
{
    public bool isActive = false;
    public int segments = 64;
    public Color lineColor = Color.white;
   // private LineRenderer lineRenderer;
    [SerializeField] private DecalProjector _projector;

	private void Update()
    {
        //transform.localPosition = Vector3.zero;
       /* lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;*/

        //_projector.material.color = lineColor;
    }

    public void Draw(float radius)
    {
        if (isActive)
            return;

        isActive = true;
        _projector.size = new Vector3(radius*2, radius*2, 10);
		_projector.gameObject.SetActive(true);

		/*lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.useWorldSpace = false;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.positionCount = segments + 1;

        float angle = 0f;
        float angleIncrement = 360f / segments;

        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            lineRenderer.SetPosition(i, new Vector3(x, 0, y));

            angle += angleIncrement;
        }*/
    }

    public void Clear()
    {
        isActive = false;
        /*if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }*/
        _projector.gameObject.SetActive(false);
    }

    public void SetColor(Color newColor)
    {
		lineColor = newColor;

        _projector.material.color = newColor;
	}
}
