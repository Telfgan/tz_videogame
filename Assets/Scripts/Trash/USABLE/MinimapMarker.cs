using UnityEngine;

public class MinimapMarker : MonoBehaviour
{
    [SerializeField] UserNetworkSettings _userNetworkSettings;
    [SerializeField] SpriteRenderer _markForMinimap;
    [SerializeField] SpriteRenderer _selectMarkForMinimap;

    private bool _isActive;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (this == null || gameObject == null) return;
            _isActive = value;
            _selectMarkForMinimap.gameObject.SetActive(_isActive);
        }
    }

    private void Start()
    {
        OnLayerMaskChanged(_userNetworkSettings.gameObject.layer);
    }

    private void OnEnable()
    {
        _userNetworkSettings.LayerMaskChanged += OnLayerMaskChanged;

        OnLayerMaskChanged(_userNetworkSettings.gameObject.layer);
    }

    private void OnDisable()
    {
        _userNetworkSettings.LayerMaskChanged -= OnLayerMaskChanged;
    }

    private void OnLayerMaskChanged(int obj)
    {
        if (obj == LayerMask.NameToLayer("Enemy"))
        {
            _markForMinimap.color = Color.red;
        }
        else if (obj == LayerMask.NameToLayer("Allies"))
        {
            _markForMinimap.color = Color.green;
        }
    }

}
