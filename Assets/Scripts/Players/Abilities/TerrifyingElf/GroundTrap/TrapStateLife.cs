using Mirror;
using UnityEngine;

public class TrapStateLife : NetworkBehaviour
{
    [SyncVar] private GameObject _ownerCharacter;

    private Bound _bound;  

    public void Init(GameObject ownerCharacter)
    {
        _ownerCharacter = ownerCharacter;
        ResolveBound();
    }

    public override void OnStartClient()
    {
        ResolveBound();
    }

    private void ResolveBound()
    {
        if (_bound != null || _ownerCharacter == null) return;

        var characterState = _ownerCharacter.GetComponent<CharacterState>();
        _bound = characterState?.GetState(States.Bound) as Bound;
    }

    private void OnDestroy()
    {
        if (_bound == null && _ownerCharacter != null)
        {
            var charState = _ownerCharacter.GetComponent<CharacterState>();
            _bound = charState?.GetState(States.Bound) as Bound;
        }

        _bound?.NotifyTrapDestroyed();
    }
}
