using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreparationAreaManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> _preparationAreas;

    private Coroutine preparationAreasDisable;

    public void PreparationAreasDisable(float time)
    {
        if (preparationAreasDisable != null)
        {
            StopCoroutine(IPreparationAreasDisable(time));
            preparationAreasDisable = null;
        }

        preparationAreasDisable = StartCoroutine(IPreparationAreasDisable(time));
    }

    private IEnumerator IPreparationAreasDisable(float time)
    {
        foreach (GameObject preparationArea in _preparationAreas) preparationArea.SetActive(true);
        yield return new WaitForSeconds(time);
        foreach (GameObject preparationArea in _preparationAreas) preparationArea.SetActive(false);
    }
}
