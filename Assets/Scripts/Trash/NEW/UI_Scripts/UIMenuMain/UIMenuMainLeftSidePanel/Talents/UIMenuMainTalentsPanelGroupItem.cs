using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMenuMainTalentsPanelGroupItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public event UnityAction<TalentData, bool> Selected;
    public event UnityAction<TalentData> PointerEntered;
    public event UnityAction<TalentData> PointerExited;
    

    [ReadOnly,ShowInInspector]
    public UIMenuMainTalentsPanelGroup Owner;
    
    [SerializeField] private UITwoStates activeState;
    [SerializeField] private Image activeImage;
    [SerializeField] private Image nonActiveImage;
    [SerializeField] private IconState _iconState;
    [SerializeField] private Image _frameImage;
    [SerializeField] private Image _lightingFrameImage;

    [SerializeField] private Button _button;
    
    private TalentData _talent;
    private int _row = 0;


    public int Row => _row;

    public Button Button { get => _button; }
    public TalentData Talent => _talent;

    private void Start()
    {
       // _button.onClick.AddListener(Select);
    }

    public void SetActive()
    {
		_button.onClick.AddListener(Select);
	}

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(Select);
    }

    public void Fill(TalentData talent, int row)
    {
        _row = row;
        activeImage.sprite = talent.Icon;
        nonActiveImage.sprite = talent.Icon;
        _talent = talent;
        
        activeState.isActive = _talent.IsOpen;

        if (_talent.IsOpen == false)
            _frameImage.sprite = _iconState.Off;
        else
            _frameImage.sprite = _iconState.On;
    }
    
    public void Select()
    {
        Debug.Log("Talent selected in talent " +  _talent);
        Selected?.Invoke(_talent, !_talent.IsOpen);
        activeState.isActive = _talent.IsOpen;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEntered?.Invoke(_talent);
        _frameImage.sprite = _iconState.On;
        _lightingFrameImage.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExited?.Invoke(_talent);

        _lightingFrameImage.gameObject.SetActive(false);

        if (_talent.IsOpen == false)
            _frameImage.sprite = _iconState.Off;
    }
}
