using TMPro;
using UnityEngine;

public class TMProLocalizer: MonoBehaviour
{
    [SerializeField]
    private TMP_Text _tmpText;

    public void Localize(params object[] args)
    {
        string result = _tmpText.text;
        
        for (int i = 0; i < args.Length; i++)
        {
            string placeholder = "{" + i + "}";
            result = result.Replace(placeholder, args[i].ToString());
        }
        
        _tmpText.text = result;
    }

    public void ChangeKey(int placeHolder , int currentValue)
    {
        _tmpText.text = _tmpText.text.Replace(placeHolder.ToString(), currentValue.ToString());
    }
    
    public void ChangeKey(int currentValue)
    {
        _tmpText.text = currentValue.ToString();
    }
}
