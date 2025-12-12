using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class ImmaterialityState : AbstractCharacterState
{
    private int _defualtPlayerLayer;
    private int _newPlayerLayer;

    private float _duration;
    private Character _player;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Immateriality };
    public override States State => States.Immateriality;
    public override StateType Type => StateType.Immaterial;
    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _player = _characterState.Character;
        _duration = durationToExit;
        _defualtPlayerLayer = _characterState.gameObject.layer;

        _newPlayerLayer = LayerMask.NameToLayer("LightningMovement");

        DisabledCollider();
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;
        if (_duration <= 0)
        {
            ExitState();
        }
    }

    public override void ExitState()
    {
        TargetRpcResetPlayerComponents();
        _duration = 0;
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        return false;
    }

    private void DisabledCollider()
    {
        if (_player != null)
        {
            TargetRpcDisbledCollider();
        }
    }


    [ClientRpc]
    private void TargetRpcDisbledCollider()
    {
        if (_player != null)
        {
            _player.gameObject.layer = _newPlayerLayer;
        }
    }

    [ClientRpc]
    private void TargetRpcResetPlayerComponents()
    {
        _player.gameObject.layer = _defualtPlayerLayer;
    }
}