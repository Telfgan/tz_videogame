using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectObject : MonoBehaviour
{
    [HideInInspector] public Character SelectedObject;
    [HideInInspector] public bool CanSelect;
    public Character FirstPlayer;
    public Character SecondPlayer;
    public List<Character> ControlledObjects = new List<Character>();

    private static SelectObject instance;
    public static SelectObject Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if(SelectedObject != null)
        {
            SelectedObject.gameObject.layer = LayerMask.NameToLayer("Character");
        }
        else
        {
            SelectedObject = FirstPlayer;
            CanSelect = false;
        }
    }
    private void Update()
    {
        if (CanSelect)
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (SelectedObject != null)
                {
                    SelectedObject.gameObject.layer = LayerMask.NameToLayer("OtherPlayers");
                }

                SelectedObject = FirstPlayer;
                SelectedObject.gameObject.layer = LayerMask.NameToLayer("Character");
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                if (SelectedObject != null)
                {
                    SelectedObject.gameObject.layer = LayerMask.NameToLayer("OtherPlayers");
                }

                SelectedObject = SecondPlayer;
                SelectedObject.gameObject.layer = LayerMask.NameToLayer("Character");

            }

            if (ControlledObjects != null && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    if (SelectedObject != null)
                    {
                        SelectedObject.gameObject.layer = LayerMask.NameToLayer("OtherPlayers");
                    }

                    if (ControlledObjects.Count > 0)
                    {
                        SelectedObject = ControlledObjects[0];
                        SelectedObject.gameObject.layer = LayerMask.NameToLayer("Character");
                    }
                }
            }
            if (ControlledObjects != null && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    if (SelectedObject != null)
                    {
                        SelectedObject.gameObject.layer = LayerMask.NameToLayer("OtherPlayers");
                    }

                    if (ControlledObjects.Count > 1)
                    {
                        SelectedObject = ControlledObjects[1];
                        SelectedObject.gameObject.layer = LayerMask.NameToLayer("Character");
                    }
                }
            }
        }

        if(ControlledObjects.Count > 0)
        {
            if (ControlledObjects[0] == null &&  ControlledObjects.Count > 1)
            {
                ControlledObjects.Remove(ControlledObjects[0]);
            }
        }
    }
}
