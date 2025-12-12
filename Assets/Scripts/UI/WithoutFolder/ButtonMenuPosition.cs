using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonMenuPosition : MonoBehaviour
{
    public RectTransform buttonRectTransform;
    public RectTransform menuContentRectTransform;
    public RectTransform Target;

    public RectTransform originalRectTransform;

    private void Update()
    {
        if (menuContentRectTransform.gameObject.activeSelf)
        {
            buttonRectTransform.position = Target.position;
        }
        else
        {
            buttonRectTransform.position = originalRectTransform.position;
        }
    }
}
