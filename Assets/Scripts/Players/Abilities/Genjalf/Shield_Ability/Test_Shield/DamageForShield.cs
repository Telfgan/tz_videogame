using UnityEngine;

namespace Players.Abilities.Genjalf.Shield_Ability.Test_Shield
{
    public class DamageForShield : MonoBehaviour
    {
        [SerializeField] private float damage = 17f;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                gameObject.GetComponent<TestShield>().DamageInShield(damage);
            }
        }
    }
}