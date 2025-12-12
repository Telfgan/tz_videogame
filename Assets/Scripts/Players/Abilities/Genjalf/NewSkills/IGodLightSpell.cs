using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGodLightSpell
{
    public bool IsEnabled { get; }
    public void ChangeMode();
}
