using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectBar : MonoBehaviour
{
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private GameObject objectBar;
    [SerializeField] private TMP_Text _healthText;

    public void SetMaxHealth(float maxHealth)
    {
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        UpdateHealthText(maxHealth);
    }

    public void SetHealth(float currentHealth)
    {
        _healthSlider.value = currentHealth;
        UpdateHealthText(currentHealth);
    }

    public void ShowHealthBar()
    {
        objectBar.SetActive(true);
    }

    public void HideHealthBar()
    {
        objectBar.SetActive(false);
    }

    public void ShowPhantomDamage(float phantomValue)
    {
    }

    private void UpdateHealthText(float currentHealth)
    {
        if (_healthText != null)
        {
            _healthText.text = Mathf.CeilToInt(currentHealth).ToString();
        }
    }
}
