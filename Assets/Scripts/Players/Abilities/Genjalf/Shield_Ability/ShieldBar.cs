using UnityEngine;
using UnityEngine.UI;

namespace Players.Abilities.Genjalf.Shield_Ability
{
    public class ShieldBar : MonoBehaviour
    {
        private Slider _slider;

        public void SetMaxValueShield(float shield)
        {
            _slider = GetComponent<Slider>();
            
            if (_slider != null)
            {
                _slider.maxValue = shield;
                _slider.value = shield;
            }
            else
            {
                Debug.Log("Slider не найден!");
            }
        }

        public void SetShieldValue(float shield)
        {
            if (_slider != null)
            {
                _slider.value = shield;
            }
            else
            {
                Debug.Log("Slider не найден!");
            }
        }
    }
}