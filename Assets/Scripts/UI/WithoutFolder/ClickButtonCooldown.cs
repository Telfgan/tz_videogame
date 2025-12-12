using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClickButtonCooldown : MonoBehaviour
{
    public GameObject Prefab;
    public GameObject PrefabCursor;
    public Transform Player;
    public float TimeCooldown;

    private GameObject _newPrefab;
    private GameObject _newPrefabCursor;
    private Vector2 _newVectorCursor;
    private Vector2 _targetPosition;
    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();
    }
    private void Update()
    {
        if( TimeCooldown != 0)
        {
            StartCoroutine(ChangeFillAmountOverTime(1f, 0f, TimeCooldown));
        }
        if(_newPrefabCursor != null)
        {
            _targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
             _newVectorCursor = new Vector2(_targetPosition.x, _targetPosition.y - 0.3f);
            _newPrefabCursor.transform.position = _newVectorCursor;
        }
    }
    public void CreatePrefab()
    {
        Vector2 newVector = new Vector2(Player.position.x, Player.position.y - 1);
        if(_newPrefab != null)
        {
            Destroy(_newPrefab);
        }
        _newPrefab = Instantiate(Prefab, newVector, Quaternion.identity);
        _newPrefab.GetComponentInChildren<TextMeshPro>().text = GetComponentInChildren<TextMeshPro>().text;
        _newPrefab.transform.SetParent(Player.transform);

        if (_newPrefabCursor != null)
        {
            Destroy(_newPrefabCursor);
        }
        _targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _newVectorCursor = new Vector2(_targetPosition.x, _targetPosition.y - 0.7f);

        _newPrefabCursor = Instantiate(PrefabCursor, _newVectorCursor, Quaternion.identity);
        _newPrefabCursor.GetComponent<TextMeshPro>().text = GetComponentInChildren<TextMeshPro>().text;

    }
    IEnumerator ChangeFillAmountOverTime(float startValue, float endValue, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            image.fillAmount = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image.fillAmount = endValue;
    }
}
