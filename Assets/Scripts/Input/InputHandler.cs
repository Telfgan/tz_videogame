using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
	public static InputHandler Instance;

	private string[] _keySpell = new string[16];
	
	private Vector2 _movementVector;
	private StandardInput _inputActions;
	private bool _IsClick;

	public Vector2 MovementVector => _movementVector;

	public StandardInput InputActions { get => _inputActions; set => _inputActions = value; }

	#region Events

	public static bool Click { get => Instance._IsClick; }

	public static UnityAction<Vector2> OnPlayerMove = delegate { };
	public static UnityAction OnClick = delegate { };
	public static UnityAction OnClickCanceled = delegate { };
	public static UnityAction OnAltClick = delegate { };
	public static UnityAction OnDoubleAltClick = delegate { };
	public static UnityAction OnSwitchAutoMode = delegate { };
	public static UnityAction OnSwitchAutoModeCanceled = delegate { };
	public static UnityAction OnShiftLeftMouse = delegate { };
	public static UnityAction OnShiftLeftMouseCanceled = delegate { };
	public static UnityAction OnSpacetLeftMouse = delegate { };
	public static UnityAction OnSpacetLeftMouseCanceled = delegate { };
	public static UnityAction<float> ScrollMouse = delegate { };
	public static UnityAction ShowMenu = delegate { };
	public static UnityAction ShowSource = delegate { };

	public static UnityAction OnFirstAbility = delegate { };
	public static UnityAction OnSecondAbility = delegate { };
	public static UnityAction OnThirdAbility = delegate { };
	public static UnityAction OnFourthAbility = delegate { };
	public static UnityAction OnFifthAbility = delegate { };
	public static UnityAction OnSixthAbility = delegate { };
	public static UnityAction OnSeventhAbility = delegate { };
	public static UnityAction OnEighthAbility = delegate { };

	public static UnityAction<int> OnFirstCast = delegate { };
	public static UnityAction<int> OnSecondCast = delegate { };
	public static UnityAction<int> OnThirdCast = delegate { };
	public static UnityAction<int> OnFourthCast = delegate { };
	public static UnityAction<int> OnFifthCast = delegate { };
	public static UnityAction<int> OnSixthCast = delegate { };
	public static UnityAction<int> OnSeventhCast = delegate { };
	public static UnityAction<int> OnEighthCast = delegate { };

	public static UnityAction<int> OnCast = delegate { };
	#endregion

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(Instance);

		_inputActions = new StandardInput();


		//Debug.Log(_inputActions.GameplayMap.Spell3.bindings[0].path);

		#region Events Listeners

		_inputActions.GameplayMap.Move.performed += i => OnPlayerMove?.Invoke(i.ReadValue<Vector2>());
		_inputActions.GameplayMap.Click.performed += i => 
		{
			if (EventSystem.current == null)
			{
				OnClick?.Invoke();
				return;
			}

			PointerEventData eventData = new(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new();

            EventSystem.current.RaycastAll(eventData, results);

            if (results.Count > 0)
                return;

			OnClick?.Invoke(); 
		};
		_inputActions.GameplayMap.Click.canceled += i => OnClickCanceled?.Invoke();
		_inputActions.GameplayMap.AltClick.performed += i => OnAltClick?.Invoke();
		_inputActions.GameplayMap.AltDoubleClick.performed += i => OnDoubleAltClick?.Invoke();
		_inputActions.GameplayMap.SwitchAutoMode.performed += i => OnSwitchAutoMode?.Invoke();
		_inputActions.GameplayMap.SwitchAutoMode.canceled += i => OnSwitchAutoModeCanceled?.Invoke();
		_inputActions.GameplayMap.ShiftMouse.performed += i => OnShiftLeftMouse?.Invoke();
		_inputActions.GameplayMap.ShiftMouse.canceled += i => OnShiftLeftMouseCanceled?.Invoke();
		_inputActions.GameplayMap.SpaceMouse.performed += i => OnSpacetLeftMouse?.Invoke();
		_inputActions.GameplayMap.SpaceMouse.canceled += i => OnSpacetLeftMouseCanceled?.Invoke();
		_inputActions.GameplayMap.ScrollMouse.performed += i => ScrollMouse?.Invoke(i.ReadValue<float>());
		_inputActions.GameplayMap.ShowMenu.performed += i => ShowMenu?.Invoke();
		_inputActions.GameplayMap.ShowSource.performed += i => ShowSource?.Invoke();

		// spells 1-2-3-4-5-6-7-8
		_inputActions.GameplayMap.Spell1.performed += i => OnFirstAbility?.Invoke();
		_inputActions.GameplayMap.Spell2.performed += i => OnSecondAbility?.Invoke();
		_inputActions.GameplayMap.Spell3.performed += i => OnThirdAbility?.Invoke();
		_inputActions.GameplayMap.Spell4.performed += i => OnFourthAbility?.Invoke();
		_inputActions.GameplayMap.Spell5.performed += i => OnFifthAbility?.Invoke();
		_inputActions.GameplayMap.Spell6.performed += i => OnSixthAbility?.Invoke();
		_inputActions.GameplayMap.Spell7.performed += i => OnSeventhAbility?.Invoke();
		_inputActions.GameplayMap.Spell8.performed += i => OnEighthAbility?.Invoke();

		_inputActions.GameplayMap.Spell1.performed += i => OnFirstCast?.Invoke(0);
		_inputActions.GameplayMap.Spell2.performed += i => OnSecondCast?.Invoke(1);
		_inputActions.GameplayMap.Spell3.performed += i => OnThirdCast?.Invoke(2);
		_inputActions.GameplayMap.Spell4.performed += i => OnFourthCast?.Invoke(3);
		_inputActions.GameplayMap.Spell5.performed += i => OnFifthCast?.Invoke(4);
		_inputActions.GameplayMap.Spell6.performed += i => OnSixthCast?.Invoke(5);
		_inputActions.GameplayMap.Spell7.performed += i => OnSeventhCast?.Invoke(6);
		_inputActions.GameplayMap.Spell8.performed += i => OnEighthCast?.Invoke(7);

		_inputActions.GameplayMap.Spell1.performed += i => OnCast?.Invoke(0);
		_inputActions.GameplayMap.Spell2.performed += i => OnCast?.Invoke(1);
		_inputActions.GameplayMap.Spell3.performed += i => OnCast?.Invoke(2);
		_inputActions.GameplayMap.Spell4.performed += i => OnCast?.Invoke(3);
		_inputActions.GameplayMap.Spell5.performed += i => OnCast?.Invoke(4);
		_inputActions.GameplayMap.Spell6.performed += i => OnCast?.Invoke(5);
		_inputActions.GameplayMap.Spell7.performed += i => OnCast?.Invoke(6);
		_inputActions.GameplayMap.Spell8.performed += i => OnCast?.Invoke(7);
		_inputActions.GameplayMap.Spell9.performed += i => OnCast?.Invoke(8);
		_inputActions.GameplayMap.Spell10.performed += i => OnCast?.Invoke(9);
		_inputActions.GameplayMap.Spell11.performed += i => OnCast?.Invoke(10);
		_inputActions.GameplayMap.Spell12.performed += i => OnCast?.Invoke(11);
		_inputActions.GameplayMap.Spell13.performed += i => OnCast?.Invoke(12);
		_inputActions.GameplayMap.Spell14.performed += i => OnCast?.Invoke(13);
		_inputActions.GameplayMap.Spell15.performed += i => OnCast?.Invoke(14);
		_inputActions.GameplayMap.Spell16.performed += i => OnCast?.Invoke(15);

		#endregion

		OnClick += SetClickTrue;
		OnClickCanceled += SetClickFalse;
	}

	private void OnEnable()
	{
		_inputActions.Enable();
	}

	private void OnDisable()
	{
		_inputActions.Disable();
	}

	private void SetClickTrue()
	{
		_IsClick = true;
	}

	private void SetClickFalse()
	{
		_IsClick = false;
	}
}