using Mirror;

using UnityEngine;

public class HitBoxFIre : NetworkBehaviour
{
    [SerializeField] private ReconnaissanceFireAura reconnaissanceFireAura;

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<ArrowProjectile>(out ArrowProjectile arrow) && reconnaissanceFireAura.StateDark == false) 
            if (arrow.ArrowDark && reconnaissanceFireAura.FireDarkTalent) reconnaissanceFireAura.StateDark = true;
    }
}
