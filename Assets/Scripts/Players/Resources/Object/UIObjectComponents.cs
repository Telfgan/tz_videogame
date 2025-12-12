using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UIObjectComponents : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private ObjectHealth objectHealth;
    [SerializeField] private SelectedCircle circleSelect;
    [SerializeField] private MinimapMarker minimapMarker;

    [Header("Popup settings")]
    [SerializeField] private Transform damageSpawn;
    [SerializeField] private Transform regenSpawn;
    [SerializeField] private PopupTextPrefab popupPrefab;
    [SerializeField] private float popupSpawnDelay = 0.2f;

    private readonly Color physDamageColor = Color.red;

    private readonly Queue<PopupRequest> popupQueue = new();
    private bool isProcessingQueue;

    #region Unity events

    private void OnEnable()
    {
        if (!objectHealth) return;

        objectHealth.DamageTaken += OnDamageTaken;
    }

    private void OnDisable()
    {
        if (!objectHealth) return;

        objectHealth.DamageTaken -= OnDamageTaken;
    }

    #endregion

    public void ChangeSelection(bool isSelect)
    {
        circleSelect.IsActive = isSelect;
        minimapMarker.IsActive = isSelect;
    }

    private struct PopupRequest
    {
        public readonly string Text;
        public readonly Color StartColor;
        public readonly Color EndColor;
        public readonly Transform Spawn;

        public PopupRequest(string text, Color start, Color end, Transform spawn)
        {
            Text = text;
            StartColor = start;
            EndColor = end;
            Spawn = spawn;
        }
    }

    private void EnqueuePopup(string text, Color start, Color end, Transform spawn)
    {
        popupQueue.Enqueue(new PopupRequest(text, start, end, spawn));
        if (!isProcessingQueue) StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        isProcessingQueue = true;

        while (popupQueue.Count > 0)
        {
            var request = popupQueue.Dequeue();
            SpawnPopup(request);
            yield return new WaitForSeconds(popupSpawnDelay);
        }
        isProcessingQueue = false;
    }

    private void SpawnPopup(PopupRequest request)
    {
        var popup = Instantiate(popupPrefab, request.Spawn.position, Quaternion.identity, transform);
        popup.PopupText.text = request.Text;
        popup.StartColor = request.StartColor;
        popup.EndColor = request.EndColor;
    }

    private static string FormatValue(float value, bool positivePlus = true)
    {
        int i = value > 0 ? Mathf.CeilToInt(value) : Mathf.FloorToInt(value);
        if (i == 0 && !Mathf.Approximately(value, 0)) i = value > 0 ? 1 : -1;
        return (i > 0 && positivePlus ? "+" : "") + i;
    }

    private void OnDamageTaken(Damage damage, Skill _) => EnqueuePopup(FormatValue(-damage.Value, false), physDamageColor, physDamageColor, damageSpawn);
}
