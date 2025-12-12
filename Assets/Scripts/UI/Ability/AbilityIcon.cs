using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityIcon : MonoBehaviour , IPointerEnterHandler , IPointerExitHandler
{
    [SerializeField] private Image _abilityIcon;
    [SerializeField] private Image _boxFrame;
    [SerializeField] private Blink _autoAttackBoxFrame;
    [SerializeField] private FillAmountOverTime _cooldown;
    [SerializeField] private TextMeshProUGUI _chargeCounter;
    [SerializeField] private GameObject _abilityNameBox;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _description;
    [SerializeField] private AutoCastParticles _autoCastEffectPrefab;


    private FillAmountOverTime _castLine;
    private Skill _ability;
    private AutoCastParticles _autoCastEffect;


    public void OnPointerEnter(PointerEventData eventData)
    {
        _abilityNameBox.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _abilityNameBox.SetActive(false);
    }

    public void Init(Skill ability, FillAmountOverTime castLine)
    {
        _ability = ability;
        _abilityIcon.sprite = ability.Icon;
        _name.text = ability.Name;
        _description.text = ability.Description;
        _castLine = castLine;

        if (ability.IsUseCharges)
        {
            ability.CurrentChargeChanged += OnCurrentChargeText;
            _chargeCounter.enabled = true;
            OnCurrentChargeText(ability.Chargers);
        }

        if (ability is AutoAttackSkill)
        {
            _autoCastEffect = Instantiate(_autoCastEffectPrefab, transform);
        }

        SubscribingSkillOnEvents(ability);
    }

    public void Init(Skill ability)
    {
        _ability = ability;
        _abilityIcon.sprite = ability.Icon;
        _name.text = ability.Name;
        _description.text = ability.Description;
    }

    private void OnDestroy()
    {
        UnsubscribingSkillOnEvents(_ability);
    }

    public void OnCurrentChargeText(int value)
    {
        if (value > 0)
            _chargeCounter.color = Color.green;
        else
            _chargeCounter.color = Color.red;

        _chargeCounter.text = value.ToString();
    }

    public void OnStartCooldown(float dutarion)
    {
        _cooldown.StartFill(dutarion, 1, 0, false);
        _cooldown.gameObject.SetActive(true);
    }

    public void Selected()
    {
        _boxFrame.color = Color.green;
    }

    public void Deselected()
    {
        _boxFrame.color = Color.white;
    }

    public void AutoAttackSelected()
    {
        _autoAttackBoxFrame.gameObject.SetActive(true);
        _autoAttackBoxFrame.StartBlink(.5f);
    }

    public void AutoAttackDeselected()
    {
        _autoAttackBoxFrame.gameObject.SetActive(false);
        _autoAttackBoxFrame.StopBlink();
    }

    public void DestroyIcon()
    {
        Destroy(gameObject);
    }

    private void OnStartStreaming(float time)
    {
        _castLine.gameObject.SetActive(true);
        _castLine.StartFill(time, 1, 0);
    }

    private void OnStopStreaming()
    {
        _castLine.gameObject.SetActive(false);
        _castLine.Stop();
    }

    private void OnStartCastDeley(float time)
    {
        _castLine.gameObject.SetActive(true);
        _castLine.StartFill(time);
    }

    private void OnStopCastDeley()
    {
        _castLine.gameObject.SetActive(false);
        _castLine.Stop();
    }

    private void OnStartAuto()
    {
        _autoCastEffect.gameObject.SetActive(true);
        _autoCastEffect.Play();
        Debug.LogWarning("OnStartAuto!");
    }

    private void OnEndAuto()
    {
        _autoCastEffect.gameObject.SetActive(false);
        Debug.LogWarning("OnEndAuto!!!!!!!!!!!!!!!");
    }

    private void SubscribingSkillOnEvents(Skill ability)
    {
        ability.CastStreamStarted += OnStartStreaming;
        ability.Canceled += OnStopStreaming;

        ability.CastDeleyStarted += OnStartCastDeley;
        ability.Canceled += OnStopCastDeley;

        ability.CooldownStarted += OnStartCooldown;

        if (ability is AutoAttackSkill autoAttackSkill)
        {
            autoAttackSkill.Canceled += OnEndAuto;
            //autoAttackSkill.CastPaused += OnEndAuto;
            autoAttackSkill.CastStarted += OnStartAuto;
            //autoAttackSkill.CastContinued += OnStartAuto;
            //autoAttackSkill.AutoCastEnded +=
        }
    }

    private void UnsubscribingSkillOnEvents(Skill ability)
    {
        ability.CastStreamStarted -= OnStartStreaming;
        ability.Canceled -= OnStopStreaming;

        ability.CastDeleyStarted -= OnStartCastDeley;
        ability.Canceled -= OnStopCastDeley;

        ability.CooldownStarted -= OnStartCooldown;

        if (ability is AutoAttackSkill autoAttackSkill)
        {
            autoAttackSkill.Canceled -= OnEndAuto;
           // autoAttackSkill.CastPaused -= OnEndAuto;
            autoAttackSkill.CastStarted -= OnStartAuto;
           // autoAttackSkill.CastContinued -= OnStartAuto;
            //autoAttackSkill.AutoCastEnded -=
        }

    }
}
