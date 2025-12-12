using TMPro;
using UnityEngine;

public class PopupTextPrefab : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro _popupText;
    [SerializeField]
    private float _speed = 2;
    [SerializeField]
    private float _duration = 0.75f;

    private float _startTime;
    private Camera _mainCamera;

    public TextMeshPro PopupText => _popupText;
    public Color StartColor;
    public Color EndColor;

    private void Awake()
    {
        _startTime = Time.time;
        _mainCamera = Camera.main;
    }

    void Update()
    {
        transform.position += Vector3.up * (_speed * Time.deltaTime);

        if (_mainCamera != null) transform.rotation = Quaternion.LookRotation(transform.position - _mainCamera.transform.position);

        float elapsedTime = Time.time - _startTime;

        if (elapsedTime < _duration)
        {
            float lerpValue = elapsedTime / _duration;
            _popupText.color = Color.Lerp(StartColor, EndColor, lerpValue);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
