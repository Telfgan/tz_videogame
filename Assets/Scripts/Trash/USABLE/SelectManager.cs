using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class SelectManager : MonoBehaviour
{
    [SerializeField] private BoxSelector _dragBox;
    private NetworkComponent _contoller;

    private List<Character> _canContollUnits = new List<Character>();

    public List<Character> SelectedControllableUnits { get; } = new();

    private int _currentUnitNumber;

    public event Action<Character> CharacterSelected;
    public event Action<Character> CharacterDeselected;

    private void Awake()
    {
        _dragBox.gameObject.SetActive(false);
        _dragBox.SetSelectManager(this);
    }

    [ClientCallback]
    private void Update()
    {
        if (_contoller == null)
        {
            if (NetworkClient.connection == null || NetworkClient.connection.identity == null)
            {
                return;
            }

            _contoller = NetworkClient.connection.identity.GetComponent<NetworkComponent>();

            if (_contoller == null)
            {
                Debug.LogWarning("NetworkComponent not found on client identity.");
                return;
            }

            _canContollUnits = _contoller.controllableUnits;
        }

        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftAlt))
        {
            _dragBox.gameObject.SetActive(true);
            _dragBox.StartDraw();
        }
        if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftAlt))
        {
            _dragBox.Draw();
        }

        if (Input.GetMouseButtonUp(0) && Input.GetKey(KeyCode.LeftAlt))
        {
            _dragBox.StopDraw();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (SelectedControllableUnits.Count <= 0) return;

            foreach (var unit in SelectedControllableUnits)
            {
                unit.SelectComponent.IsCurrentPlayer = false;
            }

            _currentUnitNumber++;

            if(_currentUnitNumber >= _canContollUnits.Count)
                _currentUnitNumber = 0;

            SelectInArea(_canContollUnits[_currentUnitNumber]);
        }
        /*
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (SelectedControllableUnits.Count <= 0) return;

            foreach (var unit in SelectedControllableUnits)
            {
                unit.SelectComponent.IsCurrentPlayer = false;
            }

            _currentUnitNumber = (_currentUnitNumber + 1) % SelectedControllableUnits.Count;
            SelectedControllableUnits[_currentUnitNumber].SelectComponent.IsCurrentPlayer = true;
        }
        /*
         if (Input.GetKeyDown(KeyCode.Mouse0))
         {
             if (SelectedControllableUnits.Count <= 1) return;

             var center = CalculateCenterPoint();
             bool[] sectorOccupied = new bool[SelectedControllableUnits.Count];

             foreach (var character in SelectedControllableUnits)
             {
                 int sector = DetermineOffset(character.transform.position, center, sectorOccupied, out Vector3 offset);
                 sectorOccupied[sector] = true;
                 character.Move.SetOffset(offset);
             }
         }
         */
    }

    public void SelectOnClick(Character character)
    {
        DeselectAll();

        if (!_canContollUnits.Contains(character)) return;

        SelectedControllableUnits.Add(character);
        character.SelectComponent.Select();
	}

    public void SelectInArea(Character character)
    {
        DeselectAll();
        if (!_canContollUnits.Contains(character)) return;
        
        if (!SelectedControllableUnits.Contains(character))
        {
            SelectedControllableUnits.Add(character);
            character.SelectComponent.Select();
            CharacterSelected?.Invoke(character);
        }
        else
        {
            SelectedControllableUnits.Remove(character);
            character.SelectComponent.Deselect();
            CharacterDeselected?.Invoke(character);
        }

        SelectedControllableUnits.FirstOrDefault()!.SelectComponent.IsCurrentPlayer = true;
    }

    public void Deselect(Character character)
    {
        if (SelectedControllableUnits.Contains(character))
        {
            SelectedControllableUnits.Remove(character);
            CharacterDeselected?.Invoke(character);
        }
    }

    public void DeselectAll()
    {
        SelectedControllableUnits.RemoveAll(c => c == null);

        var snapshot = new List<Character>(SelectedControllableUnits);

        foreach (var character in snapshot)
        {
            if (character == null)
            {
                SelectedControllableUnits.Remove(character);
                continue;
            }

            var selectComponent = character.SelectComponent;
            if (selectComponent != null)
            {
                selectComponent.Deselect();
                CharacterDeselected?.Invoke(character);
            }

            SelectedControllableUnits.Remove(character);
        }
    }

    private Vector3 CalculateCenterPoint()
    {
        Vector3 sum = Vector3.zero;

        foreach (var character in SelectedControllableUnits)
        {
            sum += character.transform.position;
        }

        return sum / SelectedControllableUnits.Count;
    }

    private int DetermineOffset(Vector3 characterPosition, Vector3 centerPoint, bool[] sectorOccupied, out Vector3 offset)
    {
        Vector3 direction = characterPosition - centerPoint;
        float angle = Mathf.Atan2(direction.y, direction.x);

        int sectorCount = sectorOccupied.Length;
        float sectorAngle = 2 * Mathf.PI / sectorCount;
        int sector = Mathf.FloorToInt((angle + Mathf.PI) / sectorAngle);
        int initialSector = sector;

        while (sectorOccupied[sector % sectorCount])
        {
            sector = (sector + 1) % sectorCount;
            if (sector == initialSector)
            {
                offset = Vector3.zero;
                return -1;
            }
        }

        float sectorCenterAngle = sector * sectorAngle - Mathf.PI + sectorAngle / 2;

        offset = new Vector3(Mathf.Cos(sectorCenterAngle), Mathf.Sin(sectorCenterAngle), 0) * 3f;
        return sector;
    }
}
