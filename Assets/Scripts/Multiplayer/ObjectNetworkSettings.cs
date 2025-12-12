using Mirror;
using UnityEngine;

public class ObjectNetworkSettings : NetworkBehaviour
{
    [SyncVar] public byte TeamIndex;
}
