using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float panSpeed = 5f;

    private Camera _camera;

    private void Start()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        Vector3 cameraPosition = transform.position;

        if (Input.GetMouseButton(2))
        {
            float deltaX = -Input.GetAxis("Mouse X") * panSpeed * Time.deltaTime * 8;
            float deltaY = -Input.GetAxis("Mouse Y") * panSpeed * Time.deltaTime * 8;

            cameraPosition.x += deltaX;
            cameraPosition.y += deltaY;

            _camera.transform.position = cameraPosition;
        }

        transform.position = cameraPosition;
    }
}