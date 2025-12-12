using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObjectsOnHideShow : MonoBehaviour
{
    public Transform objectToMove;  // Объект, который нужно перемещать
    public Transform showPosition;  // Позиция, куда перемещать объект при отображении картинки
    public Transform originalPosition; // Исходная позиция объекта

    private bool isHidden = false;     // Флаг, указывающий, скрыта ли картинка

    private void Start()
    {
        // Сразу притягиваем объкеты
        objectToMove.position = showPosition.position;
    }

    public void ToggleImageVisibility()
    {
        isHidden = !isHidden; // Инвертируем флаг видимости картинки

        if (isHidden)
        {
            // Если картинка отображена, возвращаем объект на исходную позицию
            objectToMove.position = originalPosition.position;
        }
        else
        {
            // Если картинка cкрыта, перемещаем объект на позицию 
            objectToMove.position = showPosition.position;
        }
    }
}
