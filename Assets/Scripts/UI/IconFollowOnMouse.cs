using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconFollowOnMouse
{
    public float _minLocalYAngle = -20f;
    public float _maxLocalYAngle = 20f;
    private Transform _transform;
    private float _rotationSpeed = 5f;

    public IconFollowOnMouse(Transform transform)
    {
        _transform = transform;
    }

    public void Update()
    {
        // Получаем точку на плоскости (в мировых координатах)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, _transform.position);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 worldTarget = ray.GetPoint(rayDistance);

            // Переводим целевую точку в локальное пространство
            Vector3 localTarget = _transform.InverseTransformPoint(worldTarget);

            // Вычисляем направление в локальном пространстве (игнорируем вертикаль)
            localTarget.y = 0;

            if (localTarget != Vector3.zero)
            {
                // Сохраняем текущие локальные углы X и Z
                Vector3 currentEuler = _transform.localEulerAngles;

                // Вычисляем целевой угол поворота по локальной Y
                float targetYAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

                // Создаем новый поворот (сохраняем X и Z, меняем только Y)
                Quaternion targetRotation = Quaternion.Euler(
                    currentEuler.x,
                    targetYAngle,
                    currentEuler.z
                );

                // Плавный поворот
                _transform.localRotation = Quaternion.Slerp(
                    _transform.localRotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
            }

            // Получаем локальные углы Эйлера
            Vector3 localEuler = _transform.localEulerAngles;
            localEuler.y = NormalizeAngle(localEuler.y);

            // Ограничиваем локальный Y
            localEuler.y = Mathf.Clamp(localEuler.y, _minLocalYAngle, _maxLocalYAngle);

            // Применяем обратно
            _transform.localEulerAngles = localEuler;
        }
    }
    private float NormalizeAngle(float angle)
    {
        angle = angle % 360;
        if (angle > 180) angle -= 360;
        return angle;
    }
}
