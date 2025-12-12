using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemActivator : MonoBehaviour
{

    public GameObject Item;
    public float LongX = 6;
    public float LongY = 6;

    private void Start()
    {
        Item.SetActive(false);
    }

    public void OnMouseOver()
    {
        Item.SetActive(true);
        if (Item)
        {
            Item.GetComponent<RectTransform>().position = new Vector3(transform.position.x + LongX * Camera.main.orthographicSize, transform.position.y - LongY * Camera.main.orthographicSize, transform.position.z);
        }
    }

    public void OnMouseExit()
    {
        Item.SetActive(false);
    }
    private void OnEnable()
    {
        Item.SetActive(false);

    }
}


