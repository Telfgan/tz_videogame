using System.Collections.Generic;
using UnityEngine;

public class SuppressionState : AbstractCharacterState
{
    private const int MaxStacks = 1;

    private const float CellLength = 0.10f;
    private const float ManaLossPerCell = 0.001f;
    private const float MoveEpsilon = 0.05f;

    private GameObject _suppressionIdle;
    private GameObject _suppressionMove;

    private MoveComponent _move;
    private Rigidbody _rigidbody;

    private float _baseDuration;
    private float _duration;
    private int _currentStacks = 1;

    private float _distBuffer;
    private bool _isMoving;

    private static readonly List<StatusEffect> _effects = new() { StatusEffect.Move };

    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;
    public override States State => States.Suppression;
    public override StateType Type => StateType.Magic;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit,
                                    Character caster, string skillName)
    {
        _characterState = character;
        _personWhoMadeBuff = caster;

        _baseDuration = durationToExit;
        _duration = _baseDuration;

        _move = character.Character.GetComponent<MoveComponent>();
        _rigidbody = _move != null ? _move.Rigidbody : character.Character.GetComponent<Rigidbody>();

        _distBuffer = 0f;
        _isMoving = false;

        _suppressionIdle = _characterState.StateEffects.SuppressionIdle;
        _suppressionMove = _characterState.StateEffects.SuppressionMove;

        if (_suppressionIdle) _suppressionIdle.SetActive(true);
        if (_suppressionMove) _suppressionMove.SetActive(false);
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;
        if (_duration <= 0f)
        {
            ExitState();
            return;
        }

        float deltaDist = CalcHorizontalDistanceThisFrame();
        HandleVisuals(deltaDist);
        DrainManaByDistance(deltaDist);
    }

    public override void ExitState()
    {
        if (_suppressionIdle) _suppressionIdle.SetActive(false);
        if (_suppressionMove) _suppressionMove.SetActive(false);

        _characterState.StateIcons.RemoveItemByState(State);
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        if (_currentStacks < MaxStacks) _currentStacks++;
        _duration = _baseDuration;
        return true;
    }

    #region Helpers
    private float CalcHorizontalDistanceThisFrame()
    {
        if (_rigidbody == null) return 0f;

        Vector3 distance = _rigidbody.linearVelocity;
        distance.y = 0f;
        return distance.magnitude * Time.deltaTime;
    }

    private void HandleVisuals(float deltaDist)
    {
        bool nowMoving = deltaDist / Time.deltaTime > MoveEpsilon;

        if (nowMoving == _isMoving) return;

        _isMoving = nowMoving;

        if (_isMoving)
        {
            if (_suppressionIdle) _suppressionIdle.SetActive(false);
            if (_suppressionMove) _suppressionMove.SetActive(true);
        }
        else
        {
            if (_suppressionMove) _suppressionMove.SetActive(false);
            if (_suppressionIdle) _suppressionIdle.SetActive(true);
        }
    }

    private void DrainManaByDistance(float deltaDist)
    {
        if (deltaDist <= 0f) return;

        _distBuffer += deltaDist;

        int cells = Mathf.FloorToInt(_distBuffer / CellLength);
        if (cells <= 0) return;

        _distBuffer -= cells * CellLength;

        if (_characterState.Character.TryGetResource(ResourceType.Mana) is Mana mana)
        {
            float loss = cells * mana.MaxValue * ManaLossPerCell;
            mana.TryUse(loss);
        }
    }
    #endregion
}
