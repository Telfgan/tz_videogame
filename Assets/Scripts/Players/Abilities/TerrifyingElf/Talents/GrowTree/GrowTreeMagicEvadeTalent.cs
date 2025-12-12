using UnityEngine;

public class GrowTreeMagicEvadeTalent : Talent
{
    [SerializeField] private GrowTree growTree;

    public override void Enter()
    {
        growTree.treeMagicEvadeTalentActive(true);
    }

    public override void Exit()
    {
        growTree.treeMagicEvadeTalentActive(false);
    }
}
