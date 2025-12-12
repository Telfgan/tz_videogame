using UnityEngine;

namespace Players.Abilities.Carrygun
{
	public class FindTentaclePrefabInRadius : MonoBehaviour
	{
		private DrawCircle _drawCircle;
		private float _setRadiusCircle;
		private string _tentaclePrefabTag = "TentaclePrefab";

		public float SetRadiusCircle
		{
			get => _setRadiusCircle;
			set => _setRadiusCircle = value;
		}

		private void Start()
		{
			_drawCircle = GetComponent<DrawCircle>();
		}

		private void Update()
		{
			FindEnemy(_setRadiusCircle);
		}

		private void FindEnemy(float _radiusCircle)
		{
			Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _radiusCircle);
			bool tentacleFound = false;

			foreach (Collider2D collider in colliders)
			{
				if (collider.CompareTag(_tentaclePrefabTag))
				{
					_drawCircle.SetColor(Color.green);
					tentacleFound = true;
					break;
				}
			}

			if (!tentacleFound)
			{
                _drawCircle.SetColor(Color.red);
            }
		}
	}
}