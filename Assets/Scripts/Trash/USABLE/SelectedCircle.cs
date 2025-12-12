using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SelectedCircle : MonoBehaviour
{
    [SerializeField] private DecalProjector _selectProjector;
    [SerializeField] private DecalProjector _selectProjectorHero;
    [SerializeField] private DecalProjector _selectProjectorTargetVariant;
    [SerializeField] private DecalProjector _stroke;

    private bool _isActive;

    public bool IsActive
    {
        get
        {
            return _isActive;
        }
        set
        {
            _isActive = value;
            _selectProjectorHero.gameObject.SetActive(_isActive);
			//gameObject.SetActive(_isActive);
        }
    }

	public void SwitchClostestTarget(bool value)
    {
        _selectProjectorTargetVariant.gameObject.SetActive(value);
		//_stroke.gameObject.SetActive(value);
    }

    public void SwitchStroke(bool value) 
    {
		_stroke.gameObject.SetActive(value);
	}

    public void SwitchSelectCircle(bool value)
    {
        _selectProjector.gameObject.SetActive(value);
    }

    public void SetColorSelectProjector(Color value)
    {
        var mat = _selectProjector.material;
        if (mat != null)
        {
            mat.color = value;
        }
    }

    public void SetColorTargetVariant(Color value)
    {
        var mat = _selectProjectorTargetVariant.material;
        if (mat != null)
        {
            mat.color = value;
        }
    }

    public void SetColorSelectProjectorHero(Color value)
    {
        var mat = _selectProjectorHero.material;
        if (mat != null)
        {
            mat.color = value;
        }
    }
}
