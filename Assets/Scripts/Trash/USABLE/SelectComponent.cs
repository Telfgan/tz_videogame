using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SelectComponent : NetworkBehaviour
{
    public static List<SelectComponent> Units = new();
    
    private MoveComponent _moveComponent;
    private SkillManager _abilitiesComponent;
    private UIPlayerComponents _uiComponent;
    private SkillRenderer _skillRenderer;

    private bool _isCurrentPLayer;

    public Vector3 OffsetInGroup { get; set; }

    public event Action OnSelect;
    public event Action OnDeselect;

    public bool IsCurrentPlayer
    {
        get => _isCurrentPLayer;
        set
        {
            _isCurrentPLayer = value;
            if(_isCurrentPLayer) _abilitiesComponent.SetAbilitiesPanelEnable();
        }
    }

    [Client]
    private void Awake()
    {
        Units.Add(this);
    }

    [Client]
    private void OnDestroy()
    {
        Units.Remove(this);
        if (isOwned) FindObjectOfType<SelectManager>()?.Deselect(GetComponent<Character>());
    }

    public void Initialize(MoveComponent move, SkillManager abilitiesComponent,UIPlayerComponents uiComponent)
    {
        _moveComponent = move;
        _abilitiesComponent = abilitiesComponent;
        _uiComponent = uiComponent;
        _skillRenderer = _uiComponent.Renderer;
    } 
    
    [Client] 
    public void Select()
    {
        if (!isOwned) return;
        
        _uiComponent.ChangeSelection(true);
        _abilitiesComponent.SetAbilitiesPanelSelect(true);
        _abilitiesComponent.OnSelect(true);
        _moveComponent.SetOffset(OffsetInGroup);
        _moveComponent.IsSelect = true;

        OnSelect?.Invoke();
        _skillRenderer.StartHoverHighlight();
        _uiComponent.CircleSelect1.SetColorSelectProjectorHero(Color.green);
    }
    [Client]
    public void Deselect()
    {
        if(!isOwned) return;

        _uiComponent.ChangeSelection(false);
        _abilitiesComponent.SetAbilitiesPanelSelect(false);
        _abilitiesComponent.OnSelect(false);
        _moveComponent.SetOffset(OffsetInGroup);
        _moveComponent.IsSelect = false;

        OnDeselect?.Invoke();
        _skillRenderer.StopHoverHighlight();
        _uiComponent.CircleSelect1.SetColorSelectProjectorHero(Color.red);
    }
    
}