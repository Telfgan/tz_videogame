using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class sc_combo_test : MonoBehaviour
{
    public int a_c = 3;
    public int b_c = 3;
    public int c_c = 3;

    public TextMeshProUGUI countArm;
    public TextMeshProUGUI countLeg;
    public TextMeshProUGUI countBlade;

    public ParticleSystem Add;
    public ParticleSystem NoCharges;
    public ParticleSystem True;
    public ParticleSystem False;

    public ComboBar_Prefab bar;

    private List<string> list = new List<string>();
    private void Start()
    {
        StartCoroutine(CD(countArm));
        StartCoroutine(CD(countLeg));
        StartCoroutine(CD(countBlade));

    }
    public void HitArm()
    {
        if(a_c > 0)
        {
            a_c--;
            countArm.text = a_c.ToString();
            Add.Play();
            list.Add("ARM");
            if (list.Count == 3)
            {
                if (list[0] == list[1] && list[1] == list[2])
                {
                    False.Play();
                    list.Clear();
                    return;
                }
                bar.TurnOn(1);
                True.Play();
                list.Clear();
            }
        }
        else
        {
            NoCharges.Play();
        }
    }
    public void HitLeg()
    {
        if (b_c > 0)
        {
            b_c--;
            countLeg.text = b_c.ToString();
            Add.Play();
            list.Add("Leg");
            if(list.Count == 3)
            {
                if (list[0] == list[1] && list[1] == list[2])
                {
                    False.Play();
                    list.Clear();
                    return;
                }
                bar.TurnOn(1);
                True.Play();
                list.Clear();
            }
        }
        else
        {
            NoCharges.Play();
        }
    }
    public void HitBlade()
    {
        if (c_c > 0)
        {
            c_c--;
            countBlade.text = c_c.ToString();
            Add.Play();
            list.Add("Blade");
            if (list.Count == 3)
            {
                if (list[0] == list[1] && list[1] == list[2])
                {
                    False.Play();
                    list.Clear();
                    return;
                }
                bar.TurnOn(1);
                True.Play();
                list.Clear();
            }
        }
        else
        {
            NoCharges.Play();
        }
    }

    private IEnumerator CD(TextMeshProUGUI textField)
    {
        while(true)
        {
            if (textField == countArm)
            {
                while(a_c < 3)
                {
                    yield return new WaitForSeconds(9f);
                    a_c++;
                    countArm.text = a_c.ToString();
                }
            }

            if (textField == countLeg)
            {
                while (b_c < 3)
                {
                    yield return new WaitForSeconds(9f);
                    b_c++;
                    countLeg.text = b_c.ToString();
                }
            }

            if (textField == countBlade)
            {
                while (c_c < 3)
                {
                    yield return new WaitForSeconds(9f);
                    c_c++;
                    countBlade.text = c_c.ToString();
                }
            }
            yield return null;

        }
    }
}
