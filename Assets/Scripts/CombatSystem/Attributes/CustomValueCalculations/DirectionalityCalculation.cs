using UnityEngine;

[CreateAssetMenu(menuName="CombatSystem/CustomCalculation/DirectionalityCalculation")]
public class DirectionalityCalculation : CustomValueCalculation
{
    [SerializeField] float scalar;
    [SerializeField] private bool scaleX;
    public override float Calculate(CombatSystem source, CombatSystem target)
    {
        Vector2 dir = target.transform.position - source.transform.position;
        dir = dir.normalized;
        if (scaleX)
        {
            return dir.x * scalar;            
        }

        return dir.y * scalar;
    }
}
