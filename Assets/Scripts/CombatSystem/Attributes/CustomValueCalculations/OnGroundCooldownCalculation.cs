using UnityEngine;

[CreateAssetMenu(menuName="CombatSystem/CustomCalculation/InverseCalculation")]
public class OnGroundCooldownCalculation : CustomValueCalculation
{
    [SerializeField]
    AttributeType attribute;
    public override float Calculate(CombatSystem source, CombatSystem target)
    {
        return 1f / source.GetAttributeSet().GetCurrentAttributeValue(attribute);
    }
}
