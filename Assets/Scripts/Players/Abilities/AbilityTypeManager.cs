using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AbilityTypeManager
{
    // 0 - тип 1, 1 - тип 2
    private static int activeAbilityType = 0;

    public static int ActiveAbilityType
    {
        get { return activeAbilityType; }
        set { activeAbilityType = value; }
    }
}
