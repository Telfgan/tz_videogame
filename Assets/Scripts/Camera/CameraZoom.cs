using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public float zoomSpeed = 1.0f;
    public float minSize = 3.0f;
    public float maxSize = 17.0f;

    private Camera _camera;

    private void Start()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        float scrollDelta = Input.mouseScrollDelta.y;
        
        if (scrollDelta != 0)
        {
            // ����������� ��������� ������� � ������� ����������� � ����������� ������������ �������� ������
            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldMousePosition = _camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, _camera.nearClipPlane));
            Vector3 viewportMousePosition = _camera.ScreenToViewportPoint(mousePosition);

            // ��������� ������� ��������������� ������ ������������ ��������� �������
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - scrollDelta * zoomSpeed, minSize, maxSize);

            // ����������� ������ ���, ����� ������ ��������� � ��� �� ����� ������������ �������� ������
            Vector3 newWorldMousePosition = _camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, _camera.nearClipPlane));
            Vector3 positionDelta = newWorldMousePosition - worldMousePosition;
            _camera.transform.position -= positionDelta;
        }
    }
}





