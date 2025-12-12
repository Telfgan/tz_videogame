using System;
using UnityEngine;

public class TestController : MonoBehaviour
{
    [SerializeField, Range(0, 0.5f)] private float _smoothTime = 0.15f;
	[SerializeField] private Camera _camera;
	[SerializeField] private Rigidbody _rigidbody;
	[SerializeField] private Animator _anim;
	[SerializeField] private float _currentSpeed = 5;

	public Vector3 MoveDirection = Vector3.zero;
	public bool CanMove = false;
	public bool IsMoving = false;
	public bool IsSelect = true;

	private Vector3 _offset = Vector3.zero;

	private bool _isHero = false;

	private float _defaultSpeed = 5;

	private Vector3 _dir;
	private Vector3 _currentVelocity;
	private Vector3 _currentVelocityTemp;

    public Vector3 CurrentVelocity { get => _currentVelocity; set => _currentVelocity = value; }

    private void Awake()
    {
		Initialize();
    }

    public void SetOffset(Vector2 offset)
	{
		_offset = offset;
	}

	public void Initialize()
	{
		_rigidbody.isKinematic = false;

		SetDefaultSpeed();

		MoveDirection = Vector2.zero;

		CanMove = true;

		InputHandler.OnPlayerMove += OnMove;
	}

	private void OnDestroy()
	{
		InputHandler.OnPlayerMove -= OnMove;
	}

	public void ChangeMoveSpeed(float value)
	{
		_currentSpeed *= value;
	}
	public void SetMoveSpeed(float speed)
	{
		_currentSpeed = speed;
	}
	public void SetDefaultSpeed()
	{
		_currentSpeed = _defaultSpeed;
	}

	void Update()
	{
		if (!CanMove || _rigidbody == null)
		{
			return;
		}

        if (Input.GetKey(KeyCode.B))
        {
            try
            {
				_anim.SetTrigger(0);
			}
			catch (Exception e)
            {
				Debug.Log("eeeeee");
            }
			
        }

		_currentVelocity = Vector3.SmoothDamp(_currentVelocity, _dir, ref _currentVelocityTemp, _smoothTime); // Move from camera

		var camDir = _camera.transform.TransformDirection(_currentVelocity);

		camDir = Quaternion.AngleAxis(-_camera.transform.eulerAngles.x, _camera.transform.TransformVector(Vector3.right)) * camDir;

		_rigidbody.linearVelocity = camDir * _currentSpeed;


		
		Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
		Debug.DrawRay(_camera.transform.position, ray.direction);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit))
        {
			var transformRotate = transform.eulerAngles;
			transform.LookAt(hit.point);
			transform.eulerAngles = (new Vector3(transformRotate.x, transform.eulerAngles.y, transformRotate.z));
		}


		var animDir = transform.InverseTransformPoint(transform.position + camDir);
		_anim.SetFloat("Y", animDir.z);
		_anim.SetFloat("X", animDir.x);
	}

	private void OnMove(Vector2 dir)
	{
		_dir = new Vector3 (dir.x, 0, dir.y);
	}

	public void TargetRpcAddForce(Vector2 vector2)
	{
		_rigidbody.AddForce(vector2);
	}

	public void TargetRpcAddTransformPosition(Vector3 vector3)
	{
		transform.position += vector3;
	}

	public void TargetRpcSetTransformPosition(Vector3 vector3)
	{
		transform.position = vector3;
	}
}
