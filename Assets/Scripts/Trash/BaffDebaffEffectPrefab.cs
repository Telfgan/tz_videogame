using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaffDebaffEffectPrefab : MonoBehaviour
{
    [SerializeField] private TextMeshPro _tmp;
    [SerializeField] private Image _tmpImage;
    public float Timer = 0f;
    public EffectsType BaffType;
    private Transform parentTransform;
    private Transform effectParent;
    private string parentTag;

    public void StartCountdown(float time)
    {
        Timer = time;
    }

    private void Update()
    {
        if(Timer > 0)
        {
            effectParent = transform.parent;
            parentTransform = effectParent.parent;

            Timer -= Time.deltaTime;

            GetComponent<TextMeshPro>().text = ((int)Timer + 1).ToString();
        }
        if (Timer <= 0)
        {
            Timer = 0;
            Destroy(gameObject);
        }

        if (effectParent != null)
        {
            string effectTag = effectParent.gameObject.tag;
            if (parentTransform != null)
            {
                parentTag = parentTransform.gameObject.tag;
            }

            if (effectTag == "Debaff")
            {
                if (parentTag == "Enemies")
                {
                    GetComponent<TextMeshPro>().color = Color.green;
                }
                else if (parentTag == "Allies")
                {
                    GetComponent<TextMeshPro>().color = Color.red;
                }
            }
            else if (effectTag == "Baff")
            {
                if (parentTag == "Enemies")
                {
                    GetComponent<TextMeshPro>().color = Color.red;
                }
                else if (parentTag == "Allies")
                {
                    GetComponent<TextMeshPro>().color = Color.green;
                }
            }
        }
    }
}

public enum EffectsType
{
    EnergyOfSpirit,
    HealthOfSpirit,
    ProtectBaff,
    ProtectDebaff,
    ReversePolarity,
    HealthRecovery,
    DamageDebaff,
    DarkProtectionDebaff
}
