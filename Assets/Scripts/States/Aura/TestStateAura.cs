using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestStateAura : MonoBehaviour
{
    [SerializeField] private CharacterState _characterState;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.H))
        {
            _characterState.CmdAddState(States.TestAuraState, 0, 0, this.gameObject, name);
        }
        if (Input.GetKeyUp(KeyCode.J))
        {
            _characterState.CmdRemoveState(States.TestAuraState);
        }
    }
}

public class TestAuraState : AuraState
{
    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Move };

    public override States State => States.TestAuraState;
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override List<StatusEffect> Effects => _effects;
    public override float Distance => 10;
    //public override LayerMask LayerMask => LayerMask.GetMask("Allies");
    public override LayerMask LayerMask => LayerMask.GetMask("Enemy");
    public override float EffectRate => 1f; //раз в секунду

    public override void EffectOnEnter(Character character)
    {
        Debug.Log("Enter");
    }

    public override void EffectOnExit(Character character)
    {
        Debug.Log("Exit");
    }

    public override void EffectOnStay(List<Character> characters)
    {
        Debug.Log("Stay");
    }
}
