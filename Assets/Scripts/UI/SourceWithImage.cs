using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SourceWithImage : MonoBehaviour, IGameSourceUI
{
    [SerializeField] private List<Image> _imagesTeam1 = new List<Image>();
    [SerializeField] private List<Image> _imagesTeam2 = new List<Image>();

    [SerializeField] private Image _winImage;
    [SerializeField] private TMP_Text _winText;


    private string _teamName1 = "Light";
    private string _teamName2 = "Dark";

    public void SetSource(int teamIndex, int source)
    {
        teamIndex--;

        switch (teamIndex)
        {
            case 0:
                _imagesTeam1[--source].color = Color.white;
                break;
            case 1:
                _imagesTeam2[--source].color = Color.white;
                break;

            default:
                Debug.LogError("team not founded");
                break;
        }
    }

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
