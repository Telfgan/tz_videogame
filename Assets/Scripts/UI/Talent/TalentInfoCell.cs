using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalentInfoCell : MonoBehaviour
{
    [SerializeField] private TMP_Text _textDescription;
    [SerializeField] private Image _image;

    public TMP_Text TextDescription { get => _textDescription; }

    public void ShowDividingLine()
    {
        _image.gameObject.SetActive(true);
    }

    public void HideDividingLine()
    {
        _image.gameObject.SetActive(false);
    }
}
