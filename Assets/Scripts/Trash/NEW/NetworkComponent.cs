using System.Collections.Generic;
using Mirror;

public class NetworkComponent : NetworkBehaviour
{
    public List<Character> controllableUnits = new List<Character>();

    public override void OnStartServer()
    {
        controllableUnits = new List<Character>();
        Character.ServerOnUnitSpawned += ServerHandleUnitSpawn;
        Character.ServerOnUnitDeleted += ServerHandleUnitDelete;
    }

    public override void OnStopServer()
    {
        Character.ServerOnUnitSpawned -= ServerHandleUnitSpawn;
        Character.ServerOnUnitDeleted -= ServerHandleUnitDelete;
    }

    private void ServerHandleUnitSpawn(Character character)
    {
        if (character == null || connectionToClient == null || controllableUnits == null)
            return;

        if (character.connectionToClient == null)
            return;

        if (character.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        controllableUnits.Add(character);
    }

    private void ServerHandleUnitDelete(Character character)
    {
        if (character == null || controllableUnits == null || character.isClient == false)
            return;

        if (character.connectionToClient != null &&
            character.connectionToClient.connectionId != connectionToClient.connectionId)
        {
            return;
        }

        if (controllableUnits.Contains(character))
        {
            controllableUnits.Remove(character);
        }
    }

    public override void OnStartClient()
    {
        if (!isClientOnly) return;

        Character.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawn;
        Character.AuthorityOnUnitDeleted += AuthorityHandleUnitDelete;
    }

    public override void OnStopClient()
    {
        if (!isClientOnly) return;

        Character.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawn;
        Character.AuthorityOnUnitDeleted -= AuthorityHandleUnitDelete;
    }

    private void AuthorityHandleUnitSpawn(Character character)
    {
        if (!isOwned) return;
        controllableUnits.Add(character);
    }

    private void AuthorityHandleUnitDelete(Character character)
    {
        if (!isOwned) return;
        controllableUnits.Remove(character);
    }
}
