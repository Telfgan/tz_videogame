using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboBar_Prefab : MonoBehaviour
{
    [SerializeField] private List<ComboPoint_Prefab> _comboPoints = new List<ComboPoint_Prefab>();
    [SerializeField] private ComboPoints_Player _comboPointsResourse;
    public List<ComboPoint_Prefab> ComboPoints { get { return _comboPoints; } }

    private void Awake()
    {
        _comboPointsResourse.ValueChanged += OnValueChanged;

        foreach (Transform child in transform)
        {
            child.TryGetComponent<ComboPoint_Prefab>(out ComboPoint_Prefab comboPoint);
            _comboPoints.Add(comboPoint);
        }
    }
    public void Clear()
    {
        foreach (var point in ComboPoints)
        {
            point.comboPointFillling.SetActive(false);
        }
    }
    public void TurnOn(int value)
    {
        for (int n = 0; n < value; n++)
        {
            for (int i = 0; i < ComboPoints.Count; i++)
            {
                if (!ComboPoints[i].comboPointFillling.activeSelf)
                {
                    ComboPoints[i].comboPointFillling.SetActive(true);
                    break;
                }
            }
        }      
    }

    public void TurnOff(int value) 
    {
        for (int n = 0; n < value; n++)
        {
            for (int i = ComboPoints.Count - 1; i >= 0; i--)//выключает последний включенный комбо-поинт
            {
                if (ComboPoints[i].comboPointFillling.activeSelf)
                {
                    ComboPoints[i].comboPointFillling.SetActive(false);
                    break;
                }
            }
        }        
    }

    private void OnValueChanged(float oldValue, float newValue)
    {
        UpdateBar((int)newValue);
    }

    public void UpdateBar(int newValue)
    {
        Clear();
        TurnOn(newValue);
    }
}
