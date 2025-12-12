using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTalents : MonoBehaviour
{
    public GameObject Object;
    bool active = true;

    private void Start()
    {
        Object.SetActive(false);
    }

    public void SetButton()
    {
        if (active == true)
        {
            Object.SetActive(true);
            active = false;
        }
        else
        {
            Object.SetActive(false);
            active = true;
        }
    }

}
