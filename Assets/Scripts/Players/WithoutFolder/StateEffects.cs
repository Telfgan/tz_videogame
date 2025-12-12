using System.Collections.Generic;
using UnityEngine;

public class StateEffects : MonoBehaviour
{
    #region Effects
    [Header("Effect Frozen")]
    [SerializeField] private GameObject frozenStateEffect;
    [SerializeField] private AudioClip frozenAudio;

    [Header("Effect Frosting")]
    [SerializeField] private GameObject ice;
    [SerializeField] private AudioClip frostingAudio;

    [Header("Effect Astral")]
    [SerializeField] private Material materialGhost;

    [Header("Effect LightShield")]
    [SerializeField] private GameObject lightShield;

    [Header("Effect DarkShield")]
    [SerializeField] private GameObject darkShield;

    [Header("Effect Suppression")]
    [SerializeField] private GameObject suppressionIdle;
    [SerializeField] private GameObject suppressionMove;

    [Header("Effect ManaRegen")]
    [SerializeField] private GameObject manaRegen;

    [Header("Effect Trap")]
    [SerializeField] private GameObject ropeTrap;
    [SerializeField] private GameObject trapPrefab;
    #endregion

    #region Hero
    [Header("Material Character")]
    [SerializeField] private List<Material> materialsCharacter;
    [Header("Weapon Character")]
    [SerializeField] private GameObject weapon;
    #endregion

    #region Property
    public GameObject RopeTrap { get => ropeTrap; set => ropeTrap = value; }
    public GameObject TrapPrefab { get => trapPrefab; set => trapPrefab = value; }
    public GameObject FrozenStateEffect { get => frozenStateEffect; set => frozenStateEffect = value; }
    public GameObject Ice { get => ice; set => ice = value; }
    public GameObject Weapon { get => weapon; set => weapon = value; }
    public GameObject LightShield { get => lightShield; set => lightShield = value; }
    public GameObject DarkShield { get => darkShield; set => darkShield = value; }
    public GameObject SuppressionIdle { get => suppressionIdle; set => suppressionIdle = value; }
    public GameObject SuppressionMove { get => suppressionMove; set => suppressionMove = value; }
    public GameObject ManaRegen { get => manaRegen; set => manaRegen = value; }
    public List<Material> MaterialsCharacter { get => materialsCharacter; set => materialsCharacter = value; }
    public Material MaterialGhost { get => materialGhost; set => materialGhost = value; }
    public AudioClip FrostingAudio { get => frostingAudio; set => frostingAudio = value; }
    public AudioClip FrozenAudio { get => frozenAudio; set => frozenAudio = value; }
    #endregion


    private void Awake()
    {
        if (materialsCharacter != null) foreach (var materialCharacter in materialsCharacter) materialCharacter.color = Color.white;
    }
}
