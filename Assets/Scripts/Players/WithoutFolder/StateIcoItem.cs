using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class StateIcoItem : MonoBehaviour
{
    public Image Icon;
    public Image FadeFront;
    public Image border;
    public TextMeshProUGUI Text;
    public int count = 1;
    public int maxStack;
    public float currentDuration;

    private States _states;

    public States State { get => _states; set => _states = value; }
}
