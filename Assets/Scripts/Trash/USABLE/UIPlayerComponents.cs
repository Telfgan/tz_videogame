using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPlayerComponents : MonoBehaviour
{
    [SerializeField] private Character _character;
    [SerializeField] private SelectedCircle CircleSelect;
    [SerializeField] private MinimapMarker MarkersSelect;
    [SerializeField] private FillAmountOverTime _castLine;
    [SerializeField] private SkillRenderer skillRenderer;

    [SerializeField] private DamageTracker _damageTracker;

    public Transform DamageSpawn;
    public Transform RegenSpawn;
    public PopupTextPrefab PopupText;

    private Color _shieldColor = Color.blue;
    private Color _physDamageColor = Color.red;
    private Color _regenColor = Color.green;

    private float popupSpawnDelay = 0.2f;
    private float _fixDuration = 0.1f;

    private Queue<PopupRequest> popupQueue = new Queue<PopupRequest>();
    private bool isProcessingQueue = false;

    public SelectedCircle CircleSelect1 { get => CircleSelect; set => CircleSelect = value; }
    public SkillRenderer Renderer { get => skillRenderer; set => skillRenderer = value; }

    private void Awake()
    {
        _damageTracker = _character.DamageTracker;
    }

    private void OnEnable()
    {
        _character.Health.DamageTaken += OnDamageTaken;
        _character.Health.ShieldDamageTaken += OnShieldDamageTaken;
        _character.Health.OnShieldAdd += OnShieldAdded;
        _character.Health.HealTaked += OnHealTaked;

        foreach (var ability in _character.Abilities.Abilities)
        {
            ability.CastStreamStarted += OnStartStreaming;
            ability.Canceled += OnStopStreaming;

            ability.CastDeleyStarted += OnStartCastDeley;
            ability.Canceled += OnStopCastDeley;
        }
    }

    private void OnDisable()
    {
        _character.Health.DamageTaken -= OnDamageTaken;
        _character.Health.ShieldDamageTaken -= OnShieldDamageTaken;
        _character.Health.OnShieldAdd -= OnShieldAdded;
        _character.Health.HealTaked -= OnHealTaked;

        foreach (var ability in _character.Abilities.Abilities)
        {
            ability.CastStreamStarted -= OnStartStreaming;
            ability.Canceled -= OnStopStreaming;

            ability.CastDeleyStarted -= OnStartCastDeley;
            ability.Canceled -= OnStopCastDeley;
        }
    }

    public void ChangeSelection(bool isSelect)
    {
        CircleSelect.IsActive = isSelect;
        MarkersSelect.IsActive = isSelect;
    }

    public void ShowPopupValue(float value, Color startColor, Color endColor)
    {
        int intValue = value > 0 ? Mathf.CeilToInt(value) : Mathf.FloorToInt(value);
        if (intValue == 0 && value != 0)
            intValue = value > 0 ? 1 : -1;

        string text = (intValue > 0 ? "+" : "") + intValue;
        popupQueue.Enqueue(new PopupRequest(text, startColor, endColor, DamageSpawn));
        TryStartQueueProcessing();
    }

    public void ShowPopupText(string text, Color startColor, Color endColor)
    {
        popupQueue.Enqueue(new PopupRequest(text, startColor, endColor, DamageSpawn));
        TryStartQueueProcessing();
    }

    public void ShowPopupValueRegen(float value, Color startColor, Color endColor)
    {
        int intValue = value > 0 ? Mathf.CeilToInt(value) : Mathf.FloorToInt(value);
        if (intValue == 0 && value != 0)
            intValue = value > 0 ? 1 : -1;

        string text = (intValue > 0 ? "+" : "-") + intValue;
        popupQueue.Enqueue(new PopupRequest(text, startColor, endColor, RegenSpawn));
        TryStartQueueProcessing();
    }

    public void ShowPopupTextRegen(string text, Color startColor, Color endColor)
    {
        popupQueue.Enqueue(new PopupRequest(text, startColor, endColor, RegenSpawn));
        TryStartQueueProcessing();
    }

    private void TryStartQueueProcessing()
    {
        if (!isProcessingQueue)
            StartCoroutine(ProcessPopupQueue());
    }

    private IEnumerator ProcessPopupQueue()
    {
        isProcessingQueue = true;

        while (popupQueue.Count > 0)
        {
            var popupData = popupQueue.Dequeue();
            SpawnPopup(popupData);

            yield return new WaitForSeconds(popupSpawnDelay);
        }

        isProcessingQueue = false;
    }

    private void SpawnPopup(PopupRequest request)
    {
        var popup = Instantiate(PopupText, request.SpawnPoint.position, Quaternion.identity, transform);
        popup.PopupText.text = request.Text;
        popup.StartColor = request.StartColor;
        popup.EndColor = request.EndColor;
    }

    private void OnHealTaked(float healValue, Skill skill, string sourceName)
    {
        ShowPopupValueRegen(healValue, _regenColor, _regenColor);
    }

    private void OnDamageTaken(Damage damage, Skill skill)
    {
        ShowPopupValue(-damage.Value, _physDamageColor, _physDamageColor);
    }

    private void OnShieldDamageTaken(float damageTaken, DamageType damageType, Skill skill)
    {
        ShowPopupValue(-damageTaken, _shieldColor, _shieldColor);
    }

    private void OnShieldAdded(float shieldValue)
    {
        ShowPopupValue(shieldValue, _shieldColor, _shieldColor);
    }

    private void OnStartStreaming(float time)
    {
        _castLine.gameObject.SetActive(true);
        _castLine.StartFill(time + _fixDuration, 1, 0);
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

    private void OnHealTracked(Heal heal)
    {
        ShowPopupValueRegen(heal.Value, _regenColor, _regenColor);
    }

    private struct PopupRequest
    {
        public string Text;
        public Color StartColor;
        public Color EndColor;
        public Transform SpawnPoint;

        public PopupRequest(string text, Color startColor, Color endColor, Transform spawnPoint)
        {
            Text = text;
            StartColor = startColor;
            EndColor = endColor;
            SpawnPoint = spawnPoint;
        }
    }
}
