using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StateIcoDatabase", menuName = "StatusEffects/State Ico Database", order = 0)]
public class StateIcoDatabase : ScriptableObject
{
    public List<StateIcoData> Entries = new();
}

[Serializable]
public class StateIcoData
{
    public States State;
    public Sprite Icon;
    public Color BorderColor = Color.white;
}
