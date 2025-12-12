using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerTeam : MonoBehaviour
{
    [SerializeField] private List<GameObject> _towerTeam_1;
    [SerializeField] private List<GameObject> _towerTeam_2;

    public List<GameObject> TowerTeam_1 => _towerTeam_1;
    public List<GameObject> TowerTeam_2 => _towerTeam_2;
}
