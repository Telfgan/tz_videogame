using UnityEngine;
using UnityEngine.UI;

public class BoxSelector : MonoBehaviour
{
    [SerializeField] private Image _selectionArea;

    private Vector2 _startPosition;
    private Vector2 _endPosition;
    private SelectManager _selectManager;
    private bool _isDrawing;

    public void SetSelectManager(SelectManager selectManager)
    {
        _selectManager = selectManager;
    }

    public void StartDraw()
    {
        _isDrawing = true;
        _startPosition = Input.mousePosition;
    }
    public void Draw()
    {
        _endPosition = Input.mousePosition;
        
        Vector2 min = Vector2.Min(_startPosition, _endPosition);
        Vector2 max = Vector2.Max(_startPosition, _endPosition);

        _selectionArea.rectTransform.anchoredPosition = min;

        Vector2 size = max - min;
        _selectionArea.rectTransform.sizeDelta = size;
    }

    public void StopDraw()
    {
        if (!_isDrawing)
        {
            _selectManager.DeselectAll();
            return;
        }
        
        _selectManager.DeselectAll();

        Vector2 min = Vector2.Min(_startPosition, _endPosition);
        Vector2 max = Vector2.Max(_startPosition, _endPosition);
        Vector2 size = max - min;

        Rect rect = new Rect(min, size);

        foreach (SelectComponent unit in SelectComponent.Units)
        {
            if (unit == null) continue;

            Vector2 positionInScreen = Camera.main.WorldToScreenPoint(unit.transform.position);

            if (rect.Contains(positionInScreen))
            {
                var character = unit.GetComponent<Character>();
                _selectManager.SelectInArea(character);
            }
        }
        
        gameObject.SetActive(false);
        
        _isDrawing = false;
    }
}
