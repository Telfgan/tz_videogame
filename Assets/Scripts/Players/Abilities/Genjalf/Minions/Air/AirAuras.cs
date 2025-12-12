using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gangdollarff.AirElemental
{
    public class AirAuras : MonoBehaviour
    {

    }

    public class Discharge : AbstractCharacterState
    {
        private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Poison };
        private Character _character;
        private float _timeAfterLastEffect = 0;
        private float _effectRate = 1;
        private float _time;

        private int _chance = 50;

        public override States State => States.Burning;

        public override StateType Type => StateType.Magic;

        public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;

        public override List<StatusEffect> Effects => _effects;

        public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
        {
            _time = durationToExit;
            _character = character.Character;
        }

        public override void ExitState()
        {
            _character.CharacterState.RemoveState(this);
        }

        public override bool Stack(float time)
        {
            return false;
        }

        public override void UpdateState()
        {
            _time -= Time.deltaTime;
            if (_time <= 0)
            {
                ExitState();
            }

            _timeAfterLastEffect += Time.deltaTime;

            if (_effectRate > _timeAfterLastEffect && Random.Range(1, 100) >= _chance)
                return;

            //
            _character.CharacterState.RemoveState(_character.CharacterState.CurrentStates.FirstOrDefault(item => item.BaffDebaff == BaffDebaff.Baff));

            _timeAfterLastEffect = 0;
        }

    }

    public class RisingWind : AuraState
    {
        private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Strengthening };
        private float _procent = 1.10f;

        public override float Distance => 6;
        public override float EffectRate => 0.2f;
        public override LayerMask LayerMask => LayerMask.GetMask("Allies");
        public override States State => States.EarthsHealth;
        public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
        public override List<StatusEffect> Effects => _effects;

        public override void EffectOnEnter(Character character)
        {
            character.Move.SetMoveSpeed(character.Move.CurrentSpeed * _procent);
        }

        public override void EffectOnExit(Character character)
        {
            character.Move.SetMoveSpeed(character.Move.CurrentSpeed / _procent);
        }

        public override void EffectOnStay(List<Character> characters)
        {

        }
    }
}

