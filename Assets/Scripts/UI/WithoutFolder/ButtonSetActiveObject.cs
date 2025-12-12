using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSetActiveObject : MonoBehaviour
{
    public GameObject targetObject;

	private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GetComponent<Button>().onClick.Invoke();
        }
    }

    public void SetActiveTargetObject()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(!targetObject.activeSelf);
        }
    }
}
