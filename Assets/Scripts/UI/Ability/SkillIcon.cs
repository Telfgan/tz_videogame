using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillIcon : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image _boxFrame;
    [SerializeField] private TMP_Text _key;

    private int _index;
    private DraggableIcon _currentIcon;

    public event Action<int, Skill> CurrentSkillChenged;

    public DraggableIcon CurrentIcon 
    {
        get => _currentIcon;
        set
        {
            if (value == null)
            {
                _currentIcon = value;

                Deselected();

                CurrentSkillChenged?.Invoke(_index, null);
                return;
            }
            else if (_currentIcon == null)
            {
                _currentIcon = value;
                if (_currentIcon.Selected)
                    Selected();
                else
                    Deselected();
            }
            else
            {
                _currentIcon = value;
                if (_currentIcon.Selected)
                    Selected();
                else
                    Deselected();
            }
            CurrentSkillChenged?.Invoke(_index, _currentIcon.Skill);
        }
    }

    public TMP_Text Key { get => _key; }
    public int Index { get => _index; }

    public void Init(int index)
    {
        _index = index;
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        DraggableIcon draggableIcon = dropped.GetComponent<DraggableIcon>();

        if(CurrentIcon == null)    
        {
            draggableIcon.PatentAfterDrag = transform;
            CurrentIcon = draggableIcon;
        }
        else 
        {
            CurrentIcon.PatentAfterDrag = draggableIcon.PatentAfterDrag;
            CurrentIcon.OnEndDrag(null);
            draggableIcon.PatentAfterDrag = transform;
            CurrentIcon = draggableIcon;
        }
    }

    public void Selected()
    {
        if (_currentIcon != null)
        {
            _boxFrame.color = Color.green;
            _currentIcon.Selected = true;
        }
    }

    public void Deselected()
    {
        _boxFrame.color = Color.white;

        if (_currentIcon != null)
            _currentIcon.Selected = false;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
