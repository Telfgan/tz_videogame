using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    Buff,
    Debuff
}

public class BaseEffect : MonoBehaviour
{
    public EffectType Type;
}
