using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionLead : MinionComponent
{
    public override void SetAuthority(NetworkConnectionToClient con)
    {
        base.SetAuthority(con);

        foreach (var item in SpawnComponent.Units)
        {
            if (item is MinionComponent minion)
            {
                minion.SetAuthority(con);
            }
        }
    }
}
