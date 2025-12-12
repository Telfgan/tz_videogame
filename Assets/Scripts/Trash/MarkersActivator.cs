using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkersActivator : MonoBehaviour
{
    public List<GameObject> Markers = new List<GameObject>();

    void Start()
    {

        for (int i = 0; i < Markers.Count; ++i) 
        { 
            Markers[i].SetActive(true);
        }
    }
}
