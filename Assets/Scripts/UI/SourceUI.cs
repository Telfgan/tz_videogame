using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SourceUI : MonoBehaviour, IGameSourceUI
{
    [SerializeField] private TMP_Text _teamText1;
    [SerializeField] private TMP_Text _teamText2;
    [SerializeField] private Image _winImage;
    [SerializeField] private TMP_Text _winText;


    private string _teamName1 = "Light";
    private string _teamName2 = "Dark";

    private void Start()
    {
        _teamText1.text = _teamName1 + ": " + 0;
        _teamText2.text = _teamName2 + ": " + 0;
    }

    public void SetSource(int teamIndex, int source)
    {
        teamIndex--;

        switch (teamIndex)
        {
            case 0:
                _teamText1.text = _teamName1 + ": " + source;
                break;
            case 1:
                _teamText2.text = _teamName2 + ": " + source;
                break;

            default:
                Debug.LogError("team not founded");
                break;
        }
    }

    /*
    public void SetSource(int teamIndex, int source)
    {
        switch (teamIndex)
        {
            case 1:
                _teamText1.text = _teamName1 + ": " + source;
                break;
            case 2:
                _teamText2.text = _teamName2 + ": " + source;
                break;

            default:
                Debug.LogError("team not founded");
                break;
        }
    }
    */

    public void ShowWinner(int teamIndex)
    {
        teamIndex--;

        _winImage.gameObject.SetActive(true);

        switch (teamIndex)
        {
            case 0:
                _winText.text = _teamName1 + " team WIN";
                break;
            case 1:
                _winText.text = _teamName2 + " team WIN";
                break;

            default:
                Debug.LogError("team not founded");
                break;
        }
    }
}
