using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetForCamera : MonoBehaviour, IPointerClickHandler
{
    public RectTransform Rect;
    public Camera CamGlob;
    public Camera CamMap;
   
    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 vec;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect,
            eventData.pointerCurrentRaycast.screenPosition,
            eventData.enterEventCamera, out vec);

        float x = CamMap.pixelWidth * Rect.pivot.x + CamMap.pixelWidth * (vec.x / Rect.sizeDelta.x);
        float y = CamMap.pixelHeight * Rect.pivot.y + CamMap.pixelHeight * (vec.y / Rect.sizeDelta.y);

        var pos = new Vector3(x, y, CamMap.transform.position.z);

        var posCamera = CamMap.ScreenToWorldPoint(pos);

        CamGlob.transform.position = new Vector3(posCamera.x, posCamera.y,
            CamGlob.transform.position.z);
    }
}

