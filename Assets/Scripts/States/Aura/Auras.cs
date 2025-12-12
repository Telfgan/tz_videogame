using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Auras : MonoBehaviour
{
    private List<Aura> _auras = new();

    private void Update()
    {
        foreach (var aura in _auras)
            aura.Update();    
    }

    public void AddAura(Aura aura)
    {
        _auras.Add(aura);
    }

    public void RemoveAura(Aura aura)
    {
        _auras.Remove(aura);
    }
}
