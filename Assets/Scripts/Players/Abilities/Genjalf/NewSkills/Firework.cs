using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Gangdollarff
{
    public class Firework : MonoBehaviour
    {
        private List<Collider> _damageables = new();

        public List<Collider> Damageables { get => _damageables; protected set => _damageables = value; }

        public void SortDamageablesByDistance(Vector3 position)
        {
            _damageables = _damageables
                .OrderBy(collider => Vector3.Distance(collider.transform.position, position))
                .ToList();
        }

        private void OnDisable()
        {
            _damageables.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform != transform.parent.parent.transform)
                _damageables.Add(other); 
        }

        private void OnTriggerExit(Collider other)
        {
            _damageables.Remove(other);
        }
    }
}
