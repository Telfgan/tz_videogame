using UnityEngine;

public class CameraMoveNew : MonoBehaviour
{
    public TestController Target;

    public float moveSpeed = 5f;
    public float fastMoveSpeed = 10;
    public float rotationSpeed = 0.1f;

    private Vector3 lastMousePosition;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public Transform player;
    public Vector3 offset;
    public float smoothSpeed = 0.125f; // Скорость плавного перемещения
    public float minDistance = 2f; // Минимальное расстояние от игрока


    private Vector3 _velocity; // Переменная для плавного движения

    void FixedUpdate()
    {

    }

    private void LateUpdate()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
            return;
        HandleMovement();
        HandleRotation();
        HandleResetPosition();

        if (player == null) return;

        // Расчет расстояния от камеры до игрока
        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        // Если расстояние меньше минимального, то корректируем положение
        if (distance < minDistance)
        {
            direction = direction.normalized * minDistance;
        }


        // Вычисление новой позиции камеры
        Vector3 targetPosition = player.position + offset;

        // Плавное перемещение камеры
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, smoothSpeed);
    }

    private void HandleMovement()
    {
        float moveSpeedCurrent = Input.GetKey(KeyCode.LeftShift) ? fastMoveSpeed : moveSpeed;

        float moveX = 0;
        float moveZ = 0;

        float moveY = 0f;

        if (Input.GetKey(KeyCode.Q))
        {
            moveY = -1f;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            moveY = 1f;
        }
        
        if (Input.GetKey(KeyCode.UpArrow))
        {
            moveZ = 1f;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            moveZ = -1f;
        }
        
        if (Input.GetKey(KeyCode.RightArrow))
        {
            moveX = 1f;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            moveX = -1f;
        }


        Vector3 movement = new Vector3(moveX, moveY, moveZ) * moveSpeedCurrent * Time.deltaTime;
        transform.Translate(movement, Space.Self);
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;

            transform.Rotate(Vector3.up, mouseDelta.x * rotationSpeed, Space.World);

            transform.Rotate(Vector3.right, -mouseDelta.y * rotationSpeed, Space.Self);
        }

        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }

        lastMousePosition = Input.mousePosition;
    }

    private void HandleResetPosition()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
    }
}
