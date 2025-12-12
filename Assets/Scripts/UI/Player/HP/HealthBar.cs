using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class HealthBar : Bar
{
    /*
    private float _timeToDisapear = 0.2f;

	public override void UpdateBar(float hp, float maxHp)
    {
		_bar.value = hp/maxHp;
        _barText.text = Mathf.RoundToInt(hp).ToString();
		StartCoroutine(DisapearBar(hp, maxHp));
    }

    private IEnumerator DisapearBar(float hp, float maxHp)
    {
		yield return new WaitForSeconds(_timeToDisapear);
		_barMinus.DOValue(hp/maxHp, 0.5f);
	}
    */
}
