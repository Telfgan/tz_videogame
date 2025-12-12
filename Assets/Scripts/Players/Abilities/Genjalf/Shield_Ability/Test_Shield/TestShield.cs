using System.Collections;
using TMPro;
using UnityEngine;

namespace Players.Abilities.Genjalf.Shield_Ability.Test_Shield
{
    public class TestShield : MonoBehaviour
    {
        [SerializeField] private SoShieldData _soShieldData;
        [SerializeField] private TextMeshProUGUI _textShield;
        [SerializeField] private TextMeshProUGUI _textShieldCharges;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private GameObject _enableShield;

        public float health = 20f;

        private float _currentHealth;

        private Coroutine _coroutineActiveShield;
        private Coroutine _resetCoroutine;
        private int _currentShieldCharge;
        private float _currentAbAmount;
        private bool _isResetCoroutineRunning = false;

        private void Start()
        {
            _enableShield.SetActive(false);
            _currentHealth = health;
            _healthText.text = "_currentHealth: " + _currentHealth.ToString();
            _currentShieldCharge = _soShieldData.ShieldCharges;
            _textShieldCharges.text = "Charges Shield: " + _currentShieldCharge.ToString();
        }

        private void Update()
        {
            CheckChargeOnStartReset();

            _textShieldCharges.text = "Charges Shield: " + _currentShieldCharge.ToString();
            _textShield.text = "Shield: " + _currentAbAmount.ToString();

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (_resetCoroutine != null)
                {
                    StopCoroutine(_resetCoroutine);
                    _isResetCoroutineRunning = false;
                }

                _coroutineActiveShield = StartCoroutine(ActiveShield(_soShieldData.DurationShield));
            }
        }

        public void StartResetTime()
        {
            StartCoroutine(ResetTimeForCharging(_soShieldData.CooldownCharge));
            _isResetCoroutineRunning = true;
        }

        //Включаем щит.
        private IEnumerator ActiveShield(float durationCast)
        {
            if (_currentShieldCharge == 0)
                yield break;
            if (_resetCoroutine != null)
            {
                StopCoroutine(_resetCoroutine);
                _isResetCoroutineRunning = false;
            }

            _enableShield.SetActive(true);
            yield return new WaitForSeconds(durationCast);
            _currentShieldCharge--;
            gameObject.GetComponent<SpriteRenderer>().color = Color.cyan;
            _textShield.text = "Shield: " + _currentAbAmount.ToString();
            _currentAbAmount = _soShieldData.AbsorptionAmount;
            _enableShield.SetActive(false);
        }

        private void CheckChargeOnStartReset()
        {
            if (_currentShieldCharge < _soShieldData.ShieldCharges && !_isResetCoroutineRunning)
            {
                StartResetTime();
            }
        }

        private IEnumerator ResetTimeForCharging(float resetTime)
        {
            yield return new WaitForSeconds(resetTime);
            _currentShieldCharge++;
            _isResetCoroutineRunning = false;
        }

        public void DamageInShield(float incomingDamage)
        {
            float remainingDamage = _currentAbAmount - incomingDamage;

            if (remainingDamage <= 0)
            {
                DamageHealth(Mathf.Abs(remainingDamage));
                _currentAbAmount = 0;
            }
            else
            {
                _currentAbAmount = remainingDamage;
            }
        }

        private void DamageHealth(float remainingDamage)
        {
            _currentHealth -= remainingDamage;
            _healthText.text = "_currentHealth: " + _currentHealth.ToString();
            if (_currentHealth <= 0)
            {
                gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                //GameOver
            }
        }
    }
}