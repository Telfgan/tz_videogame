using UnityEngine;

public abstract class Talent : MonoBehaviour
{
	[SerializeField]
	private TalentData _data;

	public Character character;

	public TalentData Data => _data;

	private void OnValidate()
	{
		_data.Name = GetType().Name;
	}

	public abstract void Enter();

	public abstract void Exit();

	public void SetActive(bool isActive)
	{
		_data.IsOpen = isActive;
	}
}
