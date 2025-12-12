using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TalentInfoPanel : MonoBehaviour
{
    [SerializeField] private TalentInfoCell _cellPref;
    [SerializeField] private TMP_Text _name;

    private List<TalentInfoCell> _cells = new();

    public void Show(TalentData data)
    {
        gameObject.SetActive(true);
        _name.text = data.Description;

        int descCnt = data.DescriptionsForInfoPanel.Count;
        int stateCnt = data.StateInfos.Count;
        int pairCnt = Mathf.Max(descCnt, stateCnt);
        TalentInfoCell cell = null;

        for (int i = 0; i < pairCnt; i++)
        {
            if (i < descCnt)
            {
                var descCell = Instantiate(_cellPref, transform);
                cell = descCell;
                _cells.Add(descCell);

                descCell.TextDescription.text = data.DescriptionsForInfoPanel[i];
                descCell.ShowDividingLine();
                if (i == pairCnt - 1) descCell.HideDividingLine();
            }

            if (i < stateCnt)
            {
                var st = data.StateInfos[i];

                var stateCell = Instantiate(_cellPref, transform);
                _cells.Add(stateCell);

                stateCell.TextDescription.text =
                    $"<color=#FFFF00>{st.StateName}</color> - {st.Description}";

                cell.HideDividingLine();
                stateCell.ShowDividingLine();
                if (i == pairCnt - 1) stateCell.HideDividingLine();
            }
        }
    }


    public void Hide()
    {
        gameObject.SetActive(false);
        if (_cells == null) return;
        foreach (var cell in _cells) Destroy(cell.gameObject);
        _cells.Clear();
    }
}