using UnityEngine;
using UnityEngine.UI;

public class Selectable : MonoBehaviour
{
    public GameObject SelectObj;

    void Start()
    {
        SelectObj.SetActive(false);
    }

    public void OnMouseOver()
    {
        SelectObj.SetActive(true);
    }

    public void OnMouseExit()
    {
        if (GetComponent<Toggle>() && GetComponent<Toggle>().isOn == false)
        {
            SelectObj.SetActive(false);
        }
    }
}
