using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAltAbility
{
    public bool IsAltAbility { get; set; }

    public event Action AbilityChange;
}
