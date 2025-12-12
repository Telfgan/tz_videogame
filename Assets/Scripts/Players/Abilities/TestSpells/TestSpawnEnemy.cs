using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawnEnemy : MonoBehaviour
{
    public HeroComponent Hero;

    private void Awake()
    {
        Hero = GetComponent<HeroComponent>();
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Hero.SpawnComponent.CmdSpawnUnitEnemy(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            //CmdSpawnUnitAlies();
            //Hero.SpawnComponent.CmdSpawnUnitAlies();
        }
    }
}
