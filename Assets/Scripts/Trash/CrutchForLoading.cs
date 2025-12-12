using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrutchForLoading : MonoBehaviour
{
	[SerializeField] private Character _character;

	[Client]
	private void Awake()
	{
		SceneManager.sceneLoaded += OnLoaded;
	}

	[Client]
	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnLoaded;
	}

	private void OnLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		if (this == null) return;
		StartCoroutine(LoadedJob());
	}

	private IEnumerator LoadedJob()
    {
		yield return new WaitForFixedUpdate();
		if (this != null && transform != null && _character != null)
		{
			transform.position = transform.position + Vector3.up;
			_character.Rigidbody.isKinematic = false;
			SceneManager.sceneLoaded -= OnLoaded;
		}
		yield return null;
	}
}
