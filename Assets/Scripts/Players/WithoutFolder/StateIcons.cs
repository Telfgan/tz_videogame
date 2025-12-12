using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StateIcons : MonoBehaviour
{

    [SerializeField] private GameObject _spawnPos;
    [SerializeField] private StateIcoItem _template;

    [Header("Data")]
    [SerializeField] private StateIcoDatabase _icoDatabase;

    [Header("Border colors")]
    [SerializeField] private Color _baffColor = new(0.23f, 0.9f, 0.23f);
    [SerializeField] private Color _debaffColor = new(0.9f, 0.15f, 0.15f);
    [SerializeField] private Color _neutralColor = Color.gray;

    private Dictionary<States, StateIcoData> _icoDataDictionary;
    private CharacterState _characterState;
    private List<StateIcoItem> _activeEffects = new List<StateIcoItem>();
    private bool _added = false;

    private void Awake()
    {
        _characterState = GetComponentInParent<CharacterState>();

        _icoDatabase = Resources.Load<StateIcoDatabase>("StateIcoDatabase_Generated");

        _icoDataDictionary = new();
        foreach (var data in _icoDatabase.Entries) if (!_icoDataDictionary.ContainsKey(data.State)) _icoDataDictionary.Add(data.State, data);
    }

    public void ActivateIco(States state, float timeToDecrease, int stack, bool canStack, int maxStackValue = 1)
    {
        foreach (var ico in _activeEffects)
        {
            if (ico.State == state)
            {
                ico.FadeFront.DOKill();
                ico.count = canStack ? Mathf.Min(ico.count + stack, maxStackValue) : 1;
                ico.maxStack = maxStackValue;
                StartProgress(ico, timeToDecrease);
                RefreshText(ico);
                MoveIcoToEnd(_activeEffects.IndexOf(ico));
                return;
            }
        }


        var newIco = Instantiate(_template, _spawnPos.transform);
        newIco.State = state;
        newIco.count = stack;
        newIco.maxStack = maxStackValue;


        if (_icoDataDictionary.TryGetValue(state, out var data))
        {
            if (data.Icon != null) newIco.Icon.sprite = data.Icon;
            newIco.border.color = data.BorderColor == Color.white ? GetBorderColor(state) : data.BorderColor;
        }
        else
        {
            newIco.border.color = GetFallbackColor(state);
        }

        newIco.Text.color = GetTextColor(state);
        StartProgress(newIco, timeToDecrease);
        RefreshText(newIco);


        _activeEffects.Add(newIco);
        MoveIcoToEnd(_activeEffects.Count - 1);
    }

    private Color GetBorderColor(States state)
    {
        if (_characterState == null) return _neutralColor;

        if (_characterState.enumToState.TryGetValue(state, out var stateObj))
        {
            return stateObj.BaffDebaff switch
            {
                BaffDebaff.Baff => _baffColor,
                BaffDebaff.Debaff => _debaffColor,
                _ => _neutralColor
            };
        }
        return _neutralColor;
    }

    private void StartProgress(StateIcoItem ico, float duration)
    {
        ico.currentDuration = duration;
        ico.FadeFront.DOKill();
        ico.FadeFront.fillAmount = 0f;

        ico.FadeFront.DOFillAmount(1f, duration).SetEase(Ease.Linear).OnComplete(() => RemoveOrRestart(ico));
    }

    private void RemoveOrRestart(StateIcoItem ico)
    {
        if (--ico.count > 0)
        {
            RefreshText(ico);
            StartProgress(ico, ico.currentDuration);
        }
        else
        {
            _activeEffects.Remove(ico);
            Destroy(ico.gameObject);
        }
    }

    private void RefreshText(StateIcoItem ico)
    {
        ico.Text.text = ico.count > 1 ? ico.count.ToString() : "";
        ico.Text.gameObject.SetActive(ico.count > 1);
    }

    private Color GetTextColor(States state)
    {
        if (_characterState == null) return _neutralColor;

        if (_characterState.enumToState.TryGetValue(state, out var stateObj))
        {
            return stateObj.BaffDebaff switch
            {
                BaffDebaff.Baff => _baffColor,
                BaffDebaff.Debaff => _debaffColor,
                _ => _neutralColor
            };
        }
        return _neutralColor;
    }
    private Color GetFallbackColor(States state)
    {
        if (_characterState == null || !_characterState.enumToState.TryGetValue(state, out var stateObj)) return _neutralColor;


        return stateObj.BaffDebaff switch
        {
            BaffDebaff.Baff => _baffColor,
            BaffDebaff.Debaff => _debaffColor, _ => _neutralColor
        };
    }

    //removing item before it ends
    public void RemoveItemByState(States state)
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            if (_activeEffects[i].State == state)
            {
                Destroy(_activeEffects[i].gameObject);
                _activeEffects.RemoveAt(i);
            }
        }
    }

    public void RemoveIconCount()
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            if (_activeEffects[i].count > 0)
            {
                _activeEffects[i].count -= 1;
                _activeEffects[i].Text.text = _activeEffects[i].count.ToString();
                break;
            }
        }
    }

    public void DeactivateIcon()
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            _activeEffects[i].FadeFront.fillAmount = 0;
            Destroy(_activeEffects[i].gameObject);
            _activeEffects.RemoveAt(i);
            break;
        }
    }

    private void MoveIcoToEnd(int index)
    {
        if (index < 0 || index >= _activeEffects.Count) return;
        var ico = _activeEffects[index];
        _activeEffects.RemoveAt(index);
        _activeEffects.Add(ico);
        ico.transform.SetAsLastSibling();
    }

    public void DeactivateAll()
    {
        foreach (var ico in _activeEffects)
        {
            Destroy(ico.gameObject);
        }
        _activeEffects.Clear();
    }
}
/*
    public void RemoveIconCount()
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            if (_activeEffects[i].count > 0)
            {
                _activeEffects[i].count -= 1;
                _activeEffects[i].Text.text = _activeEffects[i].count.ToString();
                break;
            }
        }
    }

    public void DeactivateIcon()
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            _activeEffects[i].FadeFront.fillAmount = 0;
            Destroy(_activeEffects[i].gameObject);
            _activeEffects.RemoveAt(i);
            break;
        }
    }

   
}*/