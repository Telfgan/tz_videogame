using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Gangdollarff.WaterElemental
{
    public class WaterAuras : MonoBehaviour
    {
        private void Start()
        {
            var chatacter = GetComponent<Character>();
            chatacter.CharacterState.CmdAddState(States.MagicWater, 0, 0, chatacter.gameObject, name);
            chatacter.CharacterState.CmdAddState(States.CoolingAura, 0, 0, chatacter.gameObject, name);
        }
    }

    public class MagicWater : AuraState
    {
        private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Strengthening };
        private float _procent = 0.03f;

        public override float Distance => 6;
        public override float EffectRate => 0.2f;
        public override LayerMask LayerMask => LayerMask.GetMask("Allies");
        public override States State => States.EarthsHealth;
        public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
        public override List<StatusEffect> Effects => _effects;

        public override void EffectOnEnter(Character character)
        {
            if (character.Resources.Count > 0)
                character.Health.IncreaseRegen(character.Resources[0].MaxValue * _procent);
        }

        public override void EffectOnExit(Character character)
        {
            if (character.Resources.Count > 0)
                character.Health.DecreaseRegen(character.Resources[0].MaxValue * _procent);
        }

        public override void EffectOnStay(List<Character> characters)
        {

        }
    }

    public class CoolingAura : AuraState
    {
        private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Strengthening };
        private float _procent = 0.03f;

        public override float Distance => 6;
        public override float EffectRate => 1f;
        public override LayerMask LayerMask => LayerMask.GetMask("Enemy");
        public override States State => States.Cooling;
        public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
        public override List<StatusEffect> Effects => _effects;

        public override void EffectOnEnter(Character character)
        {
        }

        public override void EffectOnExit(Character character)
        {
            
        }

        public override void EffectOnStay(List<Character> characters)
        {
            foreach (Character character in characters)
            {
                character.CharacterState.CmdAddState(State, 8, 0, character.gameObject, "character");
            }
        }
    }
}
