using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ParticleFormOnetoAnotherPoint : MonoBehaviour
{
	[SerializeField] private Transform source;  // The source object in 2D (where particles are "pulled" towards)
	[SerializeField] private Transform target;  // The target object in 2D (where particles originate from)
	[SerializeField] private ParticleSystem _particleSystem;  // The particle system
	[SerializeField] private ParticleSystem _particleSystem2;  // The particle system

	private MainModule mainModule;
	private MainModule mainModule2;

	private float frequency = 4.0f;  // How fast the sine wave oscillates
	private float amplitude = 2.0f;  // How strong the sine wave effect is
	public bool useCosine = false;  // Whether to use cosine instead of sine
	public float phaseShift = 1.0f;

	void Start()
	{
		mainModule = _particleSystem.main;
		mainModule2 = _particleSystem2.main;
		var velocityModule = _particleSystem.velocityOverLifetime;
		velocityModule.enabled = true;

		AnimationCurve curve = new AnimationCurve();

		for (float t = 0; t <= 1.0f; t += 0.1f)  // t goes from 0 (start of lifetime) to 1 (end of lifetime)
		{
			// Calculate the value with phase shift
			float value = amplitude * Mathf.Sin((t * Mathf.PI * 2 * frequency) + (phaseShift * t));
			if (useCosine)
			{
				value = amplitude * Mathf.Cos((t * Mathf.PI * 2 * frequency) + (phaseShift * t));
			}
			curve.AddKey(t, value);
		}


		//velocityModule.x = new ParticleSystem.MinMaxCurve(1.0f, curve);
		velocityModule.y = new ParticleSystem.MinMaxCurve(1.0f, curve);  // Uncomment to apply to Y-axis
		// velocityModule.z = new ParticleSystem.MinMaxCurve(1.0f, curve);  // Uncomment to apply to Z-axis
	}
	void Update()
	{
		Vector2 direction = source.position - target.position;
		float distance = direction.magnitude;

		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

		float particleSpeed = mainModule.startSpeed.constant;
		mainModule.startLifetimeMultiplier = distance / particleSpeed;
		mainModule2.startLifetimeMultiplier = distance / particleSpeed;

		_particleSystem.transform.position = target.position - new Vector3(0, 0, 0.1f);
		_particleSystem2.transform.position = target.position - new Vector3(0, 0, 0.1f);



		var velocityModule = _particleSystem.velocityOverLifetime;
		velocityModule.enabled = true;

		AnimationCurve curve = new AnimationCurve();

		for (float t = 0; t <= 1.0f; t += 0.1f)  // t goes from 0 (start of lifetime) to 1 (end of lifetime)
		{
			// Calculate the value with phase shift
			float value = amplitude * Mathf.Sin(((t + phaseShift * Time.deltaTime) * Mathf.PI * 2 * frequency));
			curve.AddKey(t, value);

		}
		//velocityModule.x = new ParticleSystem.MinMaxCurve(1.0f, curve);
		velocityModule.y = new ParticleSystem.MinMaxCurve(1.0f, curve);  // Uncomment to apply to Y-axis
	}
}
